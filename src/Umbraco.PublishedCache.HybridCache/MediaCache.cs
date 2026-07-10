using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public sealed class MediaCache : IPublishedMediaCache
{
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly IMediaNavigationQueryService _mediaNavigationQueryService;

    public MediaCache(IMediaCacheService mediaCacheService, IPublishedContentTypeCache publishedContentTypeCache, IMediaNavigationQueryService mediaNavigationQueryService)
    {
        _mediaCacheService = mediaCacheService;
        _publishedContentTypeCache = publishedContentTypeCache;
        _mediaNavigationQueryService = mediaNavigationQueryService;
    }

    public async Task<IPublishedContent?> GetByIdAsync(int id) => await _mediaCacheService.GetByIdAsync(id);

    public async Task<IPublishedContent?> GetByIdAsync(Guid key) => await _mediaCacheService.GetByKeyAsync(key);

    public IPublishedContent? GetById(bool preview, int contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

    // Media has no draft/preview dimension, so preview is ignored and this delegates to the
    // single-argument Guid overload where the sync fast path lives.
    public IPublishedContent? GetById(bool preview, Guid contentId) => GetById(contentId);

    public IPublishedContent? GetById(int contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

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
