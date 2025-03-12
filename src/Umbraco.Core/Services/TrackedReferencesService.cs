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

    public Task<PagedModel<RelationItemModel>> GetPagedRelationsForItemAsync(Guid key, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedRelationsForItem(key, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return Task.FromResult(pagedModel);
    }

    public Task<PagedModel<RelationItemModel>> GetPagedDescendantsInReferencesAsync(Guid parentKey, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedDescendantsInReferences(
            parentKey,
            skip,
            take,
            filterMustBeIsDependency,
            out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return Task.FromResult(pagedModel);
    }

    public Task<PagedModel<RelationItemModel>> GetPagedItemsWithRelationsAsync(ISet<Guid> keys, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedItemsWithRelations(keys, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return Task.FromResult(pagedModel);
    }

    public async Task<PagedModel<Guid>> GetPagedKeysWithDependentReferencesAsync(ISet<Guid> keys, Guid objectTypeId, long skip, long take)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return await _trackedReferencesRepository.GetPagedNodeKeysWithDependantReferencesAsync(keys, objectTypeId, skip, take);
    }
}
