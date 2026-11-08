using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UniSound.UI
{
    public class TrackModel : INotifyPropertyChanged
    {
        private int _index;
        private string _title = "";
        private string _artist = "";
        private string _album = "";
        private string _duration = "00:00";
        private string _fileSize = "Cloud";
        private long _sizeBytes;
        private string _audioUrl = "";
        private bool _isCloudTrack = true;
        private string _genre = "Pop";

        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Artist
        {
            get => _artist;
            set { _artist = value; OnPropertyChanged(); }
        }

        public string Album
        {
            get => _album;
            set { _album = value; OnPropertyChanged(); }
        }

        public string Duration
        {
            get => _duration;
            set { _duration = value; OnPropertyChanged(); }
        }

        public string FileSize
        {
            get => _fileSize;
            set { _fileSize = value; OnPropertyChanged(); }
        }

        public long SizeBytes
        {
            get => _sizeBytes;
            set { _sizeBytes = value; OnPropertyChanged(); }
        }

        public string AudioUrl
        {
            get => _audioUrl;
            set { _audioUrl = value; OnPropertyChanged(); }
        }

        public bool IsCloudTrack
        {
            get => _isCloudTrack;
            set { _isCloudTrack = value; OnPropertyChanged(); }
        }

        public string Genre
        {
            get => _genre;
            set { _genre = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}