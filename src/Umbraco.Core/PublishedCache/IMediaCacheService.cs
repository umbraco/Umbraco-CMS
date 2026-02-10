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
public interface IMediaCacheService
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
    /// Rebuilds the memory cache for media items of the specified media types.
    /// </summary>
    /// <param name="mediaTypeIds">The collection of media type identifiers to rebuild in memory cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RebuildMemoryCacheByContentTypeAsync(IEnumerable<int> mediaTypeIds);

    /// <summary>
    /// Clears all entries from the memory cache.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearMemoryCacheAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes the memory cache entry for the media item with the specified key.
    /// </summary>
    /// <param name="key">The unique key of the media to refresh.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshMemoryCacheAsync(Guid key);

    /// <summary>
    /// Removes the media item with the specified key from the memory cache.
    /// </summary>
    /// <param name="key">The unique key of the media to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveFromMemoryCacheAsync(Guid key);

    /// <summary>
    /// Removes the specified media item from the cache.
    /// </summary>
    /// <param name="media">The media item to remove from the cache.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteItemAsync(IContentBase media);

    /// <summary>
    /// Seeds the cache with initial media data.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rebuilds the cache for media items of the specified media types.
    /// </summary>
    /// <param name="contentTypeIds">The collection of media type identifiers to rebuild.</param>
    void Rebuild(IReadOnlyCollection<int> contentTypeIds);

    /// <summary>
    /// Gets all published media items of the specified content type.
    /// </summary>
    /// <param name="contentType">The published content type to filter by.</param>
    /// <returns>A collection of published media items of the specified type.</returns>
    IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType);
}
