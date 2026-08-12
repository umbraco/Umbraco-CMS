using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Defines common cache operations shared across all content cache services (documents, media, elements).
/// </summary>
public interface IContentCacheService
{
    /// <summary>
    /// Seeds the cache with initial data.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Removes the specified item from the cache.
    /// </summary>
    /// <param name="content">The item to remove from the cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteItemAsync(IContentBase content);

    /// <summary>
    /// Rebuilds the database cache for items of the specified content types.
    /// </summary>
    /// <param name="contentTypeIds">The collection of content type identifiers to rebuild.</param>
    void Rebuild(IReadOnlyCollection<int> contentTypeIds);

    /// <summary>
    /// Clears all entries from the memory cache.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearMemoryCacheAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes the memory cache entry for the item with the specified key.
    /// </summary>
    /// <param name="key">The unique key of the item to refresh.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshMemoryCacheAsync(Guid key);

    /// <summary>
    /// Removes the item with the specified key from the memory cache.
    /// </summary>
    /// <param name="key">The unique key of the item to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveFromMemoryCacheAsync(Guid key);

    /// <summary>
    /// Rebuilds the memory cache for items of the specified content types.
    /// </summary>
    /// <param name="contentTypeIds">The collection of content type identifiers to rebuild in memory cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> contentTypeIds);

    /// <summary>
    /// Clears all converted published content entries from the in-memory cache,
    /// without rebuilding the underlying database cache or HybridCache entries.
    /// </summary>
    /// <remarks>
    /// Use this when the published model factory is reset (e.g. InMemoryAuto mode), which
    /// invalidates all compiled model types and makes cached instances of any type stale.
    /// </remarks>
    void ClearConvertedContentCache();

    /// <summary>
    /// Clears converted published content entries for the specified content types from the in-memory cache,
    /// without rebuilding the underlying database cache or HybridCache entries.
    /// </summary>
    /// <remarks>
    /// Use this when the published model factory is NOT reset (e.g. SourceCodeAuto/SourceCodeManual modes),
    /// so only the affected content types need their converted cache cleared.
    /// </remarks>
    /// <param name="contentTypeIds">The IDs of the content types whose converted entries should be cleared.</param>
    void ClearConvertedContentCache(IReadOnlyCollection<int> contentTypeIds);
}
