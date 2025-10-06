using Umbraco.Cms.Core.Cache;

namespace Umbraco.Extensions;

/// <summary>
///     Extensions for strongly typed access
/// </summary>
public static class AppCacheExtensions
{
    public static T? GetCacheItem<T>(
        this IAppPolicyCache provider,
        string cacheKey,
        Func<T?> getCacheItem,
        TimeSpan? timeout,
        bool isSliding = false)
    {
        var result = provider.Get(cacheKey, () => getCacheItem(), timeout, isSliding);
        return result == null ? default : result.TryConvertTo<T>().Result;
    }

    public static void InsertCacheItem<T>(
        this IAppPolicyCache provider,
        string cacheKey,
        Func<T> getCacheItem,
        TimeSpan? timeout = null,
        bool isSliding = false) =>
        provider.Insert(cacheKey, () => getCacheItem(), timeout, isSliding);

    public static IEnumerable<T?> GetCacheItemsByKeySearch<T>(this IAppCache provider, string keyStartsWith)
    {
        IEnumerable<object?> result = provider.SearchByKey(keyStartsWith);
        return result.Select(x => x.TryConvertTo<T>().Result);
    }

    public static IEnumerable<T?> GetCacheItemsByKeyExpression<T>(this IAppCache provider, string regexString)
    {
        IEnumerable<object?> result = provider.SearchByRegex(regexString);
        return result.Select(x => x.TryConvertTo<T>().Result);
    }

    public static T? GetCacheItem<T>(this IAppCache provider, string cacheKey)
    {
        var result = provider.Get(cacheKey);
        if (result == null)
        {
            return default;
        }

        // If we've retrieved the specific string that represents null in the cache, return it only if we are requesting it (via a typed request for a string).
        // Otherwise consider it a null value.
        if (RetrievedNullRepresentationInCache(result))
        {
            return RequestedNullRepresentationInCache<T>() ? (T)result : default;
        }

        return result.TryConvertTo<T>().Result;
    }

    public static T? GetCacheItem<T>(this IAppCache provider, string cacheKey, Func<T> getCacheItem)
    {
        var result = provider.Get(cacheKey, () => getCacheItem());
        if (result == null)
        {
            return default;
        }

        // If we've retrieved the specific string that represents null in the cache, return it only if we are requesting it (via a typed request for a string).
        // Otherwise consider it a null value.
        if (RetrievedNullRepresentationInCache(result))
        {
            return RequestedNullRepresentationInCache<T>() ? (T)result : default;
        }

        return result.TryConvertTo<T>().Result;
    }

    public static async Task<T?> GetCacheItemAsync<T>(
        this IAppPolicyCache provider,
        string cacheKey,
        Func<Task<T?>> getCacheItemAsync,
        TimeSpan? timeout,
        bool isSliding = false)
    {
        var result = provider.Get(cacheKey);

        if (result == null)
        {
            result = await getCacheItemAsync();
            provider.Insert(cacheKey, () => result, timeout, isSliding);
        }

        if (result == null)
        {
            return default;
        }

        // If we've retrieved the specific string that represents null in the cache, return it only if we are requesting it (via a typed request for a string).
        // Otherwise consider it a null value.
        if (RetrievedNullRepresentationInCache(result))
        {
            return RequestedNullRepresentationInCache<T>() ? (T)result : default;
        }

        return result.TryConvertTo<T>().Result;
    }

    private static bool RetrievedNullRepresentationInCache(object result) => result == (object)Cms.Core.Constants.Cache.NullRepresentationInCache;

    private static bool RequestedNullRepresentationInCache<T>() => typeof(T) == typeof(string);

    public static async Task InsertCacheItemAsync<T>(
        this IAppPolicyCache provider,
        string cacheKey,
        Func<Task<T>> getCacheItemAsync,
        TimeSpan? timeout = null,
        bool isSliding = false)
    {
        T value = await getCacheItemAsync();
        provider.Insert(cacheKey, () => value, timeout, isSliding);
    }
}
