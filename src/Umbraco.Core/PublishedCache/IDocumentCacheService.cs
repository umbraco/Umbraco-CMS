using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Defines operations for the document (content) cache service.
/// </summary>
/// <remarks>
/// This service provides access to published document content with caching support,
/// including operations for cache seeding, refreshing, and rebuilding.
/// </remarks>
public interface IDocumentCacheService
{
    /// <summary>
    /// Gets a published content item by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the content.</param>
    /// <param name="preview">Optional value indicating whether to include unpublished content. If <c>null</c>, uses the default preview setting.</param>
    /// <returns>The published content, or <c>null</c> if not found.</returns>
    Task<IPublishedContent?> GetByKeyAsync(Guid key, bool? preview = null);

    /// <summary>
    /// Gets a published content item by its integer identifier.
    /// </summary>
    /// <param name="id">The integer identifier of the content.</param>
    /// <param name="preview">Optional value indicating whether to include unpublished content. If <c>null</c>, uses the default preview setting.</param>
    /// <returns>The published content, or <c>null</c> if not found.</returns>
    Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null);

    /// <summary>
    /// Gets multiple published content items by their unique keys, fetching any not already cached
    /// from the database in a single batched query rather than one at a time.
    /// </summary>
    /// <param name="keys">The unique keys of the content to retrieve.</param>
    /// <param name="preview">Optional value indicating whether to include unpublished content. If <c>null</c>, uses the default preview setting.</param>
    /// <returns>The published content items that exist, in the same order as <paramref name="keys"/> (missing items omitted).</returns>
    /// <remarks>
    /// Used to materialise sets of keys (e.g. children/descendants) without the per-item database
    /// round trip and scope of repeated <see cref="GetByKeyAsync"/> calls when the cache is cold.
    /// The default implementation falls back to per-key retrieval so existing implementations keep working.
    /// </remarks>
    // TODO (V19): Remove the default implementation and reference to it in the remarks.
    async Task<IReadOnlyList<IPublishedContent>> GetByKeysAsync(IReadOnlyCollection<Guid> keys, bool? preview = null)
    {
        var result = new List<IPublishedContent>(keys.Count);
        foreach (Guid key in keys)
        {
            IPublishedContent? content = await GetByKeyAsync(key, preview);
            if (content is not null)
            {
                result.Add(content);
            }
        }

        return result;
    }

    /// <summary>
    /// Attempts to retrieve a content item from the in-memory converted-content cache without
    /// touching the distributed cache or the database.
    /// </summary>
    /// <param name="key">The unique key of the content.</param>
    /// <param name="preview">Whether to consider unpublished content.</param>
    /// <param name="content">When this method returns, contains the cached published content if a hit was made; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the content was served from the in-memory cache; <c>false</c> if a slower retrieval (HybridCache or database) is required.</returns>
    /// <remarks>
    /// Synchronous fast-path used by sync consumers (e.g. <c>IPublishedContentCache.GetById(bool, Guid)</c>)
    /// to avoid setting up the async state machine on the dominant warm-cache case. On a miss
    /// the caller falls back to the existing async path. The default implementation always
    /// returns <c>false</c> so the caller takes the async path.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    bool TryGetCached(Guid key, bool preview, out IPublishedContent? content)
    {
        content = null;
        return false;
    }

    /// <summary>
    /// Seeds the cache with initial content data.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether content with the specified identifier exists in the cache.
    /// </summary>
    /// <param name="id">The integer identifier of the content.</param>
    /// <param name="preview">A value indicating whether to check for unpublished content.</param>
    /// <returns><c>true</c> if content with the specified identifier exists; otherwise, <c>false</c>.</returns>
    Task<bool> HasContentByIdAsync(int id, bool preview = false);

    /// <summary>
    /// Refreshes the cache entry for the specified content item.
    /// </summary>
    /// <param name="content">The content item to refresh in the cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshContentAsync(IContent content);

    /// <summary>
    /// Removes the specified content item from the cache.
    /// </summary>
    /// <param name="content">The content item to remove from the cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteItemAsync(IContentBase content);

    /// <summary>
    /// Rebuilds the cache for content items of the specified content types.
    /// </summary>
    /// <param name="contentTypeIds">The collection of content type identifiers to rebuild.</param>
    void Rebuild(IReadOnlyCollection<int> contentTypeIds);

    /// <summary>
    /// Gets all published content items of the specified content type.
    /// </summary>
    /// <param name="contentType">The published content type to filter by.</param>
    /// <returns>A collection of published content items of the specified type.</returns>
    IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType);

    /// <summary>
    /// Clears all entries from the memory cache.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearMemoryCacheAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes the memory cache entry for the content item with the specified key.
    /// </summary>
    /// <param name="key">The unique key of the content to refresh.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshMemoryCacheAsync(Guid key);

    /// <summary>
    /// Removes the content item with the specified key from the memory cache.
    /// </summary>
    /// <param name="key">The unique key of the content to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveFromMemoryCacheAsync(Guid key);

    /// <summary>
    /// Rebuilds the memory cache for content items of the specified content types.
    /// </summary>
    /// <param name="contentTypeIds">The collection of content type identifiers to rebuild in memory cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> contentTypeIds);

    /// <summary>
    /// Clears all converted IPublishedContent entries from the in-memory cache,
    /// without rebuilding the underlying database cache or HybridCache entries.
    /// </summary>
    /// <remarks>
    /// Use this when the published model factory is reset (e.g. InMemoryAuto mode), which
    /// invalidates all compiled model types and makes cached instances of any type stale.
    /// </remarks>
    // TODO (V18): Remove default implementation.
    void ClearConvertedContentCache() { }

    /// <summary>
    /// Clears converted IPublishedContent entries for the specified content types from the in-memory cache,
    /// without rebuilding the underlying database cache or HybridCache entries.
    /// </summary>
    /// <remarks>
    /// Use this when the published model factory is NOT reset (e.g. SourceCodeAuto/SourceCodeManual modes),
    /// so only the affected content types need their converted cache cleared.
    /// </remarks>
    /// <param name="contentTypeIds">The IDs of the content types whose converted entries should be cleared.</param>
    // TODO (V18): Remove default implementation.
    void ClearConvertedContentCache(IReadOnlyCollection<int> contentTypeIds) { }
}
