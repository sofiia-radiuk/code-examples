using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace UniSound.UI
{
    public class OnlineSearchResult
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Duration { get; set; } = "3:30";
        public string Source { get; set; } = "YouTube Audio Stream";
        public string DisplayText => $"🎵 {Artist} - {Title} [{Duration}] ({Source})";
    }

    public partial class ImporterWindow : Window
    {
        public TrackModel? DownloadedTrack { get; private set; }

        public ImporterWindow()
        {
            InitializeComponent();
        }

        private void btnSearchOnline_Click(object sender, RoutedEventArgs e)
        {
            string query = txtSearchQuery.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Введіть назву пісні або виконавця.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lblDownloadStatus.Text = "Шукаємо треки в мережі...";

            // Симуляція результатів пошуку з відкритих джерел
            var results = new List<OnlineSearchResult>
            {
                new OnlineSearchResult { Title = query, Artist = "Original Artist", Duration = "3:45", Source = "HQ Stream" },
                new OnlineSearchResult { Title = $"{query} (Remix)", Artist = "DJ Cloud", Duration = "4:12", Source = "SoundCloud Stream" },
                new OnlineSearchResult { Title = $"{query} (Live Version)", Artist = "Live Concert 2025", Duration = "5:02", Source = "YouTube Stream" }
            };

            lstSearchResults.ItemsSource = results;
            lblDownloadStatus.Text = $"Знайдено результатів: {results.Count}";
        }

        private void lstSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDownloadToCloud.IsEnabled = lstSearchResults.SelectedItem != null;
        }

        private void btnDownloadToCloud_Click(object sender, RoutedEventArgs e)
        {
            if (lstSearchResults.SelectedItem is OnlineSearchResult selected)
            {
                btnDownloadToCloud.IsEnabled = false;
                btnSearchOnline.IsEnabled = false;
                progressDownload.Value = 30;
                lblDownloadStatus.Text = "Завантаження та обробка аудіопотоку...";

                // Імітація прогресу завантаження
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(500);
                timer.Tick += (s, args) =>
                {
                    progressDownload.Value += 35;
                    if (progressDownload.Value >= 100)
                    {
                        timer.Stop();
                        lblDownloadStatus.Text = "Успішно збережено в Google Drive!";
                        
                        DownloadedTrack = new TrackModel
                        {
                            Index = 6,
                            Title = selected.Title,
                            Artist = selected.Artist,
                            Album = "Cloud Downloads",
                            Duration = selected.Duration,
                            SizeBytes = 10485760
                        };

                        MessageBox.Show($"Трек '{selected.Title}' успішно імпортовано в вашу хмару Google Drive!", "Завантаження", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                };
                timer.Start();
            }
        }
    }
}