using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Defines operations for the element cache service.
/// </summary>
/// <remarks>
/// This service provides access to published element content with caching support,
/// including operations for cache refreshing and rebuilding.
/// </remarks>
public interface IElementCacheService : IContentCacheService
{
    /// <summary>
    /// Gets a published element by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the element.</param>
    /// <param name="preview">Optional value indicating whether to include unpublished content. If <c>null</c>, defaults to <c>false</c>.</param>
    /// <returns>The published element, or <c>null</c> if not found.</returns>
    Task<IPublishedElement?> GetByKeyAsync(Guid key, bool? preview = null);

    /// <summary>
    /// Gets a published element by its unique key, for synchronous callers.
    /// </summary>
    /// <param name="key">The unique key of the element.</param>
    /// <param name="preview">Optional value indicating whether to include unpublished content. If <c>null</c>, uses the default preview setting.</param>
    /// <returns>The published element, or <c>null</c> if not found.</returns>
    /// <remarks>
    /// Implementations should avoid blocking on <see cref="GetByKeyAsync"/>, and read synchronously from the
    /// backing stores that support it: a caller blocked on asynchronous work needs a second thread to complete it.
    /// A store with no synchronous read, for example a distributed cache, may still have to be waited on.
    /// The default implementation blocks on <see cref="GetByKeyAsync"/>.
    /// </remarks>
    // TODO (V20): Remove the default implementation and reference to it in the remarks.
    IPublishedElement? GetByKey(Guid key, bool? preview = null)
        => GetByKeyAsync(key, preview).GetAwaiter().GetResult();

    /// <summary>
    /// Gets multiple published elements by their unique keys, fetching any not already cached
    /// from the database in a single batched query rather than one at a time.
    /// </summary>
    /// <param name="keys">The unique keys of the elements to retrieve.</param>
    /// <param name="preview">Optional value indicating whether to include unpublished content. If <c>null</c>, uses the default preview setting.</param>
    /// <returns>The published elements that exist, in the same order as <paramref name="keys"/> (missing elements omitted).</returns>
    /// <remarks>
    /// Used to materialise sets of keys without the per-element database round trip of repeated
    /// <see cref="GetByKeyAsync"/> calls when the cache is cold.
    /// The default implementation falls back to per-key retrieval so existing implementations keep working.
    /// </remarks>
    // TODO (V20): Remove the default implementation and reference to it in the remarks.
    async Task<IReadOnlyList<IPublishedElement>> GetByKeysAsync(IReadOnlyCollection<Guid> keys, bool? preview = null)
    {
        var result = new List<IPublishedElement>(keys.Count);
        foreach (Guid key in keys)
        {
            IPublishedElement? element = await GetByKeyAsync(key, preview);
            if (element is not null)
            {
                result.Add(element);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets multiple published elements by their unique keys, for synchronous callers.
    /// </summary>
    /// <param name="keys">The unique keys of the elements to retrieve.</param>
    /// <param name="preview">Optional value indicating whether to include unpublished content. If <c>null</c>, uses the default preview setting.</param>
    /// <returns>The published elements that exist, in the same order as <paramref name="keys"/> (missing elements omitted).</returns>
    /// <remarks>See <see cref="GetByKey"/>. The default implementation blocks on <see cref="GetByKeysAsync"/>.</remarks>
    // TODO (V20): Remove the default implementation and reference to it in the remarks.
    IReadOnlyList<IPublishedElement> GetByKeys(IReadOnlyCollection<Guid> keys, bool? preview = null)
        => GetByKeysAsync(keys, preview).GetAwaiter().GetResult();

    /// <summary>
    /// Attempts to retrieve an element from the in-memory converted-content cache without
    /// touching the distributed cache or the database.
    /// </summary>
    /// <param name="key">The unique key of the element.</param>
    /// <param name="preview">Whether to consider unpublished content.</param>
    /// <param name="element">When this method returns, contains the cached published element if a hit was made; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the element was served from the in-memory cache; <c>false</c> if a slower retrieval (HybridCache or database) is required.</returns>
    /// <remarks>
    /// Synchronous fast-path used by sync consumers (e.g. <c>IPublishedElementCache.GetById(bool, Guid)</c>
    /// and property value converters that sync-over-async) to avoid setting up the async state
    /// machine on the dominant warm-cache case. On a miss the caller falls back to the existing
    /// async path. The default implementation always returns <c>false</c> so the caller takes the
    /// async path.
    /// </remarks>
    bool TryGetCached(Guid key, bool preview, out IPublishedElement? element);

    /// <summary>
    /// Refreshes the cache entry for the specified element.
    /// </summary>
    /// <param name="element">The element to refresh in the cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshElementAsync(IElement element);

    /// <summary>
    /// Gets all published elements of the specified content type.
    /// </summary>
    /// <param name="contentType">The published content type to filter by.</param>
    /// <returns>A collection of published elements of the specified type.</returns>
    IEnumerable<IPublishedElement> GetByContentType(IPublishedContentType contentType);
}
