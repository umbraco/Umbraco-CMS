using Microsoft.Extensions.Caching.Hybrid;

namespace Umbraco.Cms.Infrastructure.HybridCache.Extensions;

/// <summary>
/// Provides extension methods on <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/>.
/// </summary>
internal static class HybridCacheExtensions
{
    /// <summary>
    /// Returns true if the cache contains an item with a matching key.
    /// </summary>
    /// <param name="cache">An instance of <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/></param>
    /// <param name="key">The name (key) of the item to search for in the cache.</param>
    /// <returns>True if the item exists already. False if it doesn't.</returns>
    /// <remarks>
    /// Hat-tip: https://github.com/dotnet/aspnetcore/discussions/57191
    /// Will never add or alter the state of any items in the cache.
    /// </remarks>
    public static async Task<bool> ExistsAsync<T>(this Microsoft.Extensions.Caching.Hybrid.HybridCache cache, string key)
    {
        (bool exists, _) = await TryGetValueAsync<T>(cache, key);
        return exists;
    }

    /// <summary>
    /// Returns true if the cache contains an item with a matching key, along with the value of the matching cache entry.
    /// </summary>
    /// <typeparam name="T">The type of the value of the item in the cache.</typeparam>
    /// <param name="cache">An instance of <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/></param>
    /// <param name="key">The name (key) of the item to search for in the cache.</param>
    /// <returns>A tuple of <see cref="bool"/> and the object (if found) retrieved from the cache.</returns>
    /// <remarks>
    /// Hat-tip: https://github.com/dotnet/aspnetcore/discussions/57191
    /// Will never add or alter the state of any items in the cache.
    /// </remarks>
    public static async Task<(bool Exists, T? Value)> TryGetValueAsync<T>(this Microsoft.Extensions.Caching.Hybrid.HybridCache cache, string key)
    {
        var exists = true;

        T? result = await cache.GetOrCreateAsync<object, T>(
            key,
            null!,
            (_, _) =>
            {
                exists = false;
                return new ValueTask<T>(default(T)!);
            },
            new HybridCacheEntryOptions(),
            null,
            CancellationToken.None);

        // In checking for the existence of the item, if not found, we will have created a cache entry with a null value.
        // So remove it again.
        if (exists is false)
        {
            await cache.RemoveAsync(key);
        }

        return (exists, result);
    }
}
