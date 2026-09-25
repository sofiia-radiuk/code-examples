using System;

namespace GoogleDriveClient
{
    /// <summary>
    /// Базовий виняток для помилок клієнта Google Drive.
    /// </summary>
    public class DriveApiException : Exception
    {
        public int StatusCode { get; }

        public DriveApiException(string message, int statusCode = 400) 
            : base(message)
        {
            StatusCode = statusCode;
        }

        public DriveApiException(string message, Exception innerException, int statusCode = 400) 
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Помилка авторизації або простроченого токена.
    /// </summary>
    public class DriveAuthenticationException : DriveApiException
    {
        public DriveAuthenticationException(string message) 
            : base(message, 401)
        {
        }
    }

    /// <summary>
    /// Помилка, коли запитуваний об'єкт не знайдено на диску.
    /// </summary>
    public class DriveItemNotFoundException : DriveApiException
    {
        public string ItemId { get; }

        public DriveItemNotFoundException(string itemId) 
            : base($"Елемент із зазначеним ідентифікатором '{itemId}' не знайдено.", 404)
        {
            ItemId = itemId;
        }
    }
}