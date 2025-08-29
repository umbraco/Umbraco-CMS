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

    public IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null)
    {
        if (_documentNavigationQueryService.TryGetRootKeys(out IEnumerable<Guid> rootKeys) is false)
        {
            return [];
        }

        IEnumerable<IPublishedContent> rootContent = rootKeys.Select(key => GetById(preview, key)).WhereNotNull();
        return culture is null ? rootContent : rootContent.Where(x => x.IsInvariantOrHasCulture(culture));
    }
}
