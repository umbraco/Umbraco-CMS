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
}
