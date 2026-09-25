using System;

namespace GoogleDriveClient
{
    /// <summary>
    /// Базовий абстрактний клас для сховища облікових даних (База 2-ї ієрархії).
    /// </summary>
    public abstract class CredentialStore
    {
        private readonly string _storeName;

        public string StoreName => _storeName;

        protected CredentialStore(string storeName)
        {
            if (string.IsNullOrWhiteSpace(storeName))
                throw new ArgumentException("Назва сховища не може бути порожньою.", nameof(storeName));

            _storeName = storeName.Trim();
        }

        /// <summary>
        /// Поліморфне збереження облікових даних (Динамічний поліморфізм №2).
        /// </summary>
        public abstract void SaveCredentials(string userId, UserCredential credential);

        /// <summary>
        /// Поліморфне завантаження облікових даних.
        /// </summary>
        public abstract UserCredential? LoadCredentials(string userId);

        /// <summary>
        /// Поліморфне видалення облікових даних.
        /// </summary>
        public abstract bool ClearCredentials(string userId);
    }
}