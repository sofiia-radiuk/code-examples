using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GoogleDriveClient;

namespace UniSound.UI
{
    

    public partial class MainWindow : Window
    {
        private GoogleDriveService? _driveService;
        private ObservableCollection<TrackModel> _allTracks = new();
        
        private readonly MediaPlayer _mediaPlayer = new MediaPlayer();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private bool _isPlaying = false;

        public MainWindow()
        {
            InitializeComponent();
            InitAudioPlayer();
            LoadInitialData();

            this.Closed += (s, e) => ClearAudioCache();
        }

        private void InitAudioPlayer()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;

            _mediaPlayer.MediaOpened += (s, e) =>
            {
                if (_mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    sliderProgress.Maximum = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    lblTotalTime.Text = _mediaPlayer.NaturalDuration.TimeSpan.ToString(@"m\:ss");
                }
            };

            _mediaPlayer.MediaFailed += (s, e) =>
            {
                _isPlaying = false;
                btnPlayPause.Content = "▶";
                _timer.Stop();
                MessageBox.Show("Не вдалося відтворити файл. Перевірте формат або завантажте трек знов.", 
                                "UniSound Player", MessageBoxButton.OK, MessageBoxImage.Warning);
            };

            _mediaPlayer.MediaEnded += (s, e) =>
            {
                btnNext_Click(this, new RoutedEventArgs());
            };
        }

        private void LoadInitialData()
        {
            // Порожня медіатека при запуску — нові треки додаються через Google Drive
            _allTracks = new ObservableCollection<TrackModel>();
            dgTracks.ItemsSource = _allTracks;
        }

