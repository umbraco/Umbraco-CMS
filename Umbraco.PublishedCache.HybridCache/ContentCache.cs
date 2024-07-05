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

    public async Task<IPublishedContent?> GetById(int contentId, bool preview = false) =>
        await _cache.GetOrCreateAsync(
            $"{contentId}", // Unique key to the cache entry
            async cancel =>
            {
                IPublishedContent? content = await _cacheService.GetById(contentId, preview);
                return content;
            });

    public Task<IPublishedContent?> GetById(Guid contentId, bool preview = false) => throw new NotImplementedException();

    public Task<bool> HasById(bool preview, int contentId) => throw new NotImplementedException();

    public Task<bool> HasById(int contentId) => throw new NotImplementedException();

    public Task<bool> HasContent(bool preview) => throw new NotImplementedException();

    public Task<bool> HasContent() => throw new NotImplementedException();
}
