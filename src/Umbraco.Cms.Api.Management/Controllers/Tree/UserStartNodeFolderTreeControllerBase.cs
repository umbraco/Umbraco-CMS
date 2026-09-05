using Umbraco.Cms.Api.Management.Models.Entities;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

/// <summary>
/// Base class for folder tree controllers that support user start node filtering.
/// </summary>
/// <typeparam name="TItem">The type of tree item response model.</typeparam>
/// <remarks>
/// This base class combines folder tree support from <see cref="FolderTreeControllerBase{TItem}"/>
/// with user start node filtering, similar to <see cref="UserStartNodeTreeControllerBase{TItem}"/>.
/// Users without root access will only see items within their configured start nodes,
/// with ancestor items marked as "no access" for navigation purposes.
/// </remarks>
public abstract class UserStartNodeFolderTreeControllerBase<TItem> : FolderTreeControllerBase<TItem>
    where TItem : FolderTreeItemResponseModel, new()
{
    private readonly IUserStartNodeTreeFilterService _treeFilterService;

    private Dictionary<Guid, bool> _accessMap = new();
    private Guid? _dataTypeKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStartNodeFolderTreeControllerBase{TItem}"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="flagProviders">The flag provider collection.</param>
    /// <param name="treeFilterService">The user start node tree filter service.</param>
    protected UserStartNodeFolderTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeTreeFilterService treeFilterService)
        : base(entityService, flagProviders)
    {
        _treeFilterService = treeFilterService;
    }

    /// <summary>
    /// Configures the controller to ignore user start nodes for a specific data type.
    /// </summary>
    /// <param name="dataTypeKey">The data type key, or <c>null</c> to disable.</param>
    protected void IgnoreUserStartNodesForDataType(Guid? dataTypeKey) => _dataTypeKey = dataTypeKey;

    /// <inheritdoc />
    protected override IEntitySlim[] GetPagedRootEntities(int skip, int take, out long totalItems)
        => ShouldBypassStartNodeFiltering()
            ? base.GetPagedRootEntities(skip, take, out totalItems)
            : MapAccessEntities(_treeFilterService.GetFilteredRootEntities(out totalItems));

    /// <inheritdoc />
    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, int skip, int take, out long totalItems)
        => ShouldBypassStartNodeFiltering()
            ? base.GetPagedChildEntities(parentKey, skip, take, out totalItems)
            : MapAccessEntities(_treeFilterService.GetFilteredChildEntities(parentKey, skip, take, ItemOrdering, out totalItems));

    /// <inheritdoc />
    protected override IEntitySlim[] GetSiblingEntities(Guid target, int before, int after, out long totalBefore, out long totalAfter)
        => ShouldBypassStartNodeFiltering()
            ? base.GetSiblingEntities(target, before, after, out totalBefore, out totalAfter)
            : MapAccessEntities(_treeFilterService.GetFilteredSiblingEntities(target, before, after, ItemOrdering, out totalBefore, out totalAfter));

    /// <inheritdoc />
    protected override async Task<TItem[]> MapTreeItemViewModelsAsync(Guid? parentKey, IEntitySlim[] entities)
    {
        if (ShouldBypassStartNodeFiltering())
        {
            return await base.MapTreeItemViewModelsAsync(parentKey, entities);
        }

        // for users with no root access, only add items for the entities contained within the calculated access map.
        // the access map may contain entities that the user does not have direct access to, but need still to see,
        // because it has descendants that the user *does* have access to. these entities are added as "no access" items.
        IEnumerable<Task<TItem?>> tasks = entities.Select(async entity =>
        {
            if (_accessMap.TryGetValue(entity.Key, out var hasAccess) is false)
            {
                // entity is not a part of the calculated access map
                return null;
            }

            // direct access => return a regular item
            // no direct access => return a "no access" item
            return hasAccess
                ? await MapTreeItemViewModelAsync(parentKey, entity)
                : await MapTreeItemViewModelAsNoAccessAsync(parentKey, entity);
        });

        TItem?[] mapped = await Task.WhenAll(tasks);
        return mapped.WhereNotNull().ToArray();
    }

    /// <summary>
    /// Maps an entity to a tree item view model marked as "no access".
    /// </summary>
    /// <param name="parentKey">The parent key.</param>
    /// <param name="entity">The entity to map.</param>
    /// <returns>The mapped tree item view model.</returns>
    /// <remarks>
    /// Subclasses should override this to set the appropriate "no access" flag on the view model.
    /// </remarks>
    protected virtual Task<TItem> MapTreeItemViewModelAsNoAccessAsync(Guid? parentKey, IEntitySlim entity)
        => MapTreeItemViewModelAsync(parentKey, entity);

    private IEntitySlim[] MapAccessEntities(UserAccessEntity[] userAccessEntities)
    {
        _accessMap = userAccessEntities.ToDictionary(uae => uae.Entity.Key, uae => uae.HasAccess);
        return userAccessEntities.Select(uae => uae.Entity).ToArray();
    }

    private bool ShouldBypassStartNodeFiltering()
        => _treeFilterService.ShouldBypassStartNodeFiltering(_dataTypeKey);
}
