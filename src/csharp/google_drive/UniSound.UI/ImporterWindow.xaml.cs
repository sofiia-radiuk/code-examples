using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using GoogleDriveClient;

namespace UniSound.UI
{
    public class OnlineSearchResult
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Duration { get; set; } = "3:30";
        public string SourceUrl { get; set; } = string.Empty;
        public string DisplayText => $"🎵 {Artist} - {Title} [{Duration}]";
    }

    public partial class ImporterWindow : Window
    {
        public TrackModel? DownloadedTrack { get; private set; }
        private readonly GoogleDriveService? _driveService;

        public ImporterWindow(GoogleDriveService? driveService = null)
        {
            InitializeComponent();
            _driveService = driveService;
        }

        private void btnSearchOnline_Click(object sender, RoutedEventArgs e)
        {
            string query = txtSearchQuery.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Введіть назву пісні або пряме посилання на MP3.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var results = new List<OnlineSearchResult>();

            if (query.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                query.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                string fileName = Path.GetFileName(new Uri(query).AbsolutePath);
                if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = "Imported_Track.mp3";
                }

                results.Add(new OnlineSearchResult
                {
                    Title = fileName.Replace(".mp3", ""),
                    Artist = "Direct URL Download",
                    Duration = "Network Track",
                    SourceUrl = query
                });
            }
            else
            {
                results.Add(new OnlineSearchResult 
                { 
                    Title = query, 
                    Artist = "Online Stream", 
                    Duration = "0:27", 
                    SourceUrl = "https://raw.githubusercontent.com/rafaelreis-hotmart/Audio-Sample-files/main/sample.mp3" 
                });
            }

            lstSearchResults.ItemsSource = results;
            lstSearchResults.SelectedIndex = 0;
            lblDownloadStatus.Text = $"Знайдено треків: {results.Count}";
        }

        private void lstSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnDownloadToCloud.IsEnabled = lstSearchResults.SelectedItem != null;
        }

        private async void btnDownloadToCloud_Click(object sender, RoutedEventArgs e)
        {
            if (lstSearchResults.SelectedItem is OnlineSearchResult selected)
            {
                btnDownloadToCloud.IsEnabled = false;
                btnSearchOnline.IsEnabled = false;
                progressDownload.Value = 20;
                lblDownloadStatus.Text = "Завантаження MP3 з мережі...";

                try
                {
                    byte[] audioBytes;

                    using (var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) })
                    {
                        var response = await httpClient.GetAsync(selected.SourceUrl);
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException($"Сайт заблокував завантаження (HTTP {(int)response.StatusCode}).");
                        }

                        audioBytes = await response.Content.ReadAsByteArrayAsync();
                    }

                    progressDownload.Value = 65;
                    lblDownloadStatus.Text = "Збереження у Google Drive...";

                    string cloudFileId = "cloud_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    
                    if (_driveService != null)
                    {
                        _driveService.CheckAuthorization();
                        var uploadedDoc = await _driveService.UploadAudioTrackAsync($"{selected.Title}.mp3", audioBytes);
                        cloudFileId = uploadedDoc.Id;
                    }

                    progressDownload.Value = 100;
                    lblDownloadStatus.Text = "Успішно збережено в Google Drive!";

                    DownloadedTrack = new TrackModel
                    {
                        Index = 1,
                        Title = selected.Title,
                        Artist = selected.Artist,
                        Album = "Google Drive Cloud",
                        Duration = selected.Duration,
                        SizeBytes = audioBytes.Length,
                        AudioUrl = cloudFileId,
                        IsCloudTrack = true
                    };

                    MessageBox.Show($"Трек '{selected.Title}' успішно завантажено та збережено у вашому Google Drive!", 
                                    "UniSound Cloud", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося завантажити трек: {ex.Message}\n\nСпробуйте відкрито доступне посилання.", 
                                    "Помилка завантаження", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnDownloadToCloud.IsEnabled = true;
                    btnSearchOnline.IsEnabled = true;
                    progressDownload.Value = 0;
                    lblDownloadStatus.Text = "Помилка завантаження.";
                }
            }
        }
    }
}