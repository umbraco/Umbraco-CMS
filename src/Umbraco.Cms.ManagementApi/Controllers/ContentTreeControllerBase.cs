using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Models.Entities;
using Umbraco.Cms.ManagementApi.Services.Entities;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers;

public abstract class ContentTreeControllerBase<TItem> : TreeControllerBase<TItem>
    where TItem : ContentTreeItemViewModel, new()
{
    private readonly IUserAccessEntitiesService _userAccessEntitiesService;

    private int[]? _userStartNodeIds;
    private string[]? _userStartNodePaths;
    private Dictionary<Guid, bool> _accessMap = new();

    protected ContentTreeControllerBase(
        IEntityService entityService,
        IUserAccessEntitiesService userAccessEntitiesService)
        : base(entityService) =>
        _userAccessEntitiesService = userAccessEntitiesService;

    protected abstract int[] GetUserStartNodeIds();

    protected abstract string[] GetUserStartNodePaths();

    protected override IEntitySlim[] GetPagedRootEntities(long pageNumber, int pageSize, out long totalItems)
        => UserHasRootAccess()
            ? base.GetPagedRootEntities(pageNumber, pageSize, out totalItems)
            : CalculateAccessMap(() => _userAccessEntitiesService.RootUserAccessEntities(ItemObjectType, UserStartNodeIds), out totalItems);

    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, long pageNumber, int pageSize, out long totalItems)
    {
        IEntitySlim[] children = base.GetPagedChildEntities(parentKey, pageNumber, pageSize, out totalItems);
        return UserHasRootAccess()
            ? children
            : CalculateAccessMap(() => _userAccessEntitiesService.ChildUserAccessEntities(children, UserStartNodePaths), out totalItems);
    }

    protected override IEntitySlim[] GetEntities(Guid[] keys)
    {
        IEntitySlim[] entities = base.GetEntities(keys);
        return UserHasRootAccess()
            ? entities
            : CalculateAccessMap(() => _userAccessEntitiesService.UserAccessEntities(entities, UserStartNodePaths), out _);
    }

    protected override TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        if (UserHasRootAccess())
        {
            return base.MapTreeItemViewModels(parentKey, entities);
        }

        // tree items for users without root access - pseudo code:
        // - if the entity is in the access map as accessible, add a regular item
        // - else if the entity is in the access map as not accessible, add a "no access" item
        // - else remove the item
        TItem[] contentTreeItemViewModels = entities.Select(entity
                => _accessMap.TryGetValue(entity.Key, out var hasAccess)
                    ? hasAccess
                        ? MapTreeItemViewModel(parentKey, entity)
                        : MapTreeItemViewModelAsNoAccess(parentKey, entity)
                    : null)
            .WhereNotNull()
            .ToArray();

        return contentTreeItemViewModels;
    }

    private int[] UserStartNodeIds => _userStartNodeIds ??= GetUserStartNodeIds();

    private string[] UserStartNodePaths => _userStartNodePaths ??= GetUserStartNodePaths();

    private bool UserHasRootAccess() => UserStartNodeIds.Contains(Constants.System.Root);

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
