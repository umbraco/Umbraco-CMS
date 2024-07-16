using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal class MemberCacheService : IMemberCacheService
{
    private readonly IMemberEditingService _memberEditingService;
    private readonly IPublishedContentFactory _publishedContentFactory;

    public MemberCacheService(IMemberEditingService memberEditingService, IPublishedContentFactory publishedContentFactory)
    {
        _memberEditingService = memberEditingService;
        _publishedContentFactory = publishedContentFactory;
    }

    public async Task<IPublishedMember?> GetByKey(Guid key)
    {
        // We're not caching here as we have never done so in the past
        // This is because members are both Content & Identities.
        IMember? member = await _memberEditingService.GetAsync(key);

        return member is null ? null : _publishedContentFactory.ToPublishedMember(member);
    }
}
