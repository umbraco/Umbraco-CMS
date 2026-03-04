using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

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
            : _treeFilterService.GetFilteredRootEntities(out totalItems);

    /// <inheritdoc />
    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, int skip, int take, out long totalItems)
        => ShouldBypassStartNodeFiltering()
            ? base.GetPagedChildEntities(parentKey, skip, take, out totalItems)
            : _treeFilterService.GetFilteredChildEntities(parentKey, skip, take, ItemOrdering, out totalItems);

    /// <inheritdoc />
    protected override IEntitySlim[] GetSiblingEntities(Guid target, int before, int after, out long totalBefore, out long totalAfter)
        => ShouldBypassStartNodeFiltering()
            ? base.GetSiblingEntities(target, before, after, out totalBefore, out totalAfter)
            : _treeFilterService.GetFilteredSiblingEntities(target, before, after, ItemOrdering, out totalBefore, out totalAfter);

    /// <inheritdoc />
    protected override TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
        => ShouldBypassStartNodeFiltering()
            ? base.MapTreeItemViewModels(parentKey, entities)
            : _treeFilterService.MapWithAccessFiltering(
                entities,
                entity => MapTreeItemViewModel(parentKey, entity),
                entity => MapTreeItemViewModelAsNoAccess(parentKey, entity));

    /// <summary>
    /// Maps an entity to a tree item view model marked as "no access".
    /// </summary>
    /// <param name="parentKey">The parent key.</param>
    /// <param name="entity">The entity to map.</param>
    /// <returns>The mapped tree item view model.</returns>
    /// <remarks>
    /// Subclasses should override this to set the appropriate "no access" flag on the view model.
    /// </remarks>
    protected virtual TItem MapTreeItemViewModelAsNoAccess(Guid? parentKey, IEntitySlim entity)
        => MapTreeItemViewModel(parentKey, entity);

    private bool ShouldBypassStartNodeFiltering()
        => _treeFilterService.ShouldBypassStartNodeFiltering(_dataTypeKey);
}
