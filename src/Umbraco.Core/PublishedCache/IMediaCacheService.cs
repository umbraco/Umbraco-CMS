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
    /// Attempts to retrieve a media item from the in-memory converted-content cache without
    /// touching the distributed cache or the database.
    /// </summary>
    /// <param name="key">The unique key of the media.</param>
    /// <param name="content">When this method returns, contains the cached published media if a hit was made; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if the media was served from the in-memory cache; <c>false</c> if a slower retrieval (HybridCache or database) is required.</returns>
    /// <remarks>
    /// Synchronous fast-path used by sync consumers (e.g. <c>IPublishedMediaCache.GetById(bool, Guid)</c>)
    /// to avoid setting up the async state machine on the dominant warm-cache case. On a miss
    /// the caller falls back to the existing async path. The default implementation always
    /// returns <c>false</c> so the caller takes the async path.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    bool TryGetCached(Guid key, out IPublishedContent? content)
    {
        content = null;
        return false;
    }

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
