using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.New.Cms.Core.Models.TrackedReferences;
using Umbraco.New.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.ManagementApi.Services;

public class TrackedReferencesSkipTakeService : ITrackedReferencesSkipTakeService
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ITrackedReferencesSkipTakeRepository _trackedReferencesSkipTakeRepository;

    public TrackedReferencesSkipTakeService(ICoreScopeProvider scopeProvider, ITrackedReferencesSkipTakeRepository trackedReferencesSkipTakeRepository)
    {
        _scopeProvider = scopeProvider;
        _trackedReferencesSkipTakeRepository = trackedReferencesSkipTakeRepository;
    }
    public IEnumerable<RelationItemModel> GetPagedRelationsForItem(int id, long skip, long take, bool filterMustBeIsDependency, out long totalItems)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesSkipTakeRepository.GetPagedRelationsForItem(id, skip, take, filterMustBeIsDependency, out totalItems);

        return items;
    }

    public IEnumerable<RelationItemModel> GetPagedDescendantsInReferences(int parentId, long skip, long take, bool filterMustBeIsDependency, out long totalItems)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IEnumerable<RelationItemModel> items = _trackedReferencesSkipTakeRepository.GetPagedDescendantsInReferences(
            parentId,
            skip,
            take,
            filterMustBeIsDependency,
            out totalItems);
        return items;
    }

    public IEnumerable<RelationItemModel> GetPagedItemsWithRelations(int[] ids, long skip, long take, bool filterMustBeIsDependency, out long totalItems)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesSkipTakeRepository.GetPagedItemsWithRelations(ids, skip, take, filterMustBeIsDependency, out totalItems);

        return items;
    }
}
