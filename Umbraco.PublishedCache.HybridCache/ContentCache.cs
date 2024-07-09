using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal sealed class ContentCache : IPublishedHybridCache
{
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _cache;
    private readonly ICacheService _cacheService;
    private readonly IPublishedContentFactory _publishedContentFactory;

    public ContentCache(
        Microsoft.Extensions.Caching.Hybrid.HybridCache cache,
        ICacheService cacheService,
        IPublishedContentFactory publishedContentFactory)
    {
        _cache = cache;
        _cacheService = cacheService;
        _publishedContentFactory = publishedContentFactory;
    }

    public async Task<IPublishedContent?> GetById(int contentId, bool preview = false)
    {
        ContentCacheNode? contentCacheNode = await _cache.GetOrCreateAsync(
            $"{contentId}", // Unique key to the cache entry
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
