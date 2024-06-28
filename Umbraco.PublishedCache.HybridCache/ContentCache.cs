using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal sealed class ContentCache : IPublishedHybridCache
{
    private readonly Microsoft.Extensions.Caching.Hybrid.HybridCache _cache;
    private readonly ICacheService _cacheService;

    public ContentCache(Microsoft.Extensions.Caching.Hybrid.HybridCache cache, ICacheService cacheService)
    {
        _cache = cache;
        _cacheService = cacheService;
    }

    public IPublishedContent? GetById(bool preview, int contentId) => throw new NotImplementedException();

    public IPublishedContent? GetById(bool preview, Guid contentId) => throw new NotImplementedException();

    public IPublishedContent? GetById(int contentId) =>
        _cache.GetOrCreateAsync(
            $"{contentId}", // Unique key to the cache entry
            async cancel => _cacheService.GetById(contentId)).GetAwaiter().GetResult().GetAwaiter().GetResult();

    public IPublishedContent? GetById(Guid contentId) => throw new NotImplementedException();

    public bool HasById(bool preview, int contentId) => throw new NotImplementedException();

    public bool HasById(int contentId) => throw new NotImplementedException();

    public bool HasContent(bool preview) => throw new NotImplementedException();

    public bool HasContent() => throw new NotImplementedException();
}
