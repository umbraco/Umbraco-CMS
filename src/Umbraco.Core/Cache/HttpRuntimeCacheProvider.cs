using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A CacheProvider that wraps the logic of the HttpRuntime.Cache
    /// </summary>
    internal class HttpRuntimeCacheProvider : DictionaryCacheProviderBase, IRuntimeCacheProvider
    {
        private readonly System.Web.Caching.Cache _cache;

        public HttpRuntimeCacheProvider(System.Web.Caching.Cache cache)
        {
            _cache = cache;
        }

        protected override IEnumerable<DictionaryEntry> GetDictionaryEntries()
        {
            const string prefix = CacheItemPrefix + "-";
            return _cache.Cast<DictionaryEntry>()
                .Where(x => x.Key is string && ((string) x.Key).StartsWith(prefix));
        }

        protected override void RemoveEntry(string key)
        {
            _cache.Remove(key);
        }

        protected override object GetEntry(string key)
        {
            return _cache.Get(key);
        }

        #region Get

        /// <summary>
        /// Gets (and adds if necessary) an item from the cache with all of the default parameters
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <returns></returns>
        public override object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            return GetCacheItem(cacheKey, getCacheItem, null, dependentFiles: null);
        }

        /// <summary>
        /// This overload is here for legacy purposes
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <param name="timeout"></param>
        /// <param name="isSliding"></param>
        /// <param name="priority"></param>
        /// <param name="removedCallback"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        internal object GetCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, CacheDependency dependency = null)
        {
            cacheKey = GetCacheKey(cacheKey);

            // NOTE - because we don't know what getCacheItem does, how long it will take and whether it will hang,
            // getCacheItem should run OUTSIDE of the global application lock else we run into lock contention and
            // nasty performance issues.

            // So.... we insert a Lazy<object> in the cache while holding the global application lock, and then rely
            // on the Lazy lock to ensure that getCacheItem runs once and everybody waits on it, while the global
            // application lock has been released.

            // Note that the Lazy execution may produce a null value.
            // Must make sure (for backward compatibility) that we pretend they are not in the cache.
            // So if we find an entry in the cache that already has its value created and is null,
            // pretend it was not there. If value is not already created, wait... and return null, that's
            // what prior code did.

            // So... the null value _will_ be in the cache but never returned

            Lazy<object> result;

            using (var lck = new UpgradeableReadLock(Locker))
            {
                result = _cache.Get(cacheKey) as Lazy<object>; // null if key not found
                if (result == null || (result.IsValueCreated && GetSafeLazyValue(result) == null)) // get exceptions as null
                {
                    lck.UpgradeToWriteLock();

                    result = new Lazy<object>(getCacheItem);
                    var absolute = isSliding ? System.Web.Caching.Cache.NoAbsoluteExpiration : (timeout == null ? System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(timeout.Value));
                    var sliding = isSliding == false ? System.Web.Caching.Cache.NoSlidingExpiration : (timeout ?? System.Web.Caching.Cache.NoSlidingExpiration);
                    _cache.Insert(cacheKey, result, dependency, absolute, sliding, priority, removedCallback);
                }
            }

            // this may throw if getCacheItem throws, but this is the only place where
            // it would throw as everywhere else we use GetLazySaveValue() to hide exceptions
            // and pretend exceptions were never inserted into cache to begin with.
            return result.Value;
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            CacheDependency dependency = null;
            if (dependentFiles != null && dependentFiles.Any())
            {
                dependency = new CacheDependency(dependentFiles);
            }
            return GetCacheItem(cacheKey, getCacheItem, timeout, isSliding, priority, removedCallback, dependency);
        }

        #endregion

        #region Insert

        /// <summary>
        /// This overload is here for legacy purposes
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="getCacheItem"></param>
        /// <param name="timeout"></param>
        /// <param name="isSliding"></param>
        /// <param name="priority"></param>
        /// <param name="removedCallback"></param>
        /// <param name="dependency"></param>
        internal void InsertCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, CacheDependency dependency = null)
        {
            // NOTE - here also we must insert a Lazy<object> but we can evaluate it right now
            // and make sure we don't store a null value.

            var result = new Lazy<object>(getCacheItem);
            var value = result.Value; // force evaluation now - this may throw if cacheItem throws, and then nothing goes into cache
            if (value == null) return; // do not store null values (backward compat)

            cacheKey = GetCacheKey(cacheKey);           

            var absolute = isSliding ? System.Web.Caching.Cache.NoAbsoluteExpiration : (timeout == null ? System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(timeout.Value));
            var sliding = isSliding == false ? System.Web.Caching.Cache.NoSlidingExpiration : (timeout ?? System.Web.Caching.Cache.NoSlidingExpiration);

            _cache.Insert(cacheKey, result, dependency, absolute, sliding, priority, removedCallback);
        }

        public void InsertCacheItem(string cacheKey, Func<object> getCacheItem, TimeSpan? timeout = null, bool isSliding = false, CacheItemPriority priority = CacheItemPriority.Normal, CacheItemRemovedCallback removedCallback = null, string[] dependentFiles = null)
        {
            CacheDependency dependency = null;
            if (dependentFiles != null && dependentFiles.Any())
            {
                dependency = new CacheDependency(dependentFiles);
            }
            InsertCacheItem(cacheKey, getCacheItem, timeout, isSliding, priority, removedCallback, dependency);
        }

        #endregion
    }
}