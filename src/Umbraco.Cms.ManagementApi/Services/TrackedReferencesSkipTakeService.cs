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
    public PagedViewModel<RelationItemModel> GetPagedRelationsForItem(int id, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesSkipTakeRepository.GetPagedRelationsForItem(id, skip, take, filterMustBeIsDependency, out var totalItems);

        return new PagedViewModel<RelationItemModel> { Total = totalItems, Items = items };
    }

    public PagedViewModel<RelationItemModel> GetPagedDescendantsInReferences(int parentId, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IEnumerable<RelationItemModel> items = _trackedReferencesSkipTakeRepository.GetPagedDescendantsInReferences(
            parentId,
            skip,
            take,
            filterMustBeIsDependency,
            out var totalItems);
        return new PagedViewModel<RelationItemModel> { Total = totalItems, Items = items };
    }

    public PagedViewModel<RelationItemModel> GetPagedItemsWithRelations(int[] ids, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesSkipTakeRepository.GetPagedItemsWithRelations(ids, skip, take, filterMustBeIsDependency, out var totalItems);

        return new PagedViewModel<RelationItemModel> { Total = totalItems, Items = items };
    }
}
