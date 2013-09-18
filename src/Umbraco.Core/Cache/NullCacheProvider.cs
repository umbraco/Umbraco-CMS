using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

namespace Umbraco.Core.Cache
{
    internal class NullCacheProvider : RuntimeCacheProviderBase
    {
        public override void ClearAllCache()
        {
        }

        public override void ClearCacheItem(string key)
        {
        }

        public override void ClearCacheObjectTypes(string typeName)
        {
        }

        public override void ClearCacheObjectTypes<T>()
        {
        }

        public override void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
        }

        public override void ClearCacheByKeySearch(string keyStartsWith)
        {
        }

        public override void ClearCacheByKeyExpression(string regexString)
        {
        }

        public override IEnumerable<T> GetCacheItemsByKeySearch<T>(string keyStartsWith)
        {
            return Enumerable.Empty<T>();
        }

        public override T GetCacheItem<T>(string cacheKey)
        {
            return default(T);
        }

        public override T GetCacheItem<T>(string cacheKey, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public override T GetCacheItem<T>(string cacheKey, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public override T GetCacheItem<T>(string cacheKey, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public override T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public override T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem)
        {
            return getCacheItem();
        }

        public override void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, Func<T> getCacheItem)
        {
        }

        public override void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, TimeSpan? timeout, Func<T> getCacheItem)
        {
        }

        public override void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem)
        {
        }

        public override void InsertCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem)
        {
        }
    }
}