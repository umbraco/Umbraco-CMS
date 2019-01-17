using System;
using System.Collections.Concurrent;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Provides a base class for implementing a dictionary of <see cref="IAppPolicedCache"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    public abstract class AppPolicedCacheDictionary<TKey>
    {
        private readonly ConcurrentDictionary<TKey, IAppPolicedCache> _caches = new ConcurrentDictionary<TKey, IAppPolicedCache>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AppPolicedCacheDictionary{TKey}"/> class.
        /// </summary>
        /// <param name="cacheFactory"></param>
        protected AppPolicedCacheDictionary(Func<TKey, IAppPolicedCache> cacheFactory)
        {
            CacheFactory = cacheFactory;
        }

        /// <summary>
        /// Gets the internal cache factory, for tests only!
        /// </summary>
        internal readonly Func<TKey, IAppPolicedCache> CacheFactory;

        /// <summary>
        /// Gets or creates a cache.
        /// </summary>
        public IAppPolicedCache GetOrCreate(TKey key)
            => _caches.GetOrAdd(key, k => CacheFactory(k));

        /// <summary>
        /// Tries to get a cache.
        /// </summary>
        public Attempt<IAppPolicedCache> Get(TKey key)
            => _caches.TryGetValue(key, out var cache) ? Attempt.Succeed(cache) : Attempt.Fail<IAppPolicedCache>();

        /// <summary>
        /// Removes a cache.
        /// </summary>
        public void Remove(TKey key)
        {
            _caches.TryRemove(key, out _);
        }

        /// <summary>
        /// Removes all caches.
        /// </summary>
        public void RemoveAll()
        {
            _caches.Clear();
        }

        /// <summary>
        /// Clears a cache.
        /// </summary>
        public void ClearCache(TKey key)
        {
            if (_caches.TryGetValue(key, out var cache))
                cache.Clear();
        }

        /// <summary>
        /// Clears all caches.
        /// </summary>
        public void ClearAllCaches()
        {
            foreach (var cache in _caches.Values)
                cache.Clear();
        }
    }
}