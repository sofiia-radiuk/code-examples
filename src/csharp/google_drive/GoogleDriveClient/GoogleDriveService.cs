using System;

namespace GoogleDriveClient
{
    /// <summary>
    /// Основний сервіс для взаємодії з клієнтом Google Drive (Патерн Facade).
    /// </summary>
    public class GoogleDriveService
    {
        private readonly CredentialStore _credentialStore;
        private readonly StoragePool<DriveItem> _cachePool;
        private readonly DriveFolder _rootFolder;
        private string? _currentUserId;

        public DriveFolder RootFolder => _rootFolder;
        public StoragePool<DriveItem> Cache => _cachePool;

        public GoogleDriveService(CredentialStore credentialStore, int cacheCapacity = 50)
        {
            _credentialStore = credentialStore ?? throw new ArgumentNullException(nameof(credentialStore));
            _cachePool = new StoragePool<DriveItem>(cacheCapacity);
            _rootFolder = new DriveFolder("root", "My Drive");
        }

        public void Authenticate(string userId, UserCredential credential)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("ID користувача не може бути порожнім.", nameof(userId));
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));

            _credentialStore.SaveCredentials(userId, credential);
            _currentUserId = userId;
        }

        public void CheckAuthorization()
        {
            if (string.IsNullOrEmpty(_currentUserId))
                throw new DriveAuthenticationException("Користувач не автентифікований. Спочатку виконайте вхід.");

            var cred = _credentialStore.LoadCredentials(_currentUserId);
            if (cred == null || cred.IsExpired)
                throw new DriveAuthenticationException("Сесія застаріла або токен не знайдено. Потрібна повторна авторизація.");
        }

        public void UploadItem(DriveItem item)
        {
            CheckAuthorization();

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _rootFolder.AddChild(item);
            _cachePool.Add(item);
        }

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
    }
}