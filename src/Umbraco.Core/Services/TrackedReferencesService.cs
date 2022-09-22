using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public class TrackedReferencesService : ITrackedReferencesService
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly ITrackedReferencesRepository _trackedReferencesRepository;

    [Obsolete("Please use ctor that does not take an IEntityService, scheduled for removal in V12")]
    public TrackedReferencesService(
        ITrackedReferencesRepository trackedReferencesRepository,
        ICoreScopeProvider scopeProvider,
        IEntityService entityService) : this(trackedReferencesRepository, scopeProvider)
    {
    }

    public TrackedReferencesService(
        ITrackedReferencesRepository trackedReferencesRepository,
        ICoreScopeProvider scopeProvider)
    {
        _trackedReferencesRepository = trackedReferencesRepository;
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    ///     Gets a paged result of items which are in relation with the current item.
    ///     Basically, shows the items which depend on the current item.
    /// </summary>
    public PagedResult<RelationItem> GetPagedRelationsForItem(int id, long pageIndex, int pageSize, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItem> items = _trackedReferencesRepository.GetPagedRelationsForItem(id, pageIndex, pageSize, filterMustBeIsDependency, out var totalItems);

        return new PagedResult<RelationItem>(totalItems, pageIndex + 1, pageSize) { Items = items };
    }

    /// <summary>
    ///     Gets a paged result of items used in any kind of relation from selected integer ids.
    /// </summary>
    public PagedResult<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItem> items = _trackedReferencesRepository.GetPagedItemsWithRelations(ids, pageIndex, pageSize, filterMustBeIsDependency, out var totalItems);

        return new PagedResult<RelationItem>(totalItems, pageIndex + 1, pageSize) { Items = items };
    }

    /// <summary>
    ///     Gets a paged result of the descending items that have any references, given a parent id.
    /// </summary>
    public PagedResult<RelationItem> GetPagedDescendantsInReferences(int parentId, long pageIndex, int pageSize, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IEnumerable<RelationItem> items = _trackedReferencesRepository.GetPagedDescendantsInReferences(
            parentId,
            pageIndex,
            pageSize,
            filterMustBeIsDependency,
            out var totalItems);
        return new PagedResult<RelationItem>(totalItems, pageIndex + 1, pageSize) { Items = items };
    }

    public PagedModel<RelationItemModel> GetPagedRelationsForItem(int id, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedRelationsForItem(id, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return pagedModel;
    }

    public PagedModel<RelationItemModel> GetPagedDescendantsInReferences(int parentId, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedDescendantsInReferences(
            parentId,
            skip,
            take,
            filterMustBeIsDependency,
            out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return pagedModel;
    }

    public PagedModel<RelationItemModel> GetPagedItemsWithRelations(int[] ids, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedItemsWithRelations(ids, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return pagedModel;
    }
}
