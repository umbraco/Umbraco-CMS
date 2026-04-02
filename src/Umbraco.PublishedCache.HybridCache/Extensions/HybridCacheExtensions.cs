using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Hybrid;

namespace Umbraco.Cms.Infrastructure.HybridCache.Extensions;

/// <summary>
/// Provides extension methods on <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/>.
/// </summary>
internal static class HybridCacheExtensions
{
    // Per-key semaphores to ensure the GetOrCreateAsync + RemoveAsync sequence
    // executes atomically for a given cache key.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

    /// <summary>
    /// Returns true if the cache contains an item with a matching key.
    /// </summary>
    /// <param name="cache">An instance of <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/></param>
    /// <param name="key">The name (key) of the item to search for in the cache.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the item exists already. False if it doesn't.</returns>
    /// <remarks>
    /// Hat-tip: https://github.com/dotnet/aspnetcore/discussions/57191
    /// Will never add or alter the state of any items in the cache.
    /// </remarks>
    public static async Task<bool> ExistsAsync<T>(this Microsoft.Extensions.Caching.Hybrid.HybridCache cache, string key, CancellationToken token)
    {
        (bool exists, _) = await TryGetValueAsync<T>(cache, key, token).ConfigureAwait(false);
        return exists;
    }

    /// <summary>
    /// Returns true if the cache contains an item with a matching key, along with the value of the matching cache entry.
    /// </summary>
    /// <typeparam name="T">The type of the value of the item in the cache.</typeparam>
    /// <param name="cache">An instance of <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/></param>
    /// <param name="key">The name (key) of the item to search for in the cache.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A tuple of <see cref="bool"/> and the object (if found) retrieved from the cache.</returns>
    /// <remarks>
    /// Hat-tip: https://github.com/dotnet/aspnetcore/discussions/57191
    /// Will never add or alter the state of any items in the cache.
    /// </remarks>
    public static async Task<(bool Exists, T? Value)> TryGetValueAsync<T>(this Microsoft.Extensions.Caching.Hybrid.HybridCache cache, string key, CancellationToken token)
    {
        var exists = true;

        // Acquire a per-key semaphore so that GetOrCreateAsync and the possible RemoveAsync
        // complete without another thread retrieving/creating the same key in-between.
        SemaphoreSlim sem = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await sem.WaitAsync().ConfigureAwait(false);

        try
        {
            T? result = await cache.GetOrCreateAsync<T?>(
                key,
                cancellationToken =>
                {
                    exists = false;
                    return default;
                },
                new HybridCacheEntryOptions(),
                null,
                token).ConfigureAwait(false);

            // In checking for the existence of the item, if not found, we will have created a cache entry with a null value.
            // So remove it again. Because we're holding the per-key lock there is no chance another thread
            // will observe the temporary entry between GetOrCreateAsync and RemoveAsync.
            if (exists is false)
            {
                await cache.RemoveAsync(key).ConfigureAwait(false);
            }

            return (exists, result);
        }
        finally
        {
            sem.Release();

            // Only remove the semaphore mapping if it still points to the same instance we used.
            // This avoids removing another thread's semaphore or corrupting the map.
            if (_keyLocks.TryGetValue(key, out SemaphoreSlim? current) && ReferenceEquals(current, sem))
            {
                _keyLocks.TryRemove(key, out _);
            }
        }
    }
}
