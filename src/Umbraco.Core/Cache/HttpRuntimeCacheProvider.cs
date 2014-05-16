using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using Umbraco.Core.Logging;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A CacheProvider that wraps the logic of the HttpRuntime.Cache
    /// </summary>
    internal class HttpRuntimeCacheProvider : DictionaryCacheProviderBase, IRuntimeCacheProvider
    {
        private readonly System.Web.Caching.Cache _cache;
        private readonly DictionaryCacheWrapper _wrapper;
        
        public HttpRuntimeCacheProvider(System.Web.Caching.Cache cache)
        {
            _cache = cache;
            _wrapper = new DictionaryCacheWrapper(_cache, s => _cache.Get(s.ToString()), o => _cache.Remove(o.ToString()));
        }

        protected override DictionaryCacheWrapper DictionaryCache
        {
            get { return _wrapper; }
        }

        /// <summary>
        /// Clears all objects in the System.Web.Cache with the System.Type specified that satisfy the predicate
        /// </summary>
        public override void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            try
            {
                lock (Locker)
                {
                    foreach (DictionaryEntry c in _cache)
                    {
                        var key = c.Key.ToString();
                        if (_cache[key] != null
                            && _cache[key] is T
                            && predicate(key, (T)_cache[key]))
                        {
                            _cache.Remove(c.Key.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error<CacheHelper>("Cache clearing error", e);
            }
        }

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

            // Note that this means we'll end up storing null values in the cache, whereas in the past we made sure
            // not to store them. There's code below, commented out, to make sure we do re-compute the value if it
            // was previously computed as null - effectively reproducing the past behavior - but I'm not quite sure
            // it is a good idea.

            Lazy<object> result;

            using (var lck = new UpgradeableReadLock(Locker))
            {
                result = DictionaryCache.Get(cacheKey) as Lazy<object>;
                if (result == null /* || (result.IsValueCreated && result.Value == null) */)
                {
                    lck.UpgradeToWriteLock();

                    result = new Lazy<object>(getCacheItem);
                    var absolute = isSliding ? System.Web.Caching.Cache.NoAbsoluteExpiration : (timeout == null ? System.Web.Caching.Cache.NoAbsoluteExpiration : DateTime.Now.Add(timeout.Value));
                    var sliding = isSliding == false ? System.Web.Caching.Cache.NoSlidingExpiration : (timeout ?? System.Web.Caching.Cache.NoSlidingExpiration);
                    _cache.Insert(cacheKey, result, dependency, absolute, sliding, priority, removedCallback);
                }
            }

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
            // and make sure we don't store a null value. Though I'm not sure it is a good idea.

            var result = new Lazy<object>(getCacheItem);
            //if (result.Value == null) return;

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
    }
}