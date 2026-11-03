using System;
using System.IO;
using System.Text.Json;

namespace GoogleDriveClient
{
    /// <summary>
    /// Файлове сховище облікових даних із локальним збереженням конфігурацій.
    /// </summary>
    public class LocalFileCredentialStore : CredentialStore
    {
        private readonly string _storageDirectory;

        public LocalFileCredentialStore(string storageDirectory = "credentials") 
            : base("Local File Store")
        {
            _storageDirectory = string.IsNullOrWhiteSpace(storageDirectory) ? "credentials" : storageDirectory.Trim();
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
        }

        private string GetFilePath(string userId) => Path.Combine(_storageDirectory, $"{userId}.cred");

        public override void SaveCredentials(string userId, UserCredential credential)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID не може бути порожнім.", nameof(userId));
            if (credential == null)
                throw new ArgumentNullException(nameof(credential));

            string json = JsonSerializer.Serialize(new
            {
                credential.ClientId,
                credential.AccessToken,
                credential.RefreshToken,
                credential.ExpiresAt
            });

            File.WriteAllText(GetFilePath(userId.Trim()), json);
        }

        public override UserCredential? LoadCredentials(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            string path = GetFilePath(userId.Trim());
            if (!File.Exists(path))
                return null;

            try
            {
                string json = File.ReadAllText(path);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string clientId = root.GetProperty("ClientId").GetString() ?? "";
                string accessToken = root.GetProperty("AccessToken").GetString() ?? "";
                string refreshToken = root.GetProperty("RefreshToken").GetString() ?? "";
                DateTime expiresAt = root.GetProperty("ExpiresAt").GetDateTime();

                int remainingSeconds = Math.Max(1, (int)(expiresAt - DateTime.UtcNow).TotalSeconds);
                return new UserCredential(clientId, accessToken, refreshToken, remainingSeconds);
            }
            catch
            {
                return null;
            }
        }

        public override bool ClearCredentials(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            string path = GetFilePath(userId.Trim());
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }
    }
}