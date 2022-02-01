using System;
using System.Collections.Concurrent;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Provides a base class for implementing a dictionary of <see cref="IAppPolicyCache"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
    public abstract class AppPolicedCacheDictionary<TKey> : IDisposable
    {
        private readonly ConcurrentDictionary<TKey, IAppPolicyCache> _caches = new ConcurrentDictionary<TKey, IAppPolicyCache>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AppPolicedCacheDictionary{TKey}"/> class.
        /// </summary>
        /// <param name="cacheFactory"></param>
        protected AppPolicedCacheDictionary(Func<TKey, IAppPolicyCache> cacheFactory)
        {
            CacheFactory = cacheFactory;
        }

        /// <summary>
        /// Gets the internal cache factory, for tests only!
        /// </summary>
        internal readonly Func<TKey, IAppPolicyCache> CacheFactory;
        private bool _disposedValue;

        /// <summary>
        /// Gets or creates a cache.
        /// </summary>
        public IAppPolicyCache GetOrCreate(TKey key)
            => _caches.GetOrAdd(key, k => CacheFactory(k));

        /// <summary>
        /// Tries to get a cache.
        /// </summary>
        public Attempt<IAppPolicyCache> Get(TKey key)
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach(var value in _caches.Values)
                    {
                        value.DisposeIfDisposable();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }
    }
}
