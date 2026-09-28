using Umbraco.Cms.Api.Management.Models.Entities;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

/// <summary>
/// Provides a base controller for managing tree structures representing user start nodes in the Umbraco management API.
/// </summary>
/// <typeparam name="TItem">The type of the tree item managed by the controller.</typeparam>
public abstract class UserStartNodeTreeControllerBase<TItem> : EntityTreeControllerBase<TItem>
    where TItem : ContentTreeItemResponseModel, new()
{
    private readonly IUserStartNodeTreeFilterService _treeFilterService;

    private Dictionary<Guid, bool> _accessMap = new();
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
        : base(entityService, flagProviders) =>
        _treeFilterService = treeFilterService;

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

        IEnumerable<Task<TItem?>> tasks = entities.Select(async entity =>
        {
            if (_accessMap.TryGetValue(entity.Key, out var hasAccess) is false)
            {
                return null;
            }

            return hasAccess
                ? await MapTreeItemViewModelAsync(parentKey, entity)
                : await MapTreeItemViewModelAsNoAccessAsync(parentKey, entity);
        });

        TItem?[] mapped = await Task.WhenAll(tasks);
        return mapped.WhereNotNull().ToArray();
    }

    private IEntitySlim[] MapAccessEntities(UserAccessEntity[] userAccessEntities)
    {
        _accessMap = userAccessEntities.ToDictionary(uae => uae.Entity.Key, uae => uae.HasAccess);
        return userAccessEntities.Select(uae => uae.Entity).ToArray();
    }

    private async Task<TItem> MapTreeItemViewModelAsNoAccessAsync(Guid? parentKey, IEntitySlim entity)
    {
        TItem viewModel = await MapTreeItemViewModelAsync(parentKey, entity);
        viewModel.NoAccess = true;
        return viewModel;
    }

    private bool ShouldBypassStartNodeFiltering()
        => _treeFilterService.ShouldBypassStartNodeFiltering(_dataTypeKey);

}
