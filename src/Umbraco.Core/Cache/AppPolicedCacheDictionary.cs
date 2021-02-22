using System;
using System.Collections.Concurrent;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// Provides a base class for implementing a dictionary of <see cref="IAppPolicyCache"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    public abstract class AppPolicedCacheDictionary<TKey>
    {
        private readonly ConcurrentDictionary<TKey, IAppPolicyCache> _caches = new ConcurrentDictionary<TKey, IAppPolicyCache>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AppPolicedCacheDictionary{TKey}"/> class.
        /// </summary>
        /// <param name="cacheFactory"></param>
        protected AppPolicedCacheDictionary(Func<TKey, IAppPolicyCache> cacheFactory)
        {
            _cacheFactory = cacheFactory;
        }

        /// <summary>
        /// Gets the internal cache factory, for tests only!
        /// </summary>
        private readonly Func<TKey, IAppPolicyCache> _cacheFactory;

        /// <summary>
        /// Gets or creates a cache.
        /// </summary>
        public IAppPolicyCache GetOrCreate(TKey key)
            => _caches.GetOrAdd(key, k => _cacheFactory(k));

        /// <summary>
        /// Tries to get a cache.
        /// </summary>
        protected Attempt<IAppPolicyCache> Get(TKey key)
            => _caches.TryGetValue(key, out var cache) ? Attempt.Succeed(cache) : Attempt.Fail<IAppPolicyCache>();

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
        protected void ClearCache(TKey key)
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
