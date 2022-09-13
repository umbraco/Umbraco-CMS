using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Models.Entities;
using Umbraco.Cms.ManagementApi.Services.Entities;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Tree;

public abstract class UserStartNodeTreeControllerBase<TItem> : EntityTreeControllerBase<TItem>
    where TItem : ContentTreeItemViewModel, new()
{
    private readonly IUserStartNodeEntitiesService _userStartNodeEntitiesService;
    private readonly IDataTypeService _dataTypeService;

    private int[]? _userStartNodeIds;
    private string[]? _userStartNodePaths;
    private Dictionary<Guid, bool> _accessMap = new();
    private Guid? _dataTypeKey;

    protected UserStartNodeTreeControllerBase(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService)
        : base(entityService)
    {
        _userStartNodeEntitiesService = userStartNodeEntitiesService;
        _dataTypeService = dataTypeService;
    }

    protected abstract int[] GetUserStartNodeIds();

    protected abstract string[] GetUserStartNodePaths();

    protected void IgnoreUserStartNodesForDataType(Guid? dataTypeKey) => _dataTypeKey = dataTypeKey;

    protected override IEntitySlim[] GetPagedRootEntities(long pageNumber, int pageSize, out long totalItems)
        => UserHasRootAccess() || IgnoreUserStartNodes()
            ? base.GetPagedRootEntities(pageNumber, pageSize, out totalItems)
            : CalculateAccessMap(() => _userStartNodeEntitiesService.RootUserAccessEntities(ItemObjectType, UserStartNodeIds), out totalItems);

    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, long pageNumber, int pageSize, out long totalItems)
    {
        IEntitySlim[] children = base.GetPagedChildEntities(parentKey, pageNumber, pageSize, out totalItems);
        return UserHasRootAccess() || IgnoreUserStartNodes()
            ? children
            : CalculateAccessMap(() => _userStartNodeEntitiesService.ChildUserAccessEntities(children, UserStartNodePaths), out totalItems);
    }

    protected override IEntitySlim[] GetEntities(Guid[] keys)
    {
        IEntitySlim[] entities = base.GetEntities(keys);
        return UserHasRootAccess() || IgnoreUserStartNodes()
            ? entities
            : CalculateAccessMap(() => _userStartNodeEntitiesService.UserAccessEntities(entities, UserStartNodePaths), out _);
    }

    protected override TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        if (UserHasRootAccess() || IgnoreUserStartNodes())
        {
            return base.MapTreeItemViewModels(parentKey, entities);
        }

        // for users with no root access, only add items for the entities contained within the calculated access map.
        // the access map may contain entities that the user does not have direct access to, but need still to see,
        // because it has descendants that the user *does* have access to. these entities are added as "no access" items.
        TItem[] contentTreeItemViewModels = entities.Select(entity =>
            {
                if (_accessMap.TryGetValue(entity.Key, out var hasAccess) == false)
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

        return contentTreeItemViewModels;
    }

    private int[] UserStartNodeIds => _userStartNodeIds ??= GetUserStartNodeIds();

    private string[] UserStartNodePaths => _userStartNodePaths ??= GetUserStartNodePaths();

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

    private TItem MapTreeItemViewModelAsNoAccess(Guid? parentKey, IEntitySlim entity)
    {
        TItem viewModel = MapTreeItemViewModel(parentKey, entity);
        viewModel.NoAccess = true;
        return viewModel;
    }
}
