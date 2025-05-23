using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal class MemberCacheService : IMemberCacheService
{
    private readonly IPublishedContentFactory _publishedContentFactory;

    public MemberCacheService(IPublishedContentFactory publishedContentFactory) => _publishedContentFactory = publishedContentFactory;

    public async Task<IPublishedMember?> Get(IMember member) => member is null ? null : _publishedContentFactory.ToPublishedMember(member);
}
