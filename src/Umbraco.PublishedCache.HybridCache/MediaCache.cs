using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

/// <summary>
/// Provides access to published media items held in the hybrid cache.
/// </summary>
public sealed class MediaCache : IPublishedMediaCache
{
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IMediaNavigationQueryService _mediaNavigationQueryService;

    // TODO (V19): Remove the unused parameters from the constructor.

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaCache"/> class.
    /// </summary>
    /// <param name="mediaCacheService">The service that retrieves and caches published media nodes.</param>
    /// <param name="publishedContentTypeCache">The cache of published content types.</param>
    /// <param name="mediaNavigationQueryService">The service used to query the media navigation structure.</param>
    public MediaCache(
        IMediaCacheService mediaCacheService,
#pragma warning disable IDE0060 // Remove unused parameter
        IPublishedContentTypeCache publishedContentTypeCache,
#pragma warning restore IDE0060 // Remove unused parameter
        IMediaNavigationQueryService mediaNavigationQueryService)
    {
        _mediaCacheService = mediaCacheService;
        _mediaNavigationQueryService = mediaNavigationQueryService;
    }

    /// <inheritdoc/>
    public async Task<IPublishedContent?> GetByIdAsync(int id) => await _mediaCacheService.GetByIdAsync(id);

    /// <inheritdoc/>
    public async Task<IPublishedContent?> GetByIdAsync(Guid key) => await _mediaCacheService.GetByKeyAsync(key);

    /// <inheritdoc/>
    public IPublishedContent? GetById(bool preview, int contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

    /// <inheritdoc/>
    /// <remarks>
    /// Media has no draft/preview dimension, so preview is ignored and this delegates to the
    /// single-argument Guid overload where the sync fast path lives.
    /// </remarks>
    public IPublishedContent? GetById(bool preview, Guid contentId) => GetById(contentId);

    /// <inheritdoc/>
    public IPublishedContent? GetById(int contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public IPublishedContent? GetById(Guid contentId)
    {
        // Sync fast path: when the converted-content L0 cache already holds the item we can
        // return it without spinning up an async state machine. This is the dominant case on
        // a warm site and is hit per-key by the FilterAvailable lazy chain. On a miss we fall
        // through to the async path which handles HybridCache (L1/L2) and database lookups.
        if (_mediaCacheService.TryGetCached(contentId, out IPublishedContent? cached))
        {
            return cached;
        }

        return GetByIdAsync(contentId).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the published media items at the root of the media tree.
    /// </summary>
    /// <param name="preview">A value indicating whether to consider unpublished items. Media has no draft state, so this has no effect.</param>
    /// <param name="culture">
    /// The culture to filter root media by. When <c>null</c>, all root media are returned; otherwise only those that are
    /// invariant or vary for the specified culture are returned.
    /// </param>
    /// <returns>The published media items at root level available for the specified culture.</returns>
    public IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null)
    {
        if (_mediaNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys) is false)
        {
            return [];
        }

        IEnumerable<IPublishedContent> rootContent = rootKeys.Select(key => GetById(preview, key)).WhereNotNull();
        return culture is null ? rootContent : rootContent.Where(x => x.IsInvariantOrHasCulture(culture));
    }
}
