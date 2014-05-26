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

        public override void ClearAllCache()
        {
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries())
                    _cache.Remove((string)entry.Key);
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
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        var value = ((Lazy<object>) x.Value).Value;
                        return value == null || value.GetType().ToString().InvariantEquals(typeName); // remove null values as well
                    }))
                    _cache.Remove((string)entry.Key);
            }
        }

        public override void ClearCacheObjectTypes<T>()
        {
            // note: compare on exact type, don't use "is"

            var typeOfT = typeof(T);
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        var value = ((Lazy<object>)x.Value).Value;
                        return value == null || value.GetType() == typeOfT; // remove null values as well
                    }))
                    _cache.Remove((string)entry.Key);
            }
        }

        public override void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            // note: compare on exact type, don't use "is"

            var typeOfT = typeof(T);
            var plen = CacheItemPrefix.Length + 1;
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        var value = ((Lazy<object>)x.Value).Value;
                        if (value == null) return true; // remove null values as well
                        return value.GetType() == typeOfT
                            // run predicate on the 'public key' part only, ie without prefix
                            && predicate(((string) x.Key).Substring(plen), (T) value);
                    }))
                    _cache.Remove((string)entry.Key);
            }
        }

        public override void ClearCacheByKeySearch(string keyStartsWith)
        {
            var plen = CacheItemPrefix.Length + 1;
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x => ((string)x.Key).Substring(plen).InvariantStartsWith(keyStartsWith)))
                    _cache.Remove((string)entry.Key);
            }
        }

        public override void ClearCacheByKeyExpression(string regexString)
        {
            var plen = CacheItemPrefix.Length + 1;
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x => Regex.IsMatch(((string)x.Key).Substring(plen), regexString)))
                    _cache.Remove((string)entry.Key);
            }
        }

        private IEnumerable<DictionaryEntry> GetDictionaryEntries()
        {
            const string prefix = CacheItemPrefix + "-";
            return _cache.Cast<DictionaryEntry>()
                .Where(x => x.Key is string && ((string) x.Key).StartsWith(prefix));
        }

        public override IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            var plen = CacheItemPrefix.Length + 1;
            using (new ReadLock(Locker))
            {
                return GetDictionaryEntries()
                    .Where(x => ((string) x.Key).Substring(plen).InvariantStartsWith(keyStartsWith))
                    .Select(x => ((Lazy<object>)x.Value).Value)
                    .Where(x => x != null) // backward compat, don't store null values in the cache
                    .ToList();
            }
        }

        public override IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            const string prefix = CacheItemPrefix + "-";
            var plen = prefix.Length;
            using (new ReadLock(Locker))
            {
                return GetDictionaryEntries()
                    .Where(x => Regex.IsMatch(((string) x.Key).Substring(plen), regexString))
                    .Select(x => ((Lazy<object>)x.Value).Value)
                    .Where(x => x != null) // backward compat, don't store null values in the cache
                    .ToList();
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
                if (result == null || (result.IsValueCreated && result.Value == null))
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
            // and make sure we don't store a null value.

            var result = new Lazy<object>(getCacheItem);
            var value = result.Value; // force evaluation now
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
    }
}