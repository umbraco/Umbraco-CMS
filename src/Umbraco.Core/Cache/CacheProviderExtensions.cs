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
        public static T GetCacheItem<T>(this IAppPolicyCache provider,
            string cacheKey,
            Func<T> getCacheItem,
            TimeSpan? timeout,
            bool isSliding = false,
            CacheItemPriority priority = CacheItemPriority.Normal,
            CacheItemRemovedCallback removedCallback = null,
            string[] dependentFiles = null)
        {
            var result = provider.Get(cacheKey, () => getCacheItem(), timeout, isSliding, priority, removedCallback, dependentFiles);
            return result == null ? default(T) : result.TryConvertTo<T>().Result;
        }

        public static void InsertCacheItem<T>(this IAppPolicyCache provider,
            string cacheKey,
            Func<T> getCacheItem,
            TimeSpan? timeout = null,
            bool isSliding = false,
            CacheItemPriority priority = CacheItemPriority.Normal,
            CacheItemRemovedCallback removedCallback = null,
            string[] dependentFiles = null)
        {
            provider.Insert(cacheKey, () => getCacheItem(), timeout, isSliding, priority, removedCallback, dependentFiles);
        }

        public static IEnumerable<T> GetCacheItemsByKeySearch<T>(this IAppCache provider, string keyStartsWith)
        {
            var result = provider.SearchByKey(keyStartsWith);
            return result.Select(x => x.TryConvertTo<T>().Result);
        }

        public static IEnumerable<T> GetCacheItemsByKeyExpression<T>(this IAppCache provider, string regexString)
        {
            var result = provider.SearchByRegex(regexString);
            return result.Select(x => x.TryConvertTo<T>().Result);
        }

        public static T GetCacheItem<T>(this IAppCache provider, string cacheKey)
        {
            var result = provider.Get(cacheKey);
            if (result == null)
            {
                return default(T);
            }
            return result.TryConvertTo<T>().Result;
        }

        public static T GetCacheItem<T>(this IAppCache provider, string cacheKey, Func<T> getCacheItem)
        {
            var result = provider.Get(cacheKey, () => getCacheItem());
            if (result == null)
            {
                return default(T);
            }
            return result.TryConvertTo<T>().Result;
        }
    }
}
