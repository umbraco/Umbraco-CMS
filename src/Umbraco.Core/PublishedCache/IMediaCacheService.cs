using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
/// Defines operations for the media cache service.
/// </summary>
/// <remarks>
/// This service provides access to published media content with caching support,
/// including operations for cache seeding, refreshing, and rebuilding.
/// </remarks>
public interface IMediaCacheService : IContentCacheService
{
    /// <summary>
    /// Gets a published media item by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the media.</param>
    /// <returns>The published media content, or <c>null</c> if not found.</returns>
    Task<IPublishedContent?> GetByKeyAsync(Guid key);

    /// <summary>
    /// Gets a published media item by its integer identifier.
    /// </summary>
    /// <param name="id">The integer identifier of the media.</param>
    /// <returns>The published media content, or <c>null</c> if not found.</returns>
    Task<IPublishedContent?> GetByIdAsync(int id);

    /// <summary>
    /// Determines whether media with the specified identifier exists in the cache.
    /// </summary>
    /// <param name="id">The integer identifier of the media.</param>
    /// <returns><c>true</c> if media with the specified identifier exists; otherwise, <c>false</c>.</returns>
    Task<bool> HasContentByIdAsync(int id);

    /// <summary>
    /// Refreshes the cache entry for the specified media item.
    /// </summary>
    /// <param name="media">The media item to refresh in the cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshMediaAsync(IMedia media);

    /// <summary>
    /// Gets all published media items of the specified content type.
    /// </summary>
    /// <param name="contentType">The published content type to filter by.</param>
    /// <returns>A collection of published media items of the specified type.</returns>
    IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType);
}
