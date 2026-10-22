using System.Collections.Immutable;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for strongly typed access to <see cref="IAppCache" /> and <see cref="IAppPolicyCache" />.
/// </summary>
public static class AppCacheExtensions
{
    /// <summary>
    ///     Gets a strongly typed cache item with the specified key, creating it if necessary using the provided factory.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="provider">The cache provider.</param>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="getCacheItem">A factory function that creates the item if not found.</param>
    /// <param name="timeout">An optional cache timeout.</param>
    /// <param name="isSliding">Whether the timeout is sliding (resets on access).</param>
    /// <returns>The cached item, or default if not found and factory returns null.</returns>
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

    /// <summary>
    ///     Inserts a strongly typed cache item with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="provider">The cache provider.</param>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="getCacheItem">A factory function that creates the item to cache.</param>
    /// <param name="timeout">An optional cache timeout.</param>
    /// <param name="isSliding">Whether the timeout is sliding (resets on access).</param>
    public static void InsertCacheItem<T>(
        this IAppPolicyCache provider,
        string cacheKey,
        Func<T> getCacheItem,
        TimeSpan? timeout = null,
        bool isSliding = false) =>
        provider.Insert(cacheKey, () => getCacheItem(), timeout, isSliding);

    /// <summary>
    ///     Gets strongly typed cache items with keys starting with the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the cached items.</typeparam>
    /// <param name="provider">The cache provider.</param>
    /// <param name="keyStartsWith">The key prefix to search for.</param>
    /// <returns>A collection of cached items matching the search.</returns>
    public static IEnumerable<T?> GetCacheItemsByKeySearch<T>(this IAppCache provider, string keyStartsWith)
    {
        IEnumerable<object?> result = provider.SearchByKey(keyStartsWith);
        return result.Select(x => x.TryConvertTo<T>().Result);
    }

    /// <summary>
    ///     Gets strongly typed cache items with keys matching a regular expression.
    /// </summary>
    /// <typeparam name="T">The type of the cached items.</typeparam>
    /// <param name="provider">The cache provider.</param>
    /// <param name="regexString">The regular expression pattern to match keys against.</param>
    /// <returns>A collection of cached items matching the search.</returns>
    public static IEnumerable<T?> GetCacheItemsByKeyExpression<T>(this IAppCache provider, string regexString)
    {
        IEnumerable<object?> result = provider.SearchByRegex(regexString);
        return result.Select(x => x.TryConvertTo<T>().Result);
    }

    /// <summary>
    ///     Gets a strongly typed cache item with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="provider">The cache provider.</param>
    /// <param name="cacheKey">The cache key.</param>
    /// <returns>The cached item, or default if not found.</returns>
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

    /// <summary>
    ///     Gets a strongly typed cache item with the specified key, creating it if necessary using the provided factory.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="provider">The cache provider.</param>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="getCacheItem">A factory function that creates the item if not found.</param>
    /// <returns>The cached item, or default if not found and factory returns null.</returns>
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

    /// <summary>
    ///     Asynchronously gets a strongly typed cache item with the specified key, creating it if necessary using the provided factory.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="provider">The cache provider.</param>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="getCacheItemAsync">An async factory function that creates the item if not found.</param>
    /// <param name="timeout">An optional cache timeout.</param>
    /// <param name="isSliding">Whether the timeout is sliding (resets on access).</param>
    /// <returns>A task representing the asynchronous operation, containing the cached item.</returns>
    public static async Task<T?> GetCacheItemAsync<T>(
        this IAppPolicyCache provider,
        string cacheKey,
        Func<Task<T?>> getCacheItemAsync,
        TimeSpan? timeout,
        bool isSliding = false)
    {
        var result = provider.Get(cacheKey)
            ?? await GetOrInsertLockedAsync(provider, cacheKey, getCacheItemAsync, timeout, isSliding);

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

    // Serializes concurrent misses on the same key so the factory only runs once, matching the stampede protection
    // the synchronous GetCacheItem gets from IAppPolicyCache.Get(key, factory). Locks are scoped per cache instance
    // and removed once no caller is using them. (#23921)
    private static async Task<object?> GetOrInsertLockedAsync<T>(
        IAppPolicyCache provider,
        string cacheKey,
        Func<Task<T?>> getCacheItemAsync,
        TimeSpan? timeout,
        bool isSliding)
    {
        var lockKey = (provider, cacheKey);

        // The lock isn't re-entrant, so a factory requesting its own key would wait on itself forever. Fail fast
        // instead, as the synchronous GetCacheItem does (Lazy throws on a recursive Value access).
        var heldKeys = _heldKeyedLocks.Value ?? [];
        if (heldKeys.Contains(lockKey))
        {
            throw new InvalidOperationException(
                $"The factory for cache key '{cacheKey}' attempted to get the same key from the same cache.");
        }

        using var releaser = await KeyedLock.AcquireAsync(lockKey);

        // Another caller may have populated the cache while we were waiting.
        var result = provider.Get(cacheKey);
        if (result != null)
        {
            return result;
        }

        // Flows into the factory's async context only, so it is gone once this method returns.
        _heldKeyedLocks.Value = heldKeys.Add(lockKey);
        result = await getCacheItemAsync();
        provider.Insert(cacheKey, () => result, timeout, isSliding);

        return result;
    }

    private static readonly AsyncLocal<ImmutableHashSet<(IAppPolicyCache, string)>?> _heldKeyedLocks = new();

    // A per-key semaphore, reference-counted so it is removed once no caller is holding or waiting on it.
    private sealed class KeyedLock
    {
        private static readonly Dictionary<(IAppPolicyCache, string), KeyedLock> _locks = [];

        private readonly (IAppPolicyCache, string) _key;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private int _refCount;

        private KeyedLock((IAppPolicyCache, string) key) => _key = key;

        public static async Task<Releaser> AcquireAsync((IAppPolicyCache, string) key)
        {
            var keyedLock = AddReference(key);
            await keyedLock._semaphore.WaitAsync();
            return new Releaser(keyedLock);
        }

        private static KeyedLock AddReference((IAppPolicyCache, string) key)
        {
            lock (_locks)
            {
                if (_locks.TryGetValue(key, out var keyedLock) is false)
                {
                    keyedLock = new KeyedLock(key);
                    _locks[key] = keyedLock;
                }

                keyedLock._refCount++;
                return keyedLock;
            }
        }

        private void Release()
        {
            _semaphore.Release();

            lock (_locks)
            {
                if (--_refCount == 0)
                {
                    _locks.Remove(_key);
                    _semaphore.Dispose();
                }
            }
        }

        public readonly struct Releaser(KeyedLock keyedLock) : IDisposable
        {
            public void Dispose() => keyedLock.Release();
        }
    }

    private static bool RetrievedNullRepresentationInCache(object result) => result == (object)Cms.Core.Constants.Cache.NullRepresentationInCache;

    private static bool RequestedNullRepresentationInCache<T>() => typeof(T) == typeof(string);

    /// <summary>
    ///     Asynchronously inserts a strongly typed cache item with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the cached item.</typeparam>
    /// <param name="provider">The cache provider.</param>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="getCacheItemAsync">An async factory function that creates the item to cache.</param>
    /// <param name="timeout">An optional cache timeout.</param>
    /// <param name="isSliding">Whether the timeout is sliding (resets on access).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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
