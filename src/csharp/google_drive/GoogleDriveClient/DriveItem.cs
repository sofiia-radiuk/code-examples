using System;

namespace GoogleDriveClient
{
    /// <summary>
    /// Базовий абстрактний компонент файлової системи Google Drive (Component у Composite).
    /// </summary>
    public abstract class DriveItem
    {
        private string _id;
        private string _name;
        private string _mimeType;
        private DateTime _lastModified;

        public string Id => _id;
        public string Name => _name;
        public string MimeType => _mimeType;
        public DateTime LastModified => _lastModified;

        protected DriveItem(string id, string name, string mimeType)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID не може бути порожнім.", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Назва не може бути порожньою.", nameof(name));
            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("MIME-тип не може бути порожнім.", nameof(mimeType));

            _id = id.Trim();
            _name = name.Trim();
            _mimeType = mimeType.Trim();
            _lastModified = DateTime.UtcNow;
        }

        public virtual void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Нова назва не може бути порожньою.", nameof(newName));

            _name = newName.Trim();
            Touch();
        }

        protected void Touch()
        {
            _lastModified = DateTime.UtcNow;
        }

        public virtual bool IsFolder()
        {
            return _mimeType == "application/vnd.google-apps.folder";
        }

        /// <summary>
        /// Поліморфний розрахунок розміру елемента в байтах (Динамічний поліморфізм №1).
        /// </summary>
        public abstract long CalculateTotalSize();

        /// <summary>
        /// Відображення інформації про елемент.
        /// </summary>
        public virtual void Display(int indentLevel = 0)
        {
            string indent = new string(' ', indentLevel * 2);
            Console.WriteLine($"{indent}- [{GetType().Name}] {Name} (ID: {Id}, Розмір: {CalculateTotalSize()} байт, Змінено: {LastModified:yyyy-MM-dd HH:mm})");
        }
    }
}