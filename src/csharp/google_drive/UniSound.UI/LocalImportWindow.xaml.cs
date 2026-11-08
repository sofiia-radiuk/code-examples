using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using TagLib;

namespace UniSound.UI
{
    public partial class LocalImportWindow : Window
    {
        public TrackModel? ImportedTrack { get; private set; }
        public string? SelectedFilePath { get; private set; }

        public LocalImportWindow()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Аудіофайли (*.mp3;*.wav;*.m4a)|*.mp3;*.wav;*.m4a|Усі файли (*.*)|*.*",
                Title = "Оберіть аудіофайл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedFilePath = openFileDialog.FileName;
                txtFilePath.Text = SelectedFilePath;

                try
                {
                    // Уточнюємо TagLib.File замість просто File
                    var audioFile = TagLib.File.Create(SelectedFilePath);
                    string? title = audioFile.Tag.Title;
                    string? artist = audioFile.Tag.FirstPerformer;

                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artist))
                    {
                        string rawName = System.IO.Path.GetFileNameWithoutExtension(SelectedFilePath);

                        int underscoreIndex = rawName.IndexOf('_');
                        if (underscoreIndex != -1 && underscoreIndex < rawName.Length - 1)
                        {
                            rawName = rawName.Substring(underscoreIndex + 1);
                        }

                        var parts = rawName.Split('-');
                        if (parts.Length >= 2)
                        {
                            if (string.IsNullOrWhiteSpace(artist)) artist = parts[0].Trim();
                            if (string.IsNullOrWhiteSpace(title)) title = string.Join(" ", parts.Skip(1)).Trim();
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(title)) title = rawName;
                        }
                    }

                    txtTitle.Text = string.IsNullOrWhiteSpace(title) ? System.IO.Path.GetFileNameWithoutExtension(SelectedFilePath) : title;
                    txtArtist.Text = string.IsNullOrWhiteSpace(artist) ? "Невідомий виконавець" : artist;
                }
                catch
                {
                    txtTitle.Text = System.IO.Path.GetFileNameWithoutExtension(SelectedFilePath);
                    txtArtist.Text = "Невідомий виконавець";
                }
            }
        }

        // Ваш працюючий метод обчислення тривалості через MediaPlayer
        private string GetAudioDuration(string filePath)
        {
            try
            {
                var player = new System.Windows.Media.MediaPlayer();
                player.Open(new Uri(filePath));

                int timeout = 0;
                while (!player.NaturalDuration.HasTimeSpan && timeout < 50)
                {
                    System.Threading.Thread.Sleep(20);
                    timeout++;
                }

                if (player.NaturalDuration.HasTimeSpan)
                {
                    TimeSpan ts = player.NaturalDuration.TimeSpan;
                    player.Close();
                    return ts.ToString(@"mm\:ss");
                }
                player.Close();
            }
            catch { }

            return "00:00";
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Уточнюємо System.IO.File замість просто File та перевіряємо SelectedFilePath на null
            if (string.IsNullOrWhiteSpace(SelectedFilePath) || !System.IO.File.Exists(SelectedFilePath))
            {
                MessageBox.Show("Будь ласка, оберіть дійсний аудіофайл на комп'ютері.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введіть назву треку.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Запобігаємо попередженню CS8604 шляхом перевірки вище
            var fileInfo = new System.IO.FileInfo(SelectedFilePath);
            double sizeInMb = fileInfo.Length / (1024.0 * 1024.0);

            ImportedTrack = new TrackModel
            {
                Title = txtTitle.Text.Trim(),
                Artist = string.IsNullOrWhiteSpace(txtArtist.Text) ? "Невідомий виконавець" : txtArtist.Text.Trim(),
                Duration = GetAudioDuration(SelectedFilePath),
                FileSize = $"{sizeInMb:F1} MB",
                SizeBytes = fileInfo.Length,
                AudioUrl = SelectedFilePath,
                IsCloudTrack = false
            };

            DialogResult = true;
            Close();
        }
    }
}