using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// An abstract class for implementing a cache helper
    /// </summary>
    /// <remarks>
    /// THIS MUST REMAIN INTERNAL UNTIL WE STREAMLINE HOW ALL CACHE IS HANDLED, WE NEED TO SUPPORT HTTP RUNTIME CACHE, IN MEMORY CACHE, ETC...
    /// </remarks>
    internal abstract class CacheProviderBase
    {
        public abstract void ClearAllCache();
        public abstract void ClearCacheItem(string key);
        public abstract void ClearCacheObjectTypes(string typeName);
        public abstract void ClearCacheObjectTypes<T>();
        public abstract void ClearCacheByKeySearch(string keyStartsWith);
        public abstract void ClearCacheByKeyExpression(string regexString);
        public abstract IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith);
        public abstract T GetCacheItem<T>(string cacheKey);
        public abstract T GetCacheItem<T>(string cacheKey, Func<T> getCacheItem);
        public abstract T GetCacheItem<T>(string cacheKey, TimeSpan? timeout, Func<T> getCacheItem);
        public abstract T GetCacheItem<T>(string cacheKey, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem);
        public abstract T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem);
        public abstract T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem);
        public abstract void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, Func<T> getCacheItem);
        public abstract void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, TimeSpan? timeout, Func<T> getCacheItem);
        public abstract void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem);
        public abstract void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem);
    }
}
