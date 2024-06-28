using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class CacheService : ICacheService
{
    private readonly INuCacheContentRepository _nuCacheContentRepository;

    public CacheService(INuCacheContentRepository nuCacheContentRepository)
    {
        _nuCacheContentRepository = nuCacheContentRepository;
    }
    public Task<IPublishedContent> GetByKey(Guid key) => throw new NotImplementedException();

    public Task<IPublishedContent> GetById(int id) => throw new NotImplementedException();
}
