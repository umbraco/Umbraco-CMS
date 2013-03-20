using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly Lazy<RuntimeCacheProvider> lazy = new Lazy<RuntimeCacheProvider>(() => new RuntimeCacheProvider());

        public static RuntimeCacheProvider Current { get { return lazy.Value; } }

        private RuntimeCacheProvider()
        {
        }

        #endregion

        //TODO Save this in cache as well, so its not limited to a single server usage
        private readonly ConcurrentHashSet<string> _keyTracker = new ConcurrentHashSet<string>();
        private ObjectCache _memoryCache = new MemoryCache("in-memory");
        private static readonly ReaderWriterLockSlim ClearLock = new ReaderWriterLockSlim();

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
            foreach (var key in _keyTracker)
            {
                if (key.StartsWith(type.Name))
                {
                    yield return _memoryCache.Get(key) as IEntity;
                }
            }
        }

        public void Save(Type type, IEntity entity)
        {
            var key = GetCompositeId(type, entity.Id);
            var exists = _memoryCache.GetCacheItem(key) != null;
            _keyTracker.TryAdd(key);
            if (exists)
            {
                _memoryCache.Set(key, entity, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) });
                return;
            }

            _memoryCache.Add(key, entity, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) });
        }

        public void Delete(Type type, IEntity entity)
        {
            var key = GetCompositeId(type, entity.Id);
            _memoryCache.Remove(key);
            _keyTracker.Remove(key);
        }

        /// <summary>
        /// Clear cache by type
        /// </summary>
        /// <param name="type"></param>
        public void Clear(Type type)
        {
            using (new WriteLock(ClearLock))
            {
                var keys = new string[_keyTracker.Count];
                _keyTracker.CopyTo(keys, 0);
                var keysToRemove = new List<string>();
                foreach (var key in keys.Where(x => x.StartsWith(string.Format("{0}-", type.Name))))
                {
                    _keyTracker.Remove(key);
                    keysToRemove.Add(key);
                }
                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                }
            }
        }

        public void Clear()
        {
            using (new WriteLock(ClearLock))
            {
                _keyTracker.Clear();
                _memoryCache.DisposeIfDisposable();
                _memoryCache = new MemoryCache("in-memory");
            }
        }

        private string GetCompositeId(Type type, Guid id)
        {
            return string.Format("{0}-{1}", type.Name, id.ToString());
        }

        private string GetCompositeId(Type type, int id)
        {
            return string.Format("{0}-{1}", type.Name, id.ToGuid());
        }
    }
}