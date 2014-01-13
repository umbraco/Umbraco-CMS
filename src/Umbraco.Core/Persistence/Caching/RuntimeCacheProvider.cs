using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Web;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.Caching
{
    /// <summary>
    /// The Runtime Cache provider looks up objects in the Runtime cache for fast retrival
    /// </summary>
    /// <remarks>
    /// 
    /// If a web session is detected then the HttpRuntime.Cache will be used for the runtime cache, otherwise a custom
    /// MemoryCache instance will be used. It is important to use the HttpRuntime.Cache when a web session is detected so 
    /// that the memory management of cache in IIS can be handled appopriately.
    /// 
    /// When a web sessions is detected we will pre-fix all HttpRuntime.Cache entries so that when we clear it we are only 
    /// clearing items that have been inserted by this provider.
    /// 
    /// NOTE: These changes are all temporary until we finalize the ApplicationCache implementation which will support static cache, runtime cache
    /// and request based cache which will all live in one central location so it is easily managed. 
    /// 
    /// Also note that we don't always keep checking if HttpContext.Current == null and instead check for _memoryCache != null. This is because
    /// when there are async requests being made even in the context of a web request, the HttpContext.Current will be null but the HttpRuntime.Cache will
    /// always be available.
    /// 
    /// </remarks>
    internal sealed class RuntimeCacheProvider : IRepositoryCacheProvider
    {
        #region Singleton

        private static readonly Lazy<RuntimeCacheProvider> lazy = new Lazy<RuntimeCacheProvider>(() => new RuntimeCacheProvider());

        public static RuntimeCacheProvider Current { get { return lazy.Value; } }

        private RuntimeCacheProvider()
        {
            if (HttpContext.Current == null)
            {
                _memoryCache = new MemoryCache("in-memory");
            }
        }

        #endregion

        //TODO Save this in cache as well, so its not limited to a single server usage
        private readonly ConcurrentHashSet<string> _keyTracker = new ConcurrentHashSet<string>();
        private ObjectCache _memoryCache;
        private static readonly ReaderWriterLockSlim ClearLock = new ReaderWriterLockSlim();

        public IEntity GetById(Type type, Guid id)
        {
            var key = GetCompositeId(type, id);
            var item = _memoryCache != null 
                ? _memoryCache.Get(key) 
                : HttpRuntime.Cache.Get(key);
            var result = item as IEntity;
            if (result == null)
            {
                //ensure the key doesn't exist anymore in the tracker
                _keyTracker.Remove(key);
            }
            return result;
        }

        public IEnumerable<IEntity> GetByIds(Type type, List<Guid> ids)
        {
            var collection = new List<IEntity>();
            foreach (var guid in ids)
            {
                var key = GetCompositeId(type, guid);
                var item = _memoryCache != null
                               ? _memoryCache.Get(key)
                               : HttpRuntime.Cache.Get(key);
                var result = item as IEntity;
                if (result == null)
                {
                    //ensure the key doesn't exist anymore in the tracker
                    _keyTracker.Remove(key);
                }
                else
                {
                    collection.Add(result);
                }
            }
            return collection;
        }

        public IEnumerable<IEntity> GetAllByType(Type type)
        {
            var collection = new List<IEntity>();
            foreach (var key in _keyTracker)
            {
                if (key.StartsWith(string.Format("{0}{1}-", CacheItemPrefix, type.Name)))
                {
                    var item = _memoryCache != null
                               ? _memoryCache.Get(key)
                               : HttpRuntime.Cache.Get(key);

                    var result = item as IEntity;
                    if (result == null)
                    {
                        //ensure the key doesn't exist anymore in the tracker
                        _keyTracker.Remove(key);
                    }
                    else
                    {
                        collection.Add(result);
                    }
                }
            }
            return collection;
        }

        public void Save(Type type, IEntity entity)
        {
            var key = GetCompositeId(type, entity.Id);
            
            _keyTracker.TryAdd(key);

            //NOTE: Before we were checking if it already exists but the MemoryCache.Set handles this implicitly and does 
            // an add or update, same goes for HttpRuntime.Cache.Insert.

            if (_memoryCache != null)
            {
                _memoryCache.Set(key, entity, new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(5) });
            }
            else
            {
                HttpRuntime.Cache.Insert(key, entity, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5));
            }
        }

        public void Delete(Type type, IEntity entity)
        {
            var key = GetCompositeId(type, entity.Id);
            if (_memoryCache != null)
            {
                _memoryCache.Remove(key);
            }
            else
            {
                HttpRuntime.Cache.Remove(key);
            }
            
            _keyTracker.Remove(key);
        }

        public void Delete(Type type, int entityId)
        {
            var key = GetCompositeId(type, entityId);
            if (_memoryCache != null)
            {
                _memoryCache.Remove(key);
            }
            else
            {
                HttpRuntime.Cache.Remove(key);
            }

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
                foreach (var key in keys.Where(x => x.StartsWith(string.Format("{0}{1}-", CacheItemPrefix, type.Name))))
                {
                    _keyTracker.Remove(key);
                    keysToRemove.Add(key);
                }
                foreach (var key in keysToRemove)
                {
                    if (_memoryCache != null)
                    {
                        _memoryCache.Remove(key);
                    }
                    else
                    {
                        HttpRuntime.Cache.Remove(key);
                    }
                }
            }
        }

        public void Clear()
        {
            using (new WriteLock(ClearLock))
            {
                _keyTracker.Clear();

                ClearDataCache();
            }
        }

        //DO not call this unless it's for testing since it clears the data cached but not the keys
        internal void ClearDataCache()
        {
            if (_memoryCache != null)
            {
                _memoryCache.DisposeIfDisposable();
                _memoryCache = new MemoryCache("in-memory");
            }
            else
            {
                foreach (DictionaryEntry c in HttpRuntime.Cache)
                {
                    if (c.Key is string && ((string)c.Key).InvariantStartsWith(CacheItemPrefix))
                    {
                        if (HttpRuntime.Cache[(string)c.Key] == null) return;
                        HttpRuntime.Cache.Remove((string)c.Key);
                    }
                }
            }
        }

        /// <summary>
        /// We prefix all cache keys with this so that we know which ones this class has created when 
        /// using the HttpRuntime cache so that when we clear it we don't clear other entries we didn't create.
        /// </summary>
        private const string CacheItemPrefix = "umbrtmche_";

        private string GetCompositeId(Type type, Guid id)
        {
            return string.Format("{0}{1}-{2}", CacheItemPrefix, type.Name, id.ToString());
        }

        private string GetCompositeId(Type type, int id)
        {
            return string.Format("{0}{1}-{2}", CacheItemPrefix, type.Name, id.ToGuid());
        }
    }
}