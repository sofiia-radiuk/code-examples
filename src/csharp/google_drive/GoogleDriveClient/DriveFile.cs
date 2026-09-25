using System;
using System.IO;

namespace GoogleDriveClient
{
    /// <summary>
    /// Представляє загальний файл у сховищі (Leaf у Composite).
    /// </summary>
    public class DriveFile : DriveItem
    {
        private string _fileExtension;

        public string FileExtension => _fileExtension;

        public DriveFile(string id, string name, string mimeType) 
            : base(id, name, mimeType)
        {
            _fileExtension = Path.GetExtension(name);
        }

        public override void Rename(string newName)
        {
            base.Rename(newName);
            _fileExtension = Path.GetExtension(newName);
        }

        public override long CalculateTotalSize()
        {
            return 0;
        }
    }
}