using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Models.Entities;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

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

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    protected UserStartNodeTreeControllerBase(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>(),
              userStartNodeEntitiesService,
              dataTypeService)
    {
    }

#pragma warning disable CS0618 // Type or member is obsolete
    [Obsolete("Please use the constructor accepting IUserStartNodeTreeFilterService. Scheduled for removal in Umbraco 19.")]
    protected UserStartNodeTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService)
        : base(entityService, flagProviders)
            => _treeFilterService = new CallbackStartNodeTreeFilterService(
                userStartNodeEntitiesService,
                dataTypeService,
                GetUserStartNodeIds,
                GetUserStartNodePaths,
                () => ItemObjectType);
#pragma warning restore CS0618 // Type or member is obsolete

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
    /// Gets the calculated start node IDs for the current user.
    /// </summary>
    /// <returns>An array of start node IDs.</returns>
    [Obsolete("No longer used. Register a custom IUserStartNodeTreeFilterService instead. Scheduled for removal in Umbraco 19.")]
    protected virtual int[] GetUserStartNodeIds() => [];

    /// <summary>
    /// Gets the calculated start node paths for the current user.
    /// </summary>
    /// <returns>An array of start node paths.</returns>
    [Obsolete("No longer used. Register a custom IUserStartNodeTreeFilterService instead. Scheduled for removal in Umbraco 19.")]
    protected virtual string[] GetUserStartNodePaths() => [];

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
    protected override TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
        => ShouldBypassStartNodeFiltering()
            ? base.MapTreeItemViewModels(parentKey, entities)
            : _treeFilterService.MapWithAccessFiltering(
                entities,
                _accessMap,
                entity => MapTreeItemViewModel(parentKey, entity),
                entity => MapTreeItemViewModelAsNoAccess(parentKey, entity));

    private IEntitySlim[] MapAccessEntities(UserAccessEntity[] userAccessEntities)
    {
        _accessMap = userAccessEntities.ToDictionary(uae => uae.Entity.Key, uae => uae.HasAccess);
        return userAccessEntities.Select(uae => uae.Entity).ToArray();
    }

    private TItem MapTreeItemViewModelAsNoAccess(Guid? parentKey, IEntitySlim entity)
    {
        TItem viewModel = MapTreeItemViewModel(parentKey, entity);
        viewModel.NoAccess = true;
        return viewModel;
    }

    private bool ShouldBypassStartNodeFiltering()
        => _treeFilterService.ShouldBypassStartNodeFiltering(_dataTypeKey);

    /// <summary>
    /// A backward-compatible adapter that implements <see cref="UserStartNodeTreeFilterService"/>
    /// by delegating start node resolution to callback functions.
    /// </summary>
    /// <remarks>
    /// Used by the obsolete constructor to bridge the old abstract-method-based
    /// start node resolution to the new service-based approach.
    /// </remarks>
    [Obsolete("Only used by the obsolete constructor. Scheduled for removal in Umbraco 19.")]
    private sealed class CallbackStartNodeTreeFilterService : UserStartNodeTreeFilterService
    {
        private readonly Func<int[]> _getStartNodeIds;
        private readonly Func<string[]> _getStartNodePaths;
        private readonly Func<UmbracoObjectTypes> _getTreeObjectType;

        public CallbackStartNodeTreeFilterService(
            IUserStartNodeEntitiesService userStartNodeEntitiesService,
            IDataTypeService dataTypeService,
            Func<int[]> getStartNodeIds,
            Func<string[]> getStartNodePaths,
            Func<UmbracoObjectTypes> getTreeObjectType)
            : base(userStartNodeEntitiesService, dataTypeService)
        {
            _getStartNodeIds = getStartNodeIds;
            _getStartNodePaths = getStartNodePaths;
            _getTreeObjectType = getTreeObjectType;
        }

        /// <inheritdoc />
        protected override UmbracoObjectTypes TreeObjectType => _getTreeObjectType();

        /// <inheritdoc />
        protected override int[] CalculateUserStartNodeIds() => _getStartNodeIds();

        /// <inheritdoc />
        protected override string[] CalculateUserStartNodePaths() => _getStartNodePaths();
    }
}
