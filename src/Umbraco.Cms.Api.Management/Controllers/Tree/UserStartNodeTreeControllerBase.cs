using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

/// <summary>
/// Base class for tree controllers that support user start node filtering.
/// </summary>
/// <typeparam name="TItem">The type of tree item response model.</typeparam>
/// <remarks>
/// Users without root access will only see items within their configured start nodes,
/// with ancestor items marked as "no access" for navigation purposes.
/// </remarks>
public abstract class UserStartNodeTreeControllerBase<TItem> : EntityTreeControllerBase<TItem>
    where TItem : ContentTreeItemResponseModel, new()
{
    private readonly IUserStartNodeTreeFilterService _treeFilterService;

    private Guid? _dataTypeKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStartNodeTreeControllerBase{TItem}"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="flagProviders">The flag provider collection.</param>
    /// <param name="treeFilterService">The user start node tree filter service.</param>
    protected UserStartNodeTreeControllerBase(
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

    private TItem MapTreeItemViewModelAsNoAccess(Guid? parentKey, IEntitySlim entity)
    {
        TItem viewModel = MapTreeItemViewModel(parentKey, entity);
        viewModel.NoAccess = true;
        return viewModel;
    }

    private bool ShouldBypassStartNodeFiltering()
        => _treeFilterService.ShouldBypassStartNodeFiltering(_dataTypeKey);
}
