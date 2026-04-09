using Umbraco.Cms.Api.Management.Models.Entities;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
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
    private readonly IUserStartNodeEntitiesService _userStartNodeEntitiesService;
    private readonly IDataTypeService _dataTypeService;

    private Dictionary<Guid, bool> _accessMap = new();
    private Guid? _dataTypeKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStartNodeFolderTreeControllerBase{TItem}"/> class.
    /// </summary>
    /// <param name="entityService">The entity service.</param>
    /// <param name="flagProviders">The flag provider collection.</param>
    /// <param name="userStartNodeEntitiesService">The user start node entities service.</param>
    /// <param name="dataTypeService">The data type service.</param>
    protected UserStartNodeFolderTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService)
        : base(entityService, flagProviders)
    {
        _userStartNodeEntitiesService = userStartNodeEntitiesService;
        _dataTypeService = dataTypeService;
    }

    private UmbracoObjectTypes[] TreeObjectTypes => [FolderObjectType, ItemObjectType];

    /// <summary>
    /// Gets the calculated start node IDs for the current user.
    /// </summary>
    /// <returns>An array of start node IDs.</returns>
    protected abstract int[] GetUserStartNodeIds();

    /// <summary>
    /// Gets the calculated start node paths for the current user.
    /// </summary>
    /// <returns>An array of start node paths.</returns>
    protected abstract string[] GetUserStartNodePaths();

    /// <summary>
    /// Configures the controller to ignore user start nodes for a specific data type.
    /// </summary>
    /// <param name="dataTypeKey">The data type key, or <c>null</c> to disable.</param>
    protected void IgnoreUserStartNodesForDataType(Guid? dataTypeKey) => _dataTypeKey = dataTypeKey;

    /// <inheritdoc />
    protected override IEntitySlim[] GetPagedRootEntities(int skip, int take, out long totalItems)
        => UserHasRootAccess() || IgnoreUserStartNodes()
            ? base.GetPagedRootEntities(skip, take, out totalItems)
            : CalculateAccessMap(() => _userStartNodeEntitiesService.RootUserAccessEntities(TreeObjectTypes, UserStartNodeIds), out totalItems);

    /// <inheritdoc />
    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, int skip, int take, out long totalItems)
    {
        if (UserHasRootAccess() || IgnoreUserStartNodes())
        {
            return base.GetPagedChildEntities(parentKey, skip, take, out totalItems);
        }

        IEnumerable<UserAccessEntity> userAccessEntities = _userStartNodeEntitiesService.ChildUserAccessEntities(
            TreeObjectTypes,
            UserStartNodePaths,
            parentKey,
            skip,
            take,
            ItemOrdering,
            out totalItems);

        return CalculateAccessMap(() => userAccessEntities, out _);
    }

    /// <inheritdoc />
    protected override IEntitySlim[] GetSiblingEntities(Guid target, int before, int after, out long totalBefore, out long totalAfter)
    {
        if (UserHasRootAccess() || IgnoreUserStartNodes())
        {
            return base.GetSiblingEntities(target, before, after, out totalBefore, out totalAfter);
        }

        IEnumerable<UserAccessEntity> userAccessEntities = _userStartNodeEntitiesService.SiblingUserAccessEntities(
            TreeObjectTypes,
            UserStartNodePaths,
            target,
            before,
            after,
            ItemOrdering,
            out totalBefore,
            out totalAfter);

        return CalculateAccessMap(() => userAccessEntities, out _);
    }

    /// <inheritdoc />
    protected override TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        if (UserHasRootAccess() || IgnoreUserStartNodes())
        {
            return base.MapTreeItemViewModels(parentKey, entities);
        }

        // for users with no root access, only add items for the entities contained within the calculated access map.
        // the access map may contain entities that the user does not have direct access to, but need still to see,
        // because it has descendants that the user *does* have access to. these entities are added as "no access" items.
        TItem[] treeItemViewModels = entities.Select(entity =>
            {
                if (_accessMap.TryGetValue(entity.Key, out var hasAccess) is false)
                {
                    // entity is not a part of the calculated access map
                    return null;
                }

                // direct access => return a regular item
                // no direct access => return a "no access" item
                return hasAccess
                    ? MapTreeItemViewModel(parentKey, entity)
                    : MapTreeItemViewModelAsNoAccess(parentKey, entity);
            })
            .WhereNotNull()
            .ToArray();

        return treeItemViewModels;
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
    protected virtual TItem MapTreeItemViewModelAsNoAccess(Guid? parentKey, IEntitySlim entity)
        => MapTreeItemViewModel(parentKey, entity);

    private int[] UserStartNodeIds => field ??= GetUserStartNodeIds();

    private string[] UserStartNodePaths => field ??= GetUserStartNodePaths();

    private bool UserHasRootAccess() => UserStartNodeIds.Contains(Constants.System.Root);

    private bool IgnoreUserStartNodes()
        => _dataTypeKey.HasValue
           && _dataTypeService.IsDataTypeIgnoringUserStartNodes(_dataTypeKey.Value);

    private IEntitySlim[] CalculateAccessMap(Func<IEnumerable<UserAccessEntity>> getUserAccessEntities, out long totalItems)
    {
        UserAccessEntity[] userAccessEntities = getUserAccessEntities().ToArray();

        _accessMap = userAccessEntities.ToDictionary(uae => uae.Entity.Key, uae => uae.HasAccess);

        IEntitySlim[] entities = userAccessEntities.Select(uae => uae.Entity).ToArray();
        totalItems = entities.Length;

        return entities;
    }
}
