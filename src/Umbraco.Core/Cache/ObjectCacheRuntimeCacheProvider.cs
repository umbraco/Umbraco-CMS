using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Caching;
using Umbraco.Core.Logging;
using CacheItemPriority = System.Web.Caching.CacheItemPriority;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A cache provider that wraps the logic of a System.Runtime.Caching.ObjectCache
    /// </summary>
    internal class ObjectCacheRuntimeCacheProvider : IRuntimeCacheProvider
    {
        private static readonly ReaderWriterLockSlim ClearLock = new ReaderWriterLockSlim();
        internal ObjectCache MemoryCache;

        public ObjectCacheRuntimeCacheProvider()
        {
            MemoryCache = new MemoryCache("in-memory");
        }

        public virtual void ClearAllCache()
        {
            using (new WriteLock(ClearLock))
            {
                MemoryCache.DisposeIfDisposable();
                MemoryCache = new MemoryCache("in-memory");
            }
        }

        public virtual void ClearCacheItem(string key)
        {
            using (new WriteLock(ClearLock))
            {
                if (MemoryCache[key] == null) return;
                MemoryCache.Remove(key);
            }            
        }

        public virtual void ClearCacheObjectTypes(string typeName)
        {
            using (new WriteLock(ClearLock))
            {
                var keysToRemove = (from c in MemoryCache where c.Value.GetType().ToString().InvariantEquals(typeName) select c.Key).ToList();
                foreach (var k in keysToRemove)
                {
                    MemoryCache.Remove(k);
                }
            }
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
            using (new WriteLock(ClearLock))
            {
                var keysToRemove = (from c in MemoryCache where c.Value.GetType() == typeof (T) select c.Key).ToList();
                foreach (var k in keysToRemove)
                {
                    MemoryCache.Remove(k);
                }
            }
        }

        public virtual void ClearCacheByKeySearch(string keyStartsWith)
        {
            using (new WriteLock(ClearLock))
            {
                var keysToRemove = (from c in MemoryCache where c.Key.InvariantStartsWith(keyStartsWith) select c.Key).ToList();
                foreach (var k in keysToRemove)
                {
                    MemoryCache.Remove(k);
                }
            }            
        }

        public virtual void ClearCacheByKeyExpression(string regexString)
        {
            using (new WriteLock(ClearLock))
            {
                var keysToRemove = (from c in MemoryCache where Regex.IsMatch(c.Key, regexString) select c.Key).ToList();
                foreach (var k in keysToRemove)
                {
                    MemoryCache.Remove(k);
                }
            }     
        }

        public virtual IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            return (from c in MemoryCache
                    where c.Key.InvariantStartsWith(keyStartsWith)
                    select c.Value.TryConvertTo<T>()
                        into attempt
                        where attempt.Success
                        select attempt.Result).ToList();
        }

        public virtual T GetCacheItem<T>(string cacheKey)
        {
            var result = MemoryCache.Get(cacheKey);
            if (result == null)
            {
                return default(T);
            }
            return result.TryConvertTo<T>().Result;
        }

        public virtual T GetCacheItem<T>(string cacheKey, Func<T> getCacheItem)
        {
            return GetCacheItem(cacheKey, CacheItemPriority.Normal, null, null, null, getCacheItem);
        }

        public virtual T GetCacheItem<T>(string cacheKey, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return GetCacheItem(cacheKey, null, timeout, getCacheItem);
        }

        public virtual T GetCacheItem<T>(string cacheKey, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return GetCacheItem(cacheKey, CacheItemPriority.Normal, refreshAction, timeout, getCacheItem);
        }

        public virtual T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return GetCacheItem(cacheKey, priority, refreshAction, null, timeout, getCacheItem);
        }

        public virtual T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem)
        {
            using (var lck = new UpgradeableReadLock(ClearLock))
            {
                var result = MemoryCache.Get(cacheKey);
                if (result == null)
                {
                    lck.UpgradeToWriteLock();

                    result = getCacheItem();
                    if (result != null)
                    {
                        var policy = new CacheItemPolicy
                            {
                                AbsoluteExpiration = timeout == null ? ObjectCache.InfiniteAbsoluteExpiration : DateTime.Now.Add(timeout.Value),
                                SlidingExpiration = TimeSpan.Zero
                            };
                        
                        //TODO: CUrrently we cannot implement this in this provider, we'll have to change the underlying interface
                        // to accept an array of files instead of CacheDependency.
                        //policy.ChangeMonitors.Add(new HostFileChangeMonitor(cacheDependency.));

                        MemoryCache.Set(cacheKey, result, policy);
                    }
                }
                return result.TryConvertTo<T>().Result;
            }
        }

        public virtual void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, Func<T> getCacheItem)
        {
            InsertCacheItem(cacheKey, priority, null, null, null, getCacheItem);
        }

        public virtual void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, TimeSpan? timeout, Func<T> getCacheItem)
        {
            InsertCacheItem(cacheKey, priority, null, null, timeout, getCacheItem);
        }

        public virtual void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem)
        {
            InsertCacheItem(cacheKey, priority, null, cacheDependency, timeout, getCacheItem);
        }

        public virtual void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem)
        {
            object result = getCacheItem();
            if (result != null)
            {

                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = timeout == null ? ObjectCache.InfiniteAbsoluteExpiration : DateTime.Now.Add(timeout.Value),
                    SlidingExpiration = TimeSpan.Zero
                };

                //TODO: CUrrently we cannot implement this in this provider, we'll have to change the underlying interface
                // to accept an array of files instead of CacheDependency.
                //policy.ChangeMonitors.Add(new HostFileChangeMonitor(cacheDependency.));

                MemoryCache.Set(cacheKey, result, policy);
            }
        }
        
    }
}