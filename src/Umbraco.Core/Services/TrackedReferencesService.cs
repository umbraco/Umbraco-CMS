using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for tracking and querying references between entities in Umbraco.
/// </summary>
/// <remarks>
///     This service is used to find relationships and dependencies between content, media, and other entities.
///     It helps determine which items reference other items, which is useful for operations like deletion checks.
/// </remarks>
public class TrackedReferencesService : ITrackedReferencesService
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IEntityService _entityService;
    private readonly ITrackedReferencesRepository _trackedReferencesRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TrackedReferencesService"/> class.
    /// </summary>
    /// <param name="trackedReferencesRepository">The repository for accessing tracked reference data.</param>
    /// <param name="scopeProvider">The scope provider for database operations.</param>
    /// <param name="entityService">The entity service for retrieving entity information.</param>
    public TrackedReferencesService(
        ITrackedReferencesRepository trackedReferencesRepository,
        ICoreScopeProvider scopeProvider,
        IEntityService entityService)
    {
        _trackedReferencesRepository = trackedReferencesRepository;
        _scopeProvider = scopeProvider;
        _entityService = entityService;
    }

    /// <inheritdoc />
    [Obsolete("Use the GetPagedRelationsForItemAsync overload which returns an Attempt with operation status. Scheduled for removal in Umbraco 19.")]
    public Task<PagedModel<RelationItemModel>> GetPagedRelationsForItemAsync(Guid key, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedRelationsForItem(key, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return Task.FromResult(pagedModel);
    }

    /// <inheritdoc />
    public async Task<Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus>> GetPagedRelationsForItemAsync(Guid key, UmbracoObjectTypes objectType, long skip, long take, bool filterMustBeIsDependency)
    {
        IEntitySlim? entity = _entityService.Get(key, objectType);
        if (entity is null)
        {
            return Attempt.FailWithStatus(GetReferencesOperationStatus.ContentNotFound, new PagedModel<RelationItemModel>());
        }

#pragma warning disable CS0618 // Type or member is obsolete (but using whilst it exists to avoid code repetition)
        PagedModel<RelationItemModel> pagedModel = await GetPagedRelationsForItemAsync(key, skip, take, filterMustBeIsDependency);
#pragma warning restore CS0618 // Type or member is obsolete

        return Attempt.SucceedWithStatus(GetReferencesOperationStatus.Success, pagedModel);
    }

    /// <inheritdoc />
    public Task<PagedModel<RelationItemModel>> GetPagedRelationsForRecycleBinAsync(UmbracoObjectTypes objectType, long skip, long take, bool filterMustBeIsDependency)
    {
        Guid objectTypeKey = objectType switch
        {
            UmbracoObjectTypes.Document => Constants.ObjectTypes.Document,
            UmbracoObjectTypes.Media => Constants.ObjectTypes.Media,
            UmbracoObjectTypes.Element => Constants.ObjectTypes.Element,
            _ => throw new ArgumentOutOfRangeException(nameof(objectType), "Only documents, media and elements have recycle bin support."),
        };

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedRelationsForRecycleBin(
            objectTypeKey,
            skip,
            take,
            filterMustBeIsDependency,
            out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);
        return Task.FromResult(pagedModel);
    }

    /// <inheritdoc />
    [Obsolete("Use GetPagedDescendantsInReferencesAsync which returns an Attempt with operation status. Scheduled for removal in Umbraco 19.")]
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

    /// <inheritdoc />
    public async Task<Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus>> GetPagedDescendantsInReferencesAsync(Guid parentKey, UmbracoObjectTypes objectType, long skip, long take, bool filterMustBeIsDependency)
    {
        IEntitySlim? entity = _entityService.Get(parentKey, objectType);
        if (entity is null)
        {
            return Attempt.FailWithStatus(GetReferencesOperationStatus.ContentNotFound, new PagedModel<RelationItemModel>());
        }

#pragma warning disable CS0618 // Type or member is obsolete (but using whilst it exists to avoid code repetition)
        PagedModel<RelationItemModel> pagedModel = await GetPagedDescendantsInReferencesAsync(parentKey, skip, take, filterMustBeIsDependency);
#pragma warning restore CS0618 // Type or member is obsolete

        return Attempt.SucceedWithStatus(GetReferencesOperationStatus.Success, pagedModel);
    }

    /// <inheritdoc />
    public Task<PagedModel<RelationItemModel>> GetPagedItemsWithRelationsAsync(ISet<Guid> keys, long skip, long take, bool filterMustBeIsDependency)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        IEnumerable<RelationItemModel> items = _trackedReferencesRepository.GetPagedItemsWithRelations(keys, skip, take, filterMustBeIsDependency, out var totalItems);
        var pagedModel = new PagedModel<RelationItemModel>(totalItems, items);

        return Task.FromResult(pagedModel);
    }

    /// <inheritdoc />
    public async Task<PagedModel<Guid>> GetPagedKeysWithDependentReferencesAsync(ISet<Guid> keys, Guid objectTypeId, long skip, long take)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return await _trackedReferencesRepository.GetPagedNodeKeysWithDependantReferencesAsync(keys, objectTypeId, skip, take);
    }
}
