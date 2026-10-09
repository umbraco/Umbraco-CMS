namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides programmatic eviction of Delivery API output cache entries.
/// </summary>
/// <remarks>
///     Use this to evict cached responses from custom code, for example when external data changes
///     that affects a Delivery API response. All methods are no-ops when output caching is not enabled.
/// </remarks>
public interface IDeliveryApiOutputCacheManager
{
    /// <summary>
    ///     Evicts the cached Delivery API response for a specific content item.
    /// </summary>
    /// <param name="contentKey">The key of the content item to evict.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task EvictContentAsync(Guid contentKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Evicts the cached Delivery API response for a specific media item.
    /// </summary>
    /// <param name="mediaKey">The key of the media item to evict.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task EvictMediaAsync(Guid mediaKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Evicts all cached Delivery API responses matching a custom tag.
    /// </summary>
    /// <param name="tag">The cache tag to evict.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Evicts all cached Delivery API content responses.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task EvictAllContentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Evicts all cached Delivery API media responses.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task EvictAllMediaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Evicts all cached Delivery API responses (content and media).
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task EvictAllAsync(CancellationToken cancellationToken = default);
}
