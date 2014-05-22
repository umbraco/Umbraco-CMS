using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        private IEnumerable<KeyValuePair<string, object>> EnumerateDictionaryCache()
        {
            // DictionaryCache just wraps _cache which has a special enumerator
            var enumerator = _cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key as string;
                if (key == null) continue;
                yield return new KeyValuePair<string, object>(key, enumerator.Value);
            }
        }

        public override void ClearAllCache()
        {
            using (new WriteLock(Locker))
            {
                foreach (var kvp in EnumerateDictionaryCache()
                    .Where(x => x.Key.StartsWith(CacheItemPrefix) && x.Value != null))
                    _cache.Remove(kvp.Key);
            }
        }

        public override void ClearCacheItem(string key)
        {
            using (new WriteLock(Locker))
            {
                var cacheKey = GetCacheKey(key);
                _cache.Remove(cacheKey);
            }
        }

        public override void ClearCacheObjectTypes(string typeName)
        {
            var typeName2 = typeName;
            using (new WriteLock(Locker))
            {
                foreach (var kvp in EnumerateDictionaryCache()
                    .Where(x => x.Key.StartsWith(CacheItemPrefix) 
                        && x.Value != null
                        && x.Value.GetType().ToString().InvariantEquals(typeName2)))
                    _cache.Remove(kvp.Key);
            }
        }

        public override void ClearCacheObjectTypes<T>()
        {
            // should we use "is" or compare types?

            //var typeOfT = typeof(T);
            using (new WriteLock(Locker))
            {
                foreach (var kvp in EnumerateDictionaryCache()
                    .Where(x => x.Key.StartsWith(CacheItemPrefix)
                        //&& x.Value != null
                        //&& x.Value.GetType() == typeOfT))
                        && x.Value is T))
                    _cache.Remove(kvp.Key);
            }
        }

        public override void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            // see note above
            // should we use "is" or compare types?

            try
            {
                using (new WriteLock(Locker))
                {
                    foreach (var kvp in EnumerateDictionaryCache()
                        .Where(x => x.Key.StartsWith(CacheItemPrefix) 
                            && x.Value is T 
                            && predicate(x.Key, (T) x.Value)))
                        _cache.Remove(kvp.Key);
                }
            }
            catch (Exception e)
            {
                // oops, what is this?!
                LogHelper.Error<CacheHelper>("Cache clearing error", e);
            }
        }

        public override void ClearCacheByKeySearch(string keyStartsWith)
        {
            using (new WriteLock(Locker))
            {
                foreach (var kvp in EnumerateDictionaryCache()
                    .Where(x => x.Key.InvariantStartsWith(string.Format("{0}-{1}", CacheItemPrefix, keyStartsWith))))
                    _cache.Remove(kvp.Key);
            }
        }

        public override void ClearCacheByKeyExpression(string regexString)
        {
            var plen = CacheItemPrefix.Length + 1; // string.Format("{0}-", CacheItemPrefix)
            using (new WriteLock(Locker))
            {
                foreach (var kvp in EnumerateDictionaryCache()
                    .Where(x => x.Key.StartsWith(CacheItemPrefix)
                        && Regex.IsMatch(x.Key.Substring(plen), regexString)))
                    _cache.Remove(kvp.Key);
            }
        }

        public override IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            return EnumerateDictionaryCache()
                .Where(x => x.Key.InvariantStartsWith(string.Format("{0}-{1}", CacheItemPrefix, keyStartsWith)))
                .Select(x => ((Lazy<object>)x.Value).Value)
                .ToList();
        }

        public override IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            var plen = CacheItemPrefix.Length + 1; // string.Format("{0}-", CacheItemPrefix)
            return EnumerateDictionaryCache()
                .Where(x => x.Key.StartsWith(CacheItemPrefix)
                    && Regex.IsMatch(x.Key.Substring(plen), regexString))
                .Select(x => ((Lazy<object>)x.Value).Value)
                .ToList();
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
            var value = result.Value; // force evaluation now
            //if (value == null) return;

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