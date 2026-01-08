using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

public class TrackedReferencesService : ITrackedReferencesService
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IEntityService _entityService;
    private readonly ITrackedReferencesRepository _trackedReferencesRepository;


    public TrackedReferencesService(
        ITrackedReferencesRepository trackedReferencesRepository,
        ICoreScopeProvider scopeProvider,
        IEntityService entityService)
    {
        _trackedReferencesRepository = trackedReferencesRepository;
        _scopeProvider = scopeProvider;
        _entityService = entityService;
    }

    [Obsolete("Please use ctor that does not take an IEntityService, scheduled for removal in V15")]
    public TrackedReferencesService(
        ITrackedReferencesRepository trackedReferencesRepository,
        ICoreScopeProvider scopeProvider): this(trackedReferencesRepository, scopeProvider, StaticServiceProvider.Instance.GetRequiredService<IEntityService>())
    {

    }

    /// <summary>
    ///     Gets a paged result of items which are in relation with the current item.
    ///     Basically, shows the items which depend on the current item.
    /// </summary>
    [Obsolete("Use overload that takes key instead of id. This will be removed in Umbraco 15.")]
    public PagedResult<RelationItem> GetPagedRelationsForItem(int id, long pageIndex, int pageSize, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItem> items = _trackedReferencesRepository.GetPagedRelationsForItem(id, pageIndex, pageSize, filterMustBeIsDependency, out var totalItems);

        return new PagedResult<RelationItem>(totalItems, pageIndex + 1, pageSize) { Items = items };
    }

    /// <summary>
    ///     Gets a paged result of items used in any kind of relation from selected integer ids.
    /// </summary>
    [Obsolete("Use overload that takes key instead of id. This will be removed in Umbraco 15.")]
    public PagedResult<RelationItem> GetPagedItemsWithRelations(int[] ids, long pageIndex, int pageSize, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItem> items = _trackedReferencesRepository.GetPagedItemsWithRelations(ids, pageIndex, pageSize, filterMustBeIsDependency, out var totalItems);

        return new PagedResult<RelationItem>(totalItems, pageIndex + 1, pageSize) { Items = items };
    }

    /// <summary>
    ///     Gets a paged result of the descending items that have any references, given a parent id.
    /// </summary>
    [Obsolete("Use overload that takes key instead of id. This will be removed in Umbraco 15.")]
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

    [Obsolete("Use overload that takes key instead of id. This will be removed in Umbraco 15.")]
    public PagedModel<RelationItemModel> GetPagedRelationsForItem(int id, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedRelationsForItem(id, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return pagedModel;
    }

    public async Task<PagedModel<RelationItemModel>> GetPagedRelationsForItemAsync(Guid key, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedRelationsForItem(key, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return await Task.FromResult(pagedModel);
    }

    public async Task<PagedModel<RelationItemModel>> GetPagedRelationsForRecycleBinAsync(UmbracoObjectTypes objectType, long skip, long take, bool filterMustBeIsDependency)
    {
        Guid objectTypeKey = objectType switch
        {
            UmbracoObjectTypes.Document => Constants.ObjectTypes.Document,
            UmbracoObjectTypes.Media => Constants.ObjectTypes.Media,
            _ => throw new ArgumentOutOfRangeException(nameof(objectType), "Only documents and media have recycle bin support."),
        };

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedRelationsForRecycleBin(objectTypeKey, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);
        return await Task.FromResult(pagedModel);
    }

    [Obsolete("Use overload that takes key instead of id. This will be removed in Umbraco 15.")]
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

    public async Task<PagedModel<RelationItemModel>> GetPagedDescendantsInReferencesAsync(Guid parentKey, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedDescendantsInReferences(
            parentKey,
            skip,
            take,
            filterMustBeIsDependency,
            out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return await Task.FromResult(pagedModel);
    }

    [Obsolete("Use overload that takes key instead of id. This will be removed in Umbraco 15.")]
    public PagedModel<RelationItemModel> GetPagedItemsWithRelations(int[] ids, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedItemsWithRelations(ids, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return pagedModel;
    }

    public async Task<PagedModel<RelationItemModel>> GetPagedItemsWithRelationsAsync(ISet<Guid> keys, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedItemsWithRelations(keys, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return await Task.FromResult(pagedModel);
    }

    public async Task<PagedModel<Guid>> GetPagedKeysWithDependentReferencesAsync(ISet<Guid> keys, Guid objectTypeId, long skip, long take)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return await _trackedReferencesRepository.GetPagedNodeKeysWithDependantReferencesAsync(keys, objectTypeId, skip, take);
    }
}
