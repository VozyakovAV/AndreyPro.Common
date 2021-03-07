using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AndreyPro.Common
{
    public class DictionaryWrapper<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public object SyncObj => new object();
        
        public delegate void ModifyHandler(ModifyType type, TValue value);
        
        public event ModifyHandler Modify;
        
        public int Count => _items.Count;

        public ConcurrentDictionary<TKey, TValue> Source => _items;

        public ICollection<TKey> Keys => _items.Keys;

        public ICollection<TValue> Values => _items.Values;

        private readonly ConcurrentDictionary<TKey, TValue> _items;

        private readonly Func<TValue, TKey> _identifier;

        public virtual TValue this[TKey key] { get => _items[key]; }

        public DictionaryWrapper(Func<TValue, TKey> identifier)
        {
            _identifier = identifier;
            _items = new ConcurrentDictionary<TKey, TValue>();
        }

        public virtual void Clear()
        {
            lock (SyncObj)
                _items.Clear();
        }

        /// <summary>
        /// Добавить в коллекцию если нет такого.
        /// </summary>
        /// <returns>true - если добавлено</returns>
        public virtual bool Add(TValue value)
        {
            lock (SyncObj)
            {
                var key = _identifier(value);
                if (_items.TryAdd(key, value))
                {
                    Modify?.Invoke(ModifyType.Added, value);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Добавить в коллекцию если нет такого или обновить существующий.
        /// </summary>
        public virtual void AddOrUpdate(TValue value)
        {
            lock (SyncObj)
            {
                var key = _identifier(value);
                var modify = ModifyType.Added;
                _items.AddOrUpdate(key, (k) =>
                {
                    modify = ModifyType.Added;
                    return value;
                },
                (k, v) =>
                {
                    modify = ModifyType.Updated;
                    return value;
                });
                Modify?.Invoke(modify, value);
            }
        }

        /// <summary>
        /// Добавить в коллекцию если нет такого или обновить существующий.
        /// </summary>
        public virtual void AddOrUpdate(IEnumerable<TValue> values)
        {
            lock (SyncObj)
            {
                foreach (TValue value in values)
                    AddOrUpdate(value);
            }
        }

        /// <summary>
        /// Удалить по ключу
        /// </summary>
        public virtual void RemoveByKey(TKey key)
        {
            lock (SyncObj)
            {
                if (_items.TryRemove(key, out var value))
                    Modify?.Invoke(ModifyType.Removed, value);
            }
        }

        /// <summary>
        /// Удалить по значению
        /// </summary>
        public virtual void RemoveByValue(TValue value)
        {
            lock (SyncObj)
            {
                var key = _identifier(value);
                RemoveByKey(key);
            }
        }

        public bool Contains(TKey key)
        {
            lock (SyncObj)
                return _items.ContainsKey(key);
        }

        public virtual TValue GetOrAdd(TKey key, Func<TKey, TValue> func)
        {
            lock (SyncObj)
            {
                if (_items.TryGetValue(key, out var value))
                    return value;
                value = func(key);
                _items.TryAdd(key, value);
                return value;
            }    
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            lock (SyncObj)
                return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public enum ModifyType
    {
        Added,
        Updated,
        Removed
    }
}
