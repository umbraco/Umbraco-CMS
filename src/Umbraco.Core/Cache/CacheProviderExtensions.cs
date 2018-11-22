using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Extensions for strongly typed access
    /// </summary>
    public static class CacheProviderExtensions
    {
        public static T GetCacheItem<T>(this IRuntimeCacheProvider provider,
            string cacheKey,
            Func<T> getCacheItem,
            TimeSpan? timeout,
            bool isSliding = false,
            CacheItemPriority priority = CacheItemPriority.Normal,
            CacheItemRemovedCallback removedCallback = null,
            string[] dependentFiles = null)
        {
            var result = provider.GetCacheItem(cacheKey, () => getCacheItem(), timeout, isSliding, priority, removedCallback, dependentFiles);
            return result == null ? default(T) : result.TryConvertTo<T>().Result;
        }

        public static void InsertCacheItem<T>(this IRuntimeCacheProvider provider,
            string cacheKey,
            Func<T> getCacheItem,
            TimeSpan? timeout = null,
            bool isSliding = false,
            CacheItemPriority priority = CacheItemPriority.Normal,
            CacheItemRemovedCallback removedCallback = null,
            string[] dependentFiles = null)
        {
            provider.InsertCacheItem(cacheKey, () => getCacheItem(), timeout, isSliding, priority, removedCallback, dependentFiles);
        }

        public static IEnumerable<T> GetCacheItemsByKeySearch<T>(this ICacheProvider provider, string keyStartsWith)
        {
            var result = provider.GetCacheItemsByKeySearch(keyStartsWith);
            return result.Select(x => x.TryConvertTo<T>().Result);
        }

        public static IEnumerable<T> GetCacheItemsByKeyExpression<T>(this ICacheProvider provider, string regexString)
        {
            var result = provider.GetCacheItemsByKeyExpression(regexString);
            return result.Select(x => x.TryConvertTo<T>().Result);
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