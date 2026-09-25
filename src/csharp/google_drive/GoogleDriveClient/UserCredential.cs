using System;

namespace GoogleDriveClient
{
    /// <summary>
    /// Представляє токени та дані сесії користувача Google Drive.
    /// </summary>
    public class UserCredential
    {
        private string _clientId;
        private string _accessToken;
        private string _refreshToken;
        private DateTime _expiresAt;

        public string ClientId => _clientId;
        public string AccessToken => _accessToken;
        public string RefreshToken => _refreshToken;
        public DateTime ExpiresAt => _expiresAt;

        public bool IsExpired => DateTime.UtcNow >= _expiresAt;

        public UserCredential(string clientId, string accessToken, string refreshToken, int expiresInSeconds)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Client ID не може бути порожнім.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access Token не може бути порожнім.", nameof(accessToken));
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh Token не може бути порожнім.", nameof(refreshToken));
            if (expiresInSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(expiresInSeconds), "Час дії токена повинен бути більшим за 0.");

            _clientId = clientId.Trim();
            _accessToken = accessToken.Trim();
            _refreshToken = refreshToken.Trim();
            _expiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        }

        public void UpdateToken(string newAccessToken, int expiresInSeconds)
        {
            if (string.IsNullOrWhiteSpace(newAccessToken))
                throw new ArgumentException("Новий Access Token не може бути порожнім.", nameof(newAccessToken));

            _accessToken = newAccessToken.Trim();
            _expiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
        }
    }
}