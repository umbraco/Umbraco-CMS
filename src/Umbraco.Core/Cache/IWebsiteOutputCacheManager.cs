namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides programmatic eviction of website output cache entries.
/// </summary>
/// <remarks>
///     Use this to evict cached pages from custom code, for example when external data changes
///     that affects a rendered page. All methods are no-ops when output caching is not enabled.
/// </remarks>
public interface IWebsiteOutputCacheManager
{
    /// <summary>
    ///     Evicts the cached page for a specific content item.
    /// </summary>
    /// <param name="contentKey">The key of the content item to evict.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task EvictContentAsync(Guid contentKey, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Evicts all cached pages matching a custom tag.
    /// </summary>
    /// <param name="tag">The cache tag to evict.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Evicts all cached website pages.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task EvictAllAsync(CancellationToken cancellationToken = default);
}
