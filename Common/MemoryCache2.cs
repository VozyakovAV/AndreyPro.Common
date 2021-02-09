using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Runtime.Caching;

namespace AndreyPro.Common
{
    public class MemoryCache2 : MemoryCache
    {
        private static Lazy<MemoryCache2> _instance = new Lazy<MemoryCache2>();
        public static MemoryCache2 Instance => _instance.Value;

        private ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public MemoryCache2()
            : base("MemoryCache2")
        {

        }

        public MemoryCache2(string name, NameValueCollection config = null)
            : base(name, config)
        {

        }

        /// <summary>
        /// Получить значение из кеша по ключу key
        /// Если его нет, то вызывается функция func, записывается в кеш значение, и возвращается
        /// Обеспечивает lock по ключу
        /// </summary>
        public T GetOrAddConcurrent<T>(string key, Func<T> func)
        {
            return GetOrAddConcurrentImpl(key, func, (c, v) => Set(key, v, new CacheItemPolicy()));
        }

        /// <summary>
        /// Получить значение из кеша по ключу key
        /// Если его нет, то вызывается функция func, записывается в кеш значение, и возвращается
        /// Обеспечивает lock по ключу
        /// <param name="timeExpiration"></param>
        /// </summary>
        public T GetOrAddConcurrent<T>(string key, int timeExpirationMs, Func<T> func)
        {
            return GetOrAddConcurrentImpl(key, func, (c, v) => Set(key, v, DateTime.Now.AddMilliseconds(timeExpirationMs)));
        }

        private T GetOrAddConcurrentImpl<T>(string key, Func<T> func, Action<MemoryCache, object> actionSetCache)
        {
            var value = Get(key);
            if (value == null)
            {
                var locker = _locks.GetOrAdd(key, k => new object());
                lock (locker)
                {
                    value = Get(key);
                    if (value == null)
                    {
                        value = func();
                        actionSetCache(this, value);
                    }
                }
            }

            if (value is T res)
                return res;
            return default;
        }
    }
}
