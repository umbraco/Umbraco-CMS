using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class MediaCache : IPublishedMediaHybridCache
{
    private readonly IMediaCacheService _mediaCacheService;

    public MediaCache(IMediaCacheService mediaCacheService) => _mediaCacheService = mediaCacheService;

    public async Task<IPublishedContent?> GetByIdAsync(int id) => await _mediaCacheService.GetByIdAsync(id);

    public async Task<IPublishedContent?> GetByKeyAsync(Guid key) => await _mediaCacheService.GetByKeyAsync(key);

    public async Task<bool> HasByIdAsync(int id) => await _mediaCacheService.HasContentByIdAsync(id);
}
