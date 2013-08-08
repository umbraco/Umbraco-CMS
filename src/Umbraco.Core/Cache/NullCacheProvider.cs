using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

namespace Umbraco.Core.Cache
{
    internal class NullCacheProvider : IRuntimeCacheProvider
    {
        public virtual void ClearAllCache()
        {
        }

        public virtual void ClearCacheItem(string key)
        {
        }

        public virtual void ClearCacheObjectTypes(string typeName)
        {
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
        }

        public virtual void ClearCacheByKeySearch(string keyStartsWith)
        {
        }

        public virtual void ClearCacheByKeyExpression(string regexString)
        {
        }

        public virtual IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            return Enumerable.Empty<T>();
        }

        public virtual T GetCacheItem<T>(string cacheKey)
        {
            return default(T);
        }

        public virtual T GetCacheItem<T>(string cacheKey, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public virtual T GetCacheItem<T>(string cacheKey, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public virtual T GetCacheItem<T>(string cacheKey, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public virtual T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public virtual T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public virtual void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, Func<T> getCacheItem)
        {
        }

        public virtual void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, TimeSpan? timeout, Func<T> getCacheItem)
        {
        }

        public virtual void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem)
        {
        }

        public virtual void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem)
        {
        }
    }
}