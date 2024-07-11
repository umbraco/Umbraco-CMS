using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal sealed class ContentCache : IPublishedHybridCache
{
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _cache;
    private readonly ICacheService _cacheService;
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly IIdKeyMap _idKeyMap;

    public ContentCache(
        Microsoft.Extensions.Caching.Hybrid.HybridCache cache,
        ICacheService cacheService,
        IPublishedContentFactory publishedContentFactory,
        IIdKeyMap idKeyMap)
    {
        _cache = cache;
        _cacheService = cacheService;
        _publishedContentFactory = publishedContentFactory;
        _idKeyMap = idKeyMap;
    }

    public async Task<IPublishedContent?> GetById(int contentId, bool preview = false)
    {
        Attempt<Guid> keyAttempt = _idKeyMap.GetKeyForId(contentId, UmbracoObjectTypes.Document);
        if (keyAttempt.Success is false)
        {
            return null;
        }

        ContentCacheNode? contentCacheNode = await _cache.GetOrCreateAsync(
            $"{keyAttempt.Result}", // Unique key to the cache entry
            async cancel => await _cacheService.GetById(contentId, preview));

        return contentCacheNode is null ? null : _publishedContentFactory.ToIPublishedContent(contentCacheNode, preview);
    }

    public async Task<IPublishedContent?> GetById(Guid contentId, bool preview = false)
    {
        ContentCacheNode? contentCacheNode = await _cache.GetOrCreateAsync(
            $"{contentId}", // Unique key to the cache entry
            async cancel => await _cacheService.GetByKey(contentId, preview));

        return contentCacheNode is null ? null : _publishedContentFactory.ToIPublishedContent(contentCacheNode, preview);
    }

    public Task<bool> HasById(int contentId, bool preview = false) => throw new NotImplementedException();

    public Task<bool> HasContent(bool preview = false) => throw new NotImplementedException();
}
