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

    public IPublishedContent? GetById(bool preview, Guid contentId) =>
        GetByIdAsync(contentId).GetAwaiter().GetResult();


    public IPublishedContent? GetById(int contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

    public IPublishedContent? GetById(Guid contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

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
