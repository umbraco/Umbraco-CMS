using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

internal class MemberCache : IPublishedMemberHybridCache
{
    private readonly IMemberCacheService _memberCacheService;

    public MemberCache(IMemberCacheService memberCacheService)
    {
        _memberCacheService = memberCacheService;
    }

    public async Task<IPublishedMember?> GetById(Guid key) =>
        await _memberCacheService.GetByKey(key);
}
