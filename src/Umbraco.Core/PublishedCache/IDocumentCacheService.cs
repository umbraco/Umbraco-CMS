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
public interface IDocumentCacheService : IContentCacheService
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
    /// Gets all published content items of the specified content type.
    /// </summary>
    /// <param name="contentType">The published content type to filter by.</param>
    /// <returns>A collection of published content items of the specified type.</returns>
    IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType);
}
