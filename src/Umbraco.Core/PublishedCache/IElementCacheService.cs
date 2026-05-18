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
