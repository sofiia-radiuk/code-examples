using System;
using System.Windows;
using System.Windows.Controls;

namespace UniSound.UI
{
    public partial class TagEditorWindow : Window
    {
        public TrackModel CurrentTrack { get; private set; }

        public TagEditorWindow(TrackModel track)
        {
            InitializeComponent();
            CurrentTrack = track;

            // 1. Заповнюємо текстові поля
            txtTitle.Text = track.Title;
            txtArtist.Text = track.Artist;
            txtAlbum.Text = track.Album;

            // 2. Встановлюємо поточний жанр у ComboBox при відкритті вікна
            SelectGenreInComboBox(track.Genre);
        }

        private void SelectGenreInComboBox(string? genre)
        {
            if (string.IsNullOrEmpty(genre)) return;

            foreach (var item in cmbGenre.Items)
            {
                if (item is ComboBoxItem comboItem && comboItem.Content?.ToString() == genre)
                {
                    cmbGenre.SelectedItem = comboItem;
                    return;
                }
            }
        }

        private void btnSaveTags_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentTrack == null) return;

            // Записуємо нові значення
            CurrentTrack.Title = txtTitle.Text.Trim();
            CurrentTrack.Artist = txtArtist.Text.Trim();
            CurrentTrack.Album = txtAlbum.Text.Trim();

            // Зчитуємо колір/текст жанру з ComboBoxItem або введеного тексту
            if (cmbGenre.SelectedItem is ComboBoxItem selectedItem)
            {
                CurrentTrack.Genre = selectedItem.Content?.ToString() ?? "Pop";
            }
            else if (!string.IsNullOrWhiteSpace(cmbGenre.Text))
            {
                CurrentTrack.Genre = cmbGenre.Text.Trim();
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}