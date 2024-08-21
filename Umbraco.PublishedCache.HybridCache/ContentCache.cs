using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public sealed class ContentCache : IPublishedContentHybridCache
{
    private readonly IContentCacheService _contentCacheService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly PublishedContentTypeCache _contentTypeCache;

    public ContentCache(IContentCacheService contentCacheService, IIdKeyMap idKeyMap, IPublishedContentCacheAccessor publishedContentCacheAccessor)
    {
        _contentCacheService = contentCacheService;
        _idKeyMap = idKeyMap;
        _contentTypeCache = publishedContentCacheAccessor.Get();
    }

    public async Task<IPublishedContent?> GetByIdAsync(int id, bool preview = false)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(id, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        return await _contentCacheService.GetByIdAsync(id, preview);
    }


    public async Task<IPublishedContent?> GetByIdAsync(Guid key, bool preview = false) => await _contentCacheService.GetByKeyAsync(key, preview);

    public async Task<bool> HasByIdAsync(int id, bool preview = false) => await _contentCacheService.HasContentByIdAsync(id, preview);

    public IPublishedContent? GetById(bool preview, int contentId) => GetByIdAsync(contentId, preview).GetAwaiter().GetResult();

    public IPublishedContent? GetById(bool preview, Guid contentId) => GetByIdAsync(contentId, preview).GetAwaiter().GetResult();


    public IPublishedContent? GetById(int contentId) => GetByIdAsync(contentId, false).GetAwaiter().GetResult();

    public IPublishedContent? GetById(Guid contentId) => GetByIdAsync(contentId, false).GetAwaiter().GetResult();


    public bool HasById(bool preview, int contentId) => HasByIdAsync(contentId, preview).GetAwaiter().GetResult();

    public bool HasById(int contentId) => HasByIdAsync(contentId, false).GetAwaiter().GetResult();

    public IPublishedContentType? GetContentType(int id) => _contentTypeCache.Get(PublishedItemType.Content, id);

    public IPublishedContentType? GetContentType(string alias) => _contentTypeCache.Get(PublishedItemType.Content, alias);


    public IPublishedContentType? GetContentType(Guid key) => _contentTypeCache.Get(PublishedItemType.Content, key);

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
