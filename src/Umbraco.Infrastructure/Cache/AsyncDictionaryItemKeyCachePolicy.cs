using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping.EFCore;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// A cache policy for looking up dictionary items by their string item key.
/// </summary>
internal class AsyncDictionaryItemKeyCachePolicy : AsyncDefaultRepositoryCachePolicy<IDictionaryItem, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncDictionaryItemKeyCachePolicy"/> class.
    /// </summary>
    public AsyncDictionaryItemKeyCachePolicy(
        IAppPolicyCache cache,
        IScopeAccessor scopeAccessor,
        AsyncRepositoryCachePolicyOptions options,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(cache, scopeAccessor, options, repositoryCacheVersionService, cacheSyncService)
    {
    }

    /// <summary>
    /// Retrieves a dictionary item by its string item key, using the cache where possible.
    /// </summary>
    /// <param name="key">The string item key to look up.</param>
    /// <param name="performGet">A delegate that fetches the item from the database if not cached.</param>
    /// <returns>The cached or freshly fetched dictionary item, or <c>null</c> if not found.</returns>
    public async Task<IDictionaryItem?> GetByItemKeyAsync(string key, Func<string, Task<IDictionaryItem?>> performGet)
        => await GetAsync(key, async keyString => await performGet(keyString!), performGetAll: null!);

    /// <summary>
    /// Removes the cache entry for a dictionary item identified by its string item key.
    /// </summary>
    /// <param name="itemKey">The string item key whose cache entry should be removed.</param>
    public async Task ClearByItemKeyAsync(string itemKey)
    {
        await RegisterCacheChangeAsync();

        var cacheKey = GetEntityCacheKey(itemKey);
        Cache.ClearByKey(cacheKey);
    }
}
