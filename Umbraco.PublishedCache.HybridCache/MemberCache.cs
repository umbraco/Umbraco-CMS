using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public class MemberCache : IPublishedMemberHybridCache
{
    private readonly IMemberCacheService _memberCacheService;
    private readonly PublishedContentTypeCache _publishedContentTypeCache;

    public MemberCache(IMemberCacheService memberCacheService, IPublishedContentCacheAccessor publishedContentCacheAccessor)
    {
        _memberCacheService = memberCacheService;
        _publishedContentTypeCache = publishedContentCacheAccessor.Get();
    }

    public async Task<IPublishedMember?> GetById(Guid key) =>
        await _memberCacheService.GetByKey(key);

    // FIXME - these need to be refactored when removing nucache
    public IPublishedContent? Get(IMember member) => GetById(member.Key).GetAwaiter().GetResult();

    public IPublishedContentType GetContentType(int id) => _publishedContentTypeCache.Get(PublishedItemType.Member, id);

    public IPublishedContentType GetContentType(string alias) => _publishedContentTypeCache.Get(PublishedItemType.Member, alias);
}
