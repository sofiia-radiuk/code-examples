using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GoogleDriveClient;

namespace UniSound.UI
{
    public class TrackModel
    {
        public int Index { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public string Duration { get; set; } = "3:30";
        public long SizeBytes { get; set; }
        public string SizeFormatted => $"{SizeBytes / (1024 * 1024)} MB";
    }

    public partial class MainWindow : Window
    {
        private GoogleDriveService? _driveService;
        private ObservableCollection<TrackModel> _allTracks = new();
        private bool _isPlaying = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadDemoData();
        }

        private void LoadDemoData()
        {
            _allTracks = new ObservableCollection<TrackModel>
            {
                new TrackModel { Index = 1, Title = "Bohemian Rhapsody", Artist = "Queen", Album = "A Night at the Opera", Duration = "5:55", SizeBytes = 14155776 },
                new TrackModel { Index = 2, Title = "Shape of You", Artist = "Ed Sheeran", Album = "÷ (Divide)", Duration = "3:53", SizeBytes = 9332240 },
                new TrackModel { Index = 3, Title = "Blinding Lights", Artist = "The Weeknd", Album = "After Hours", Duration = "3:20", SizeBytes = 8000000 },
                new TrackModel { Index = 4, Title = "Hotel California", Artist = "Eagles", Album = "Hotel California", Duration = "6:30", SizeBytes = 15600000 },
                new TrackModel { Index = 5, Title = "Smells Like Teen Spirit", Artist = "Nirvana", Album = "Nevermind", Duration = "5:01", SizeBytes = 12000000 }
            };

            dgTracks.ItemsSource = _allTracks;
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            _isPlaying = !_isPlaying;
            btnPlayPause.Content = _isPlaying ? "⏸" : "▶";

            if (dgTracks.SelectedItem is TrackModel selectedTrack)
            {
                lblCurrentTitle.Text = selectedTrack.Title;
                lblCurrentArtist.Text = selectedTrack.Artist;
            }
            else if (_allTracks.Count > 0)
            {
                lblCurrentTitle.Text = _allTracks[0].Title;
                lblCurrentArtist.Text = _allTracks[0].Artist;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (dgTracks.SelectedIndex < _allTracks.Count - 1)
            {
                dgTracks.SelectedIndex++;
                UpdateCurrentTrack();
            }
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (dgTracks.SelectedIndex > 0)
            {
                dgTracks.SelectedIndex--;
                UpdateCurrentTrack();
            }
        }

        private void UpdateCurrentTrack()
        {
            if (dgTracks.SelectedItem is TrackModel track)
            {
                lblCurrentTitle.Text = track.Title;
                lblCurrentArtist.Text = track.Artist;
            }
        }

        private void dgTracks_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgTracks.SelectedItem is TrackModel track)
            {
                _isPlaying = true;
                btnPlayPause.Content = "⏸";
                lblCurrentTitle.Text = track.Title;
                lblCurrentArtist.Text = track.Artist;
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(query))
            {
                dgTracks.ItemsSource = _allTracks;
            }
            else
            {
                var filtered = _allTracks.Where(t => 
                    t.Title.ToLower().Contains(query) || 
                    t.Artist.ToLower().Contains(query) || 
                    t.Album.ToLower().Contains(query)).ToList();
                dgTracks.ItemsSource = filtered;
            }
        }

        private void tvPlaylists_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (tvPlaylists.SelectedItem is TreeViewItem item)
            {
                string header = item.Header.ToString() ?? "";
                lblCurrentArtist.Text = $"Плейлист: {header}";
            }
        }

        private void sliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int seconds = (int)(sliderProgress.Value * 2.4); // Симуляція загальних 240 сек
            int mins = seconds / 60;
            int secs = seconds % 60;
            if (lblCurrentTime != null)
            {
                lblCurrentTime.Text = $"{mins}:{secs:D2}";
            }
        }

        private void sliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Обробник зміни гучності
        }

        private void btnOpenImporter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вікно завантажувача з мережі (ImporterWindow) буде відкрито на наступному кроці.", "UniSound", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnOpenTagEditor_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вікно редактора тегів (TagEditorWindow) буде відкрито на наступному кроці.", "UniSound", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnReauth_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow auth = new AuthWindow();
            auth.ShowDialog();
        }
    }
}