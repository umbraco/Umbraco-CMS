using System;
using System.Collections.Concurrent;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Used to get/create/manipulate isolated runtime cache
    /// </summary>
    /// <remarks>
    /// This is useful for repository level caches to ensure that cache lookups by key are fast so 
    /// that the repository doesn't need to search through all keys on a global scale.
    /// </remarks>
    public class IsolatedRuntimeCache
    {
        internal Func<Type, IRuntimeCacheProvider> CacheFactory { get; set; }

        /// <summary>
        /// Constructor that allows specifying a factory for the type of runtime isolated cache to create
        /// </summary>
        /// <param name="cacheFactory"></param>
        public IsolatedRuntimeCache(Func<Type, IRuntimeCacheProvider> cacheFactory)
        {
            CacheFactory = cacheFactory;
        }

        private readonly ConcurrentDictionary<Type, IRuntimeCacheProvider> _isolatedCache = new ConcurrentDictionary<Type, IRuntimeCacheProvider>();

        /// <summary>
        /// Returns an isolated runtime cache for a given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>        
        public IRuntimeCacheProvider GetOrCreateCache<T>()
        {
            return _isolatedCache.GetOrAdd(typeof(T), type => CacheFactory(type));
        }

        /// <summary>
        /// Returns an isolated runtime cache for a given type
        /// </summary>
        /// <returns></returns>        
        public IRuntimeCacheProvider GetOrCreateCache(Type type)
        {
            return _isolatedCache.GetOrAdd(type, t => CacheFactory(t));
        }

        /// <summary>
        /// Tries to get a cache by the type specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Attempt<IRuntimeCacheProvider> GetCache<T>()
        {
            IRuntimeCacheProvider cache;
            if (_isolatedCache.TryGetValue(typeof(T), out cache))
            {
                return Attempt.Succeed(cache);
            }
            return Attempt<IRuntimeCacheProvider>.Fail();
        }

        /// <summary>
        /// Clears all values inside this isolated runtime cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void ClearCache<T>()
        {
            IRuntimeCacheProvider cache;
            if (_isolatedCache.TryGetValue(typeof(T), out cache))
            {
                cache.ClearAllCache();
            }
        }

        /// <summary>
        /// Clears all of the isolated caches
        /// </summary>
        public void ClearAllCaches()
        {
            foreach (var key in _isolatedCache.Keys)
            {
                IRuntimeCacheProvider cache;
                if (_isolatedCache.TryRemove(key, out cache))
                {
                    cache.ClearAllCache();
                }
            }
        }
    }
}