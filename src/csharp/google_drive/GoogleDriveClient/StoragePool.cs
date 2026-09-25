using System;
using System.Collections;
using System.Collections.Generic;

namespace GoogleDriveClient
{
    /// <summary>
    /// Узагальнене сховище/кеш для елементів диска (Демонстрація Generics та статичного поліморфізму).
    /// </summary>
    /// <typeparam name="T">Тип елемента, що обов'язково є нащадком DriveItem.</typeparam>
    public class StoragePool<T> : IEnumerable<T> where T : DriveItem
    {
        private readonly Dictionary<string, T> _items = new();
        private readonly int _capacity;

        public int Capacity => _capacity;
        public int Count => _items.Count;
        public bool IsFull => _items.Count >= _capacity;

        public StoragePool(int capacity = 100)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Місткість пулу має бути більшою за 0.");

            _capacity = capacity;
        }

        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (IsFull)
                throw new InvalidOperationException($"Сховище переповнене. Максимальна місткість: {_capacity}");

            _items[item.Id] = item;
        }

        public T? Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            _items.TryGetValue(id.Trim(), out var item);
            return item;
        }

        public bool Remove(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return _items.Remove(id.Trim());
        }

        public bool Contains(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return _items.ContainsKey(id.Trim());
        }

        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Підрахунок сумарного розміру всіх об'єктів у пулі.
        /// </summary>
        public long CalculatePoolSize()
        {
            long total = 0;
            foreach (var item in _items.Values)
            {
                total += item.CalculateTotalSize();
            }
            return total;
        }

        // --- Статичний поліморфізм: Перевантаження операторів ---

        public static StoragePool<T> operator +(StoragePool<T> pool, T item)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            pool.Add(item);
            return pool;
        }

        public static StoragePool<T> operator -(StoragePool<T> pool, string itemId)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            pool.Remove(itemId);
            return pool;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}