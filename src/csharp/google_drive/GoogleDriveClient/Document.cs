using System;
using System.Text;

namespace GoogleDriveClient
{
    /// <summary>
    /// Текстовий документ Google Docs.
    /// </summary>
    public class Document : DriveFile
    {
        private readonly StringBuilder _content;

        public Document(string id, string name, string initialText = "") 
            : base(id, name, "application/vnd.google-apps.document")
        {
            _content = new StringBuilder(initialText ?? string.Empty);
        }

        public int CharacterCount => _content.Length;

        public void AppendText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _content.Append(text);
            Touch();
        }

        public void Clear()
        {
            if (_content.Length > 0)
            {
                _content.Clear();
                Touch();
            }
        }

        public string ReadContent()
        {
            return _content.ToString();
        }

        public int CountWords()
        {
            string text = _content.ToString().Trim();
            if (string.IsNullOrEmpty(text))
                return 0;

            return text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public override long CalculateTotalSize()
        {
            return Encoding.UTF8.GetByteCount(_content.ToString());
        }
    }
}