using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache;

public class MemberCache : IPublishedMemberCache
{
    private readonly IMemberCacheService _memberCacheService;
    private readonly PublishedContentTypeCache _publishedContentTypeCache;

    public MemberCache(IMemberCacheService memberCacheService, IPublishedContentCacheAccessor publishedContentCacheAccessor)
    {
        _memberCacheService = memberCacheService;
        _publishedContentTypeCache = publishedContentCacheAccessor.Get();
    }

    public async Task<IPublishedMember?> GetAsync(IMember member) =>
        await _memberCacheService.Get(member);

    public IPublishedMember? Get(IMember member) => GetAsync(member).GetAwaiter().GetResult();

    public IPublishedContentType GetContentType(int id) => _publishedContentTypeCache.Get(PublishedItemType.Member, id);

    public IPublishedContentType GetContentType(string alias) => _publishedContentTypeCache.Get(PublishedItemType.Member, alias);
}
