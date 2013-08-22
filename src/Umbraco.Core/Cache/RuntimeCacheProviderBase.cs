using System;
using System.Text;
using System.Web.Caching;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// An abstract class for implementing a runtime cache provider
    /// </summary>
    /// <remarks>
    /// THIS MUST REMAIN INTERNAL UNTIL WE STREAMLINE HOW ALL CACHE IS HANDLED, WE NEED TO SUPPORT HTTP RUNTIME CACHE, IN MEMORY CACHE, REQUEST CACHE, ETC...
    /// </remarks>
    internal abstract class RuntimeCacheProviderBase : CacheProviderBase
    {
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
