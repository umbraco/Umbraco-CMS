using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

/// <inheritdoc />
public class ElementRecycleBinQueryService : IElementRecycleBinQueryService
{
    private readonly IEntityService _entityService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IRelationService _relationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementRecycleBinQueryService"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="relationService">The relation service.</param>
    public ElementRecycleBinQueryService(
        IEntityService entityService,
        ICoreScopeProvider scopeProvider,
        IRelationService relationService)
    {
        _entityService = entityService;
        _scopeProvider = scopeProvider;
        _relationService = relationService;
    }

    /// <inheritdoc />
    public Task<Attempt<IEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentAsync(Guid trashedElementId)
        => GetOriginalParentCoreAsync(
            trashedElementId,
            UmbracoObjectTypes.Element,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnElementDeleteAlias);

    /// <inheritdoc />
    public Task<Attempt<IEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentForContainerAsync(
        Guid trashedElementContainerId)
        => GetOriginalParentCoreAsync(
            trashedElementContainerId,
            UmbracoObjectTypes.ElementContainer,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnContainerDeleteAlias);

    private Task<Attempt<IEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentCoreAsync(
        Guid trashedEntityId,
        UmbracoObjectTypes entityObjectType,
        string parentRelationTypeAlias)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        IEntitySlim? entity = _entityService.Get(trashedEntityId, entityObjectType);
        if (entity is null)
        {
            return Task.FromResult(Attempt<IEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.NotFound));
        }

        if (entity.Trashed is false)
        {
            return Task.FromResult(Attempt<IEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.NotTrashed));
        }

        IEnumerable<IRelation> relationsByChild = _relationService.GetByChildId(entity.Id);
        IRelation? parentRecycleRelation = relationsByChild
            .FirstOrDefault(r => r.RelationType.Alias == parentRelationTypeAlias);

        if (parentRecycleRelation is null)
        {
            return Task.FromResult(Attempt<IEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.NoParentRecycleRelation));
        }

        if (parentRecycleRelation.ParentId == Constants.System.Root)
        {
            return Task.FromResult(Attempt<IEntitySlim?, RecycleBinQueryResultType>.Succeed(RecycleBinQueryResultType.ParentIsRoot, null));
        }

        IEntitySlim? parent = _entityService.Get(parentRecycleRelation.ParentId, UmbracoObjectTypes.ElementContainer);
        if (parent is null)
        {
            return Task.FromResult(Attempt<IEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.ParentNotFound));
        }

        if (parent.Trashed)
        {
            return Task.FromResult(Attempt<IEntitySlim?, RecycleBinQueryResultType>.Fail(RecycleBinQueryResultType.ParentIsTrashed, parent));
        }

        return Task.FromResult(Attempt<IEntitySlim?, RecycleBinQueryResultType>.Succeed(RecycleBinQueryResultType.Success, parent));
    }
}
