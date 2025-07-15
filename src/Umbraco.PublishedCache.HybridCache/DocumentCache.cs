using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public sealed class DocumentCache : IPublishedContentCache
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly IDocumentNavigationQueryService _documentNavigationQueryService;
    private readonly IDocumentUrlService _documentUrlService;
    private readonly Lazy<IPublishedUrlProvider> _publishedUrlProvider;

    public DocumentCache(
        IDocumentCacheService documentCacheService,
        IPublishedContentTypeCache publishedContentTypeCache,
        IDocumentNavigationQueryService documentNavigationQueryService,
        IDocumentUrlService documentUrlService,
        Lazy<IPublishedUrlProvider> publishedUrlProvider)
    {
        _documentCacheService = documentCacheService;
        _publishedContentTypeCache = publishedContentTypeCache;
        _documentNavigationQueryService = documentNavigationQueryService;
        _documentUrlService = documentUrlService;
        _publishedUrlProvider = publishedUrlProvider;
    }

    public async Task<IPublishedContent?> GetByIdAsync(int id, bool? preview = null) => await _documentCacheService.GetByIdAsync(id, preview);

    public async Task<IPublishedContent?> GetByIdAsync(Guid key, bool? preview = null) => await _documentCacheService.GetByKeyAsync(key, preview);

    public IPublishedContent? GetById(bool preview, int contentId) => GetByIdAsync(contentId, preview).GetAwaiter().GetResult();

    public IPublishedContent? GetById(bool preview, Guid contentId) => GetByIdAsync(contentId, preview).GetAwaiter().GetResult();


    public IPublishedContent? GetById(int contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

    public IPublishedContent? GetById(Guid contentId) => GetByIdAsync(contentId).GetAwaiter().GetResult();

    // TODO: These are all obsolete and should be removed

    [Obsolete("Scheduled for removal in v17")]
    public IPublishedContent? GetById(bool preview, Udi contentId)
    {
        if(contentId is not GuidUdi guidUdi)
        {
            throw new NotSupportedException("Only GuidUdi is supported");
        }

        return GetById(preview, guidUdi.Guid);
    }

    [Obsolete("Scheduled for removal in v17")]
    public IPublishedContent? GetById(Udi contentId)
    {
        if(contentId is not GuidUdi guidUdi)
        {
            throw new NotSupportedException("Only GuidUdi is supported");
        }

        return GetById(guidUdi.Guid);
    }

    public IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null)
    {
        _documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);

        IEnumerable<IPublishedContent> rootContent = rootKeys.Select(key => GetById(preview, key)).WhereNotNull();
        return culture is null ? rootContent : rootContent.Where(x => x.IsInvariantOrHasCulture(culture));
    }

    public IEnumerable<IPublishedContent> GetAtRoot(string? culture = null)
    {
        _documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys);

        IEnumerable<IPublishedContent> rootContent = rootKeys.Select(key => GetById(key)).WhereNotNull();
        return culture is null ? rootContent : rootContent.Where(x => x.IsInvariantOrHasCulture(culture));
    }

    [Obsolete("Scheduled for removal in v17")]
    public bool HasContent(bool preview) => HasContent();

    [Obsolete("Scheduled for removal in v17")]
    public bool HasContent() => _documentUrlService.HasAny();

    [Obsolete("Use IPublishedUrlProvider.GetUrl instead, scheduled for removal in v17")]
    public IPublishedContent? GetByRoute(bool preview, string route, bool? hideTopLevelNode = null, string? culture = null)
    {
        Guid? key = _documentUrlService.GetDocumentKeyByRoute(route, culture, null, preview);
        return key is not null ? GetById(preview, key.Value) : null;
    }

    [Obsolete("Use IPublishedUrlProvider.GetUrl instead, scheduled for removal in v17")]
    public IPublishedContent? GetByRoute(string route, bool? hideTopLevelNode = null, string? culture = null)
    {
        Guid? key = _documentUrlService.GetDocumentKeyByRoute(route, culture, null, false);
        return key is not null ? GetById(key.Value) : null;
    }

    [Obsolete("Use IPublishedUrlProvider.GetUrl instead, scheduled for removal in v17")]
    public string? GetRouteById(bool preview, int contentId, string? culture = null)
    {
        IPublishedContent? content = GetById(preview, contentId);

        return content is not null ? _publishedUrlProvider.Value.GetUrl(content, UrlMode.Relative, culture) : null;
    }

    [Obsolete("Use IPublishedUrlProvider.GetUrl instead, scheduled for removal in v17")]
    public string? GetRouteById(int contentId, string? culture = null) => GetRouteById(false, contentId, culture);
}
