using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Extensions for strongly typed access
    /// </summary>
    internal static class CacheProviderExtensions
    {
        //T GetCacheItem<T>(string cacheKey, TimeSpan? timeout, Func<T> getCacheItem);
        //T GetCacheItem<T>(string cacheKey, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem);
        //T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, TimeSpan? timeout, Func<T> getCacheItem);
        //T GetCacheItem<T>(string cacheKey, CacheItemPriority priority, CacheItemRemovedCallback refreshAction, CacheDependency cacheDependency, TimeSpan? timeout, Func<T> getCacheItem);

        public static void ClearCacheObjectTypes<T>(this ICacheProvider provider)
        {
            provider.ClearCacheObjectTypes(typeof(T).ToString());
        }

        public static IEnumerable<T> GetCacheItemsByKeySearch<T>(this ICacheProvider provider, string keyStartsWith)
        {
            var result = provider.GetCacheItemsByKeySearch(keyStartsWith);
            return result.Select(x => ObjectExtensions.TryConvertTo<T>(x).Result);
        }

        public static T GetCacheItem<T>(this ICacheProvider provider, string cacheKey)
        {
            var result = provider.GetCacheItem(cacheKey);
            if (result == null)
            {
                return default(T);
            }
            return result.TryConvertTo<T>().Result;
        }

        public static T GetCacheItem<T>(this ICacheProvider provider, string cacheKey, Func<T> getCacheItem)
        {
            var result = provider.GetCacheItem(cacheKey, () => getCacheItem());
            if (result == null)
            {
                return default(T);
            }
            return result.TryConvertTo<T>().Result;
        }
    }
}