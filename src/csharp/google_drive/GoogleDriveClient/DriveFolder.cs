using System;
using System.Collections.Generic;
using System.Linq;

namespace GoogleDriveClient
{
    /// <summary>
    /// Папка Google Drive (Composite у Composite Pattern).
    /// </summary>
    public class DriveFolder : DriveItem
    {
        private readonly List<DriveItem> _children = new();

        public DriveFolder(string id, string name) 
            : base(id, name, "application/vnd.google-apps.folder")
        {
        }

        public IReadOnlyList<DriveItem> Children => _children.AsReadOnly();

        public void AddChild(DriveItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (_children.Any(c => c.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Елемент із назвою '{item.Name}' вже існує в цій папці.");

            _children.Add(item);
            Touch();
        }

        public bool RemoveChild(string id)
        {
            var item = _children.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                _children.Remove(item);
                Touch();
                return true;
            }
            return false;
        }

        public DriveItem? FindByName(string name)
        {
            foreach (var child in _children)
            {
                if (child.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return child;

                if (child is DriveFolder subFolder)
                {
                    var found = subFolder.FindByName(name);
                    if (found != null)
                        return found;
                }
            }
            return null;
        }

        public override long CalculateTotalSize()
        {
            long sum = 0;
            foreach (var child in _children)
            {
                sum += child.CalculateTotalSize();
            }
            return sum;
        }

        public override void Display(int indentLevel = 0)
        {
            base.Display(indentLevel);
            foreach (var child in _children)
            {
                child.Display(indentLevel + 1);
            }
        }
    }
}