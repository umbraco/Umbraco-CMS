using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public sealed class DocumentCache : IPublishedContentCache
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;

    public DocumentCache(IDocumentCacheService documentCacheService, IPublishedContentTypeCache publishedContentTypeCache)
    {
        _documentCacheService = documentCacheService;
        _publishedContentTypeCache = publishedContentTypeCache;
    }

    public async Task<IPublishedContent?> GetByIdAsync(int id, bool preview = false) => await _documentCacheService.GetByIdAsync(id, preview);


    public async Task<IPublishedContent?> GetByIdAsync(Guid key, bool preview = false) => await _documentCacheService.GetByKeyAsync(key, preview);

    public IPublishedContent? GetById(bool preview, int contentId) => GetByIdAsync(contentId, preview).GetAwaiter().GetResult();

    public IPublishedContent? GetById(bool preview, Guid contentId) => GetByIdAsync(contentId, preview).GetAwaiter().GetResult();


    public IPublishedContent? GetById(int contentId) => GetByIdAsync(contentId, false).GetAwaiter().GetResult();

    public IPublishedContent? GetById(Guid contentId) => GetByIdAsync(contentId, false).GetAwaiter().GetResult();

    public IPublishedContentType? GetContentType(int id) => _publishedContentTypeCache.Get(PublishedItemType.Content, id);

    public IPublishedContentType? GetContentType(string alias) => _publishedContentTypeCache.Get(PublishedItemType.Content, alias);


    public IPublishedContentType? GetContentType(Guid key) => _publishedContentTypeCache.Get(PublishedItemType.Content, key);

    // FIXME: These need to be refactored when removing nucache
    // Thats the time where we can change the IPublishedContentCache interface.

    public IPublishedContent? GetById(bool preview, Udi contentId) => throw new NotImplementedException();

    public IPublishedContent? GetById(Udi contentId) => throw new NotImplementedException();

    public IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null) => throw new NotImplementedException();

    public IEnumerable<IPublishedContent> GetAtRoot(string? culture = null) => throw new NotImplementedException();

    public bool HasContent(bool preview) => throw new NotImplementedException();

    public bool HasContent() => throw new NotImplementedException();

    public IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType) => throw new NotImplementedException();

    public IPublishedContent? GetByRoute(bool preview, string route, bool? hideTopLevelNode = null, string? culture = null) => throw new NotImplementedException();

    public IPublishedContent? GetByRoute(string route, bool? hideTopLevelNode = null, string? culture = null) => throw new NotImplementedException();

    public string? GetRouteById(bool preview, int contentId, string? culture = null) => throw new NotImplementedException();

    public string? GetRouteById(int contentId, string? culture = null) => throw new NotImplementedException();
}