        private async void PlaySelectedTrack(TrackModel track)
        {
            try
            {
                lblCurrentTitle.Text = track.Title;
                lblCurrentArtist.Text = track.Artist;
                lblTotalTime.Text = track.Duration;

                string audioPath = track.AudioUrl;

                // 1. Відтворення завантаженого з Google Drive треку
                if (track.IsCloudTrack)
                {
                    if (_driveService == null)
                    {
                        MessageBox.Show("Авторизуйтесь у Google Drive (кнопка 'Акаунт') для доступу до хмарних треків.", 
                                        "UniSound Cloud", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string tempFolder = Path.Combine(Path.GetTempPath(), "UniSoundCache");
                    Directory.CreateDirectory(tempFolder);

                    string cachedFilePath = Path.Combine(tempFolder, $"{track.AudioUrl}.mp3");

                    if (!File.Exists(cachedFilePath))
                    {
                        lblCurrentArtist.Text = $"{track.Artist} (Зчитування з Google Drive...)";
                        
                        byte[] cloudData = await _driveService.DownloadTrackDataAsync(track.AudioUrl);
                        await File.WriteAllBytesAsync(cachedFilePath, cloudData);
                        
                        lblCurrentArtist.Text = track.Artist;
                    }

                    audioPath = cachedFilePath;
                }
                // 2. Локальний файл з ПК
                else if (string.IsNullOrEmpty(audioPath) || !File.Exists(audioPath))
                {
                    var dlg = new Microsoft.Win32.OpenFileDialog
                    {
                        Filter = "Аудіофайли (*.mp3;*.wav)|*.mp3;*.wav|Усі файли (*.*)|*.*",
                        Title = $"Вкажіть файл MP3 для '{track.Title}'"
                    };

                    if (dlg.ShowDialog() == true)
                    {
                        track.AudioUrl = dlg.FileName;
                        audioPath = dlg.FileName;
                    }
                    else
                    {
                        return;
                    }
                }

                // 3. Запуск програвання
                _mediaPlayer.Open(new Uri(audioPath, UriKind.Absolute));
                _mediaPlayer.Volume = sliderVolume.Value / 100.0;
                _mediaPlayer.Play();

                _isPlaying = true;
                btnPlayPause.Content = "⏸";
                _timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відтворення: {ex.Message}", "UniSound Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAudioCache()
        {
            try
            {
                _mediaPlayer.Close();
                string tempFolder = Path.Combine(Path.GetTempPath(), "UniSoundCache");
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, recursive: true);
                }
            }
            catch
            {
                // Ігноруємо якщо заблоковано OS
            }
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                _mediaPlayer.Pause();
                _isPlaying = false;
                btnPlayPause.Content = "▶";
                _timer.Stop();
            }
            else
            {
                if (dgTracks.SelectedItem is TrackModel selectedTrack)
                {
                    PlaySelectedTrack(selectedTrack);
                }
                else if (_allTracks.Count > 0)
                {
                    dgTracks.SelectedIndex = 0;
                    PlaySelectedTrack(_allTracks[0]);
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_mediaPlayer.Source != null && _mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                sliderProgress.Value = _mediaPlayer.Position.TotalSeconds;
                lblCurrentTime.Text = _mediaPlayer.Position.ToString(@"m\:ss");
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (dgTracks.SelectedIndex < _allTracks.Count - 1)
            {
                dgTracks.SelectedIndex++;
                if (dgTracks.SelectedItem is TrackModel track)
                {
                    PlaySelectedTrack(track);
                }
            }
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (dgTracks.SelectedIndex > 0)
            {
                dgTracks.SelectedIndex--;
                if (dgTracks.SelectedItem is TrackModel track)
                {
                    PlaySelectedTrack(track);
                }
            }
        }

        private void dgTracks_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgTracks.SelectedItem is TrackModel track)
            {
                PlaySelectedTrack(track);
            }
        }

        



        private void sliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer.Source != null && Math.Abs(_mediaPlayer.Position.TotalSeconds - sliderProgress.Value) > 2.0)
            {
                _mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Volume = sliderVolume.Value / 100.0;
            }
        }

        private void btnOpenImporter_Click(object sender, RoutedEventArgs e)
        {
            ImporterWindow importer = new ImporterWindow(_driveService);
            importer.Owner = this;
            if (importer.ShowDialog() == true && importer.DownloadedTrack != null)
            {
                importer.DownloadedTrack.Index = _allTracks.Count + 1;
                _allTracks.Add(importer.DownloadedTrack);
                
                dgTracks.ItemsSource = null;
                dgTracks.ItemsSource = _allTracks;
                
                lblCurrentArtist.Text = $"Збережено в Google Drive: {importer.DownloadedTrack.Title}";
            }
        }

        private void btnOpenTagEditor_Click(object sender, RoutedEventArgs e)
        {
            if (dgTracks.SelectedItem is TrackModel selectedTrack)
            {
                TagEditorWindow editor = new TagEditorWindow(selectedTrack);
                editor.Owner = this;
                if (editor.ShowDialog() == true)
                {
                    dgTracks.ItemsSource = null;
                    dgTracks.ItemsSource = _allTracks;
                }
            }
            else
            {
                MessageBox.Show("Оберіть трек у таблиці для редагування метаданих.", "Редактор", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
            private void BtnLocalImport_Click(object sender, RoutedEventArgs e)
{
            var importWindow = new LocalImportWindow
            {
                Owner = this
            };

            if (importWindow.ShowDialog() == true && importWindow.ImportedTrack != null)
            {
                var track = importWindow.ImportedTrack;

                _allTracks.Add(track);

                if (_driveService != null && File.Exists(importWindow.SelectedFilePath))
                {
                    byte[] fileBytes = File.ReadAllBytes(importWindow.SelectedFilePath);
                    var driveDoc = new Document(Guid.NewGuid().ToString(), track.Title, Convert.ToBase64String(fileBytes));
                    _driveService.UploadItem(driveDoc);
                }

                dgTracks.ItemsSource = null;
                dgTracks.ItemsSource = _allTracks;

                MessageBox.Show($"Трек '{track.Title}' успішно додано!", "Завантаження", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void btnReauth_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow auth = new AuthWindow();
            if (auth.ShowDialog() == true)
            {
                _driveService = auth.DriveService;
                lblCurrentArtist.Text = "Підключено до Google Drive";
            }
        }
            private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }
        private void ApplyFilters()
        {
            if (_allTracks == null) return;

            string searchText = txtSearch?.Text?.Trim().ToLower() ?? "";
            string selectedCategory = (tvPlaylists?.SelectedItem as TreeViewItem)?.Header?.ToString() ?? "";

            var filteredList = _allTracks.Where(track =>
            {
                // 1. Фільтр за пошуковим рядком (назва або виконавець)
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                    (!string.IsNullOrEmpty(track.Title) && track.Title.ToLower().Contains(searchText)) ||
                                    (!string.IsNullOrEmpty(track.Artist) && track.Artist.ToLower().Contains(searchText));

                // 2. Фільтр за категорією або жанром
                bool matchesCategory = true;

                if (!string.IsNullOrEmpty(selectedCategory))
                {
                    // Якщо обрано "Вся музика", "Улюблені треки" або "Нещодавно додані" — показуємо все
                    if (selectedCategory.Contains("Вся музика") || 
                        selectedCategory.Contains("Улюблені") || 
                        selectedCategory.Contains("Нещодавно"))
                    {
                        matchesCategory = true;
                    }
                    else
                    {
                        // Очищаємо назву жанру від іконок
                        string cleanGenre = selectedCategory.Replace("📁", "").Trim().ToLower();

                        // Перевіряємо за полем Genre, назвою або виконавцем
                        matchesCategory = (!string.IsNullOrEmpty(track.Genre) && track.Genre.ToLower().Contains(cleanGenre)) ||
                                        (!string.IsNullOrEmpty(track.Title) && track.Title.ToLower().Contains(cleanGenre)) ||
                                        (!string.IsNullOrEmpty(track.Artist) && track.Artist.ToLower().Contains(cleanGenre));
                    }
                }

                return matchesSearch && matchesCategory;
            }).ToList();

            dgTracks.ItemsSource = null;
            dgTracks.ItemsSource = filteredList;
        }

        private void TvPlaylists_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ApplyFilters();
        }

    
    }
}