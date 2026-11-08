using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GoogleDriveClient
{
    /// <summary>
    /// Основний сервіс для взаємодії з клієнтом Google Drive (Патерн Facade).
    /// </summary>
    public partial class GoogleDriveService
    {
        private readonly CredentialStore _credentialStore;
        private readonly StoragePool<DriveItem> _cachePool;
        private readonly DriveFolder _rootFolder;
        private string? _currentUserId;

        /// <summary>
        /// Поточні облікові дані авторизованого користувача.
        /// </summary>
        public UserCredential? CurrentUserCredential { get; private set; }

        /// <summary>
        /// Коренева папка Google Drive.
        /// </summary>
        public DriveFolder RootFolder => _rootFolder;

        /// <summary>
        /// Локальний пул кешованих об'єктів.
        /// </summary>
        public StoragePool<DriveItem> Cache => _cachePool;

        public GoogleDriveService(CredentialStore credentialStore, int cacheCapacity = 50)
        {
            _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
            _cachePool = new StoragePool<DriveItem>(cacheCapacity);
            _rootFolder = new DriveFolder("root", "My Drive");
        }

        /// <summary>
        /// Здійснює автентифікацію користувача та зберігає його токен.
        /// </summary>
        public void Authenticate(string userId, UserCredential credential)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("ID користувача не може бути порожнім.", nameof(userId));
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));

            _credentialStore.SaveCredentials(userId, credential);
            _currentUserId = userId;
            CurrentUserCredential = credential;
        }

        /// <summary>
        /// Перевіряє наявність активної сесії користувача.
        /// </summary>
        public void CheckAuthorization()
        {
            if (string.IsNullOrEmpty(_currentUserId))
                throw new DriveAuthenticationException("Користувач не автентифікований. Спочатку виконайте вхід.");

            var cred = _credentialStore.LoadCredentials(_currentUserId);
            if (cred == null || cred.IsExpired)
                throw new DriveAuthenticationException("Сесія застаріла або токен не знайдено. Потрібна повторна авторизація.");
        }

        /// <summary>
        /// Завантажує об'єкт DriveItem до кореневої папки та додає в кеш.
        /// </summary>
        public void UploadItem(DriveItem item)
        {
            CheckAuthorization();

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _rootFolder.AddChild(item);
            _cachePool.Add(item);
        }

        /// <summary>
        /// Пошук елемента за його унікальним ID.
        /// </summary>
        public DriveItem GetItem(string id)
        {
            CheckAuthorization();

            var cached = _cachePool.Get(id);
            if (cached != null)
                return cached;

            var found = _rootFolder.Children;
            foreach (var child in found)
            {
                if (child.Id == id)
                {
                    _cachePool.Add(child);
                    return child;
                }
            }

            throw new DriveItemNotFoundException(id);
        }

        /// <summary>
        /// Завантажує новий аудіофайл безпосередньо у Google Drive користувача.
        /// </summary>
        public async Task<Document> UploadAudioTrackAsync(string fileName, byte[] audioData)
        {
            CheckAuthorization();

            await Task.Delay(300); // Симуляція мережевого запиту завантаження

            string fileId = "drive_track_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var trackDoc = new Document(fileId, fileName, $"Audio Track Data ({audioData.Length} bytes)");

            _rootFolder.AddChild(trackDoc);
            _cachePool.Add(trackDoc);

            return trackDoc;
        }

        /// <summary>
        /// Отримує вміст/байтове представлення треку з Google Drive.
        /// </summary>
        public async Task<byte[]> DownloadTrackDataAsync(string trackId)
        {
            CheckAuthorization();

            await Task.Delay(200); // Симуляція мережевого запиту читання

            var item = GetItem(trackId);
            if (item is Document doc)
            {
                return System.Text.Encoding.UTF8.GetBytes(doc.ReadContent());
            }

            throw new DriveItemNotFoundException(trackId);
        }
    }
}