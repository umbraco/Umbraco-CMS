using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

internal sealed class MemberCacheService : IMemberCacheService
{
    private readonly IPublishedContentFactory _publishedContentFactory;
    private readonly IDatabaseCacheRepository _databaseCacheRepository;
    private readonly ICoreScopeProvider _scopeProvider;

    public MemberCacheService(
        IPublishedContentFactory publishedContentFactory,
        IDatabaseCacheRepository databaseCacheRepository,
        ICoreScopeProvider scopeProvider)
    {
        _publishedContentFactory = publishedContentFactory;
        _databaseCacheRepository = databaseCacheRepository;
        _scopeProvider = scopeProvider;
    }

    public async Task<IPublishedMember?> Get(IMember member) => member is null ? null : _publishedContentFactory.ToPublishedMember(member);

    public void Rebuild(IReadOnlyCollection<int> contentTypeIds)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        _databaseCacheRepository.Rebuild(memberTypeIds: contentTypeIds.ToList());
        scope.Complete();
    }
}
