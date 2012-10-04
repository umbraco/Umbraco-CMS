using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.Caching
{
    /// <summary>
    /// The Runtime Cache provider looks up objects in the Runtime cache for fast retrival
    /// </summary>
    internal sealed class RuntimeCacheProvider : IRepositoryCacheProvider
    {
        #region Singleton
        private static volatile RuntimeCacheProvider _instance;
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        private RuntimeCacheProvider() { }

        public static RuntimeCacheProvider Current
        {
            get
            {
                using (new WriteLock(Lock))
                {
                    if (_instance == null) _instance = new RuntimeCacheProvider();
                }

                return _instance;
            }
        }
        #endregion

        private ObjectCache _memoryCache = new MemoryCache("in-memory");
        private ConcurrentDictionary<string, string> _keyTracker = new ConcurrentDictionary<string, string>();

        public IEntity GetById(Type type, Guid id)
        {
            var key = GetCompositeId(type, id);
            var item = _memoryCache.Get(key);
            return item as IEntity;
        }

        public IEnumerable<IEntity> GetByIds(Type type, List<Guid> ids)
        {
            foreach (var guid in ids)
            {
                yield return _memoryCache.Get(GetCompositeId(type, guid)) as IEntity;
            }
        }

        public IEnumerable<IEntity> GetAllByType(Type type)
        {
            foreach (var key in _keyTracker.Keys)
            {
                if (key.StartsWith(type.Name))
                {
                    yield return _memoryCache.Get(key) as IEntity;
                }
            }
        }

        public void Save(Type type, IEntity entity)
        {
            var key = GetCompositeId(type, entity.Key);
            var exists = _memoryCache.GetCacheItem(key) != null;

            _keyTracker.TryAdd(key, key);
            if (exists)
            {
                _memoryCache.Set(key, entity, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) });
                return;
            }

            _memoryCache.Add(key, entity, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) });
        }

        public void Delete(Type type, IEntity entity)
        {
            string throwaway = null;
            var key = GetCompositeId(type, entity.Key);
            var keyBeSure = _keyTracker.TryGetValue(key, out throwaway);
            object itemRemoved = _memoryCache.Remove(key);
            _keyTracker.TryRemove(key, out throwaway);
        }

        private string GetCompositeId(Type type, Guid id)
        {
            return string.Format("{0}-{1}", type.Name, id.ToString());
        }
    }
}