using System;
using System.Collections.Generic;

namespace GoogleDriveClient
{
    /// <summary>
    /// Таблиця Google Sheets.
    /// </summary>
    public class Spreadsheet : DriveFile
    {
        private readonly Dictionary<string, string> _cells = new();

        public Spreadsheet(string id, string name) 
            : base(id, name, "application/vnd.google-apps.spreadsheet")
        {
        }

        public int TotalFilledCells => _cells.Count;

        public void SetCell(string cellAddress, string value)
        {
            if (string.IsNullOrWhiteSpace(cellAddress))
                throw new ArgumentException("Адреса комірки не може бути порожньою.", nameof(cellAddress));

            string key = cellAddress.ToUpperInvariant();
            if (string.IsNullOrEmpty(value))
            {
                _cells.Remove(key);
            }
            else
            {
                _cells[key] = value;
            }
            Touch();
        }

        public string GetCell(string cellAddress)
        {
            string key = cellAddress.ToUpperInvariant();
            return _cells.TryGetValue(key, out var val) ? val : string.Empty;
        }

        public bool HasCell(string cellAddress)
        {
            return _cells.ContainsKey(cellAddress.ToUpperInvariant());
        }

        public override long CalculateTotalSize()
        {
            long size = 0;
            foreach (var kvp in _cells)
            {
                size += kvp.Key.Length + (kvp.Value?.Length ?? 0);
            }
            return size;
        }
    }
}