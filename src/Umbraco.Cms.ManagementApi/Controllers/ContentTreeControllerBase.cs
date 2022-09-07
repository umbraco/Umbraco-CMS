using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers;

public abstract class ContentTreeControllerBase<TItem> : TreeControllerBase<TItem>
    where TItem : ContentTreeItemViewModel, new()
{
    private int[]? _userStartNodeIds;
    private string[]? _userStartNodePaths;

    protected AppCaches AppCaches { get; }

    protected IBackOfficeSecurityAccessor BackofficeSecurityAccessor { get; }

    protected ContentTreeControllerBase(
        IEntityService entityService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService)
    {
        AppCaches = appCaches;
        BackofficeSecurityAccessor = backofficeSecurityAccessor;
    }

    protected abstract int[] GetUserStartNodeIds();

    protected abstract string[] GetUserStartNodePaths();

    protected int[] UserStartNodeIds => _userStartNodeIds ??= GetUserStartNodeIds();

    protected string[] UserStartNodePaths => _userStartNodePaths ??= GetUserStartNodePaths();

    protected bool UserHasRootAccess() => UserStartNodeIds.Contains(Constants.System.Root);

    protected override IEntitySlim[] GetPagedRootEntities(long pageNumber, int pageSize, out long totalItems)
    {
        if (UserHasRootAccess())
        {
            return base.GetPagedRootEntities(pageNumber, pageSize, out totalItems);
        }

        // root entities for users without root access include:
        // - the start nodes that are actual root entities (level == 1)
        // - the root level ancestors to the rest of the start nodes (required for browsing to the actual start nodes - will be marked as "no access")
        IEntitySlim[] userStartEntities = EntityService.GetAll(ItemObjectType, UserStartNodeIds).ToArray();

        // find the start nodes that are at root level (level == 1)
        IEntitySlim[] allowedTopmostEntities = userStartEntities.Where(entity => entity.Level == 1).ToArray();

        // find the root level ancestors of the rest of the start nodes, and add those as well
        var nonAllowedTopmostEntityIds = userStartEntities.Except(allowedTopmostEntities)
            .Select(entity => int.TryParse(entity.Path.Split(Constants.CharArrays.Comma).Skip(1).FirstOrDefault(), out var id) ? id : 0)
            .Where(id => id > 0)
            .ToArray();
        IEntitySlim[] nonAllowedTopmostEntities = nonAllowedTopmostEntityIds.Any()
            ? EntityService.GetAll(ItemObjectType, nonAllowedTopmostEntityIds).ToArray()
            : Array.Empty<IEntitySlim>();

        IEntitySlim[] combinedEntities = allowedTopmostEntities.Union(nonAllowedTopmostEntities).ToArray();
        totalItems = combinedEntities.Length;
        return combinedEntities;
    }

    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, long pageNumber, int pageSize, out long totalItems)
    {
        IEntitySlim[] children = base.GetPagedChildEntities(parentKey, pageNumber, pageSize, out totalItems);
        if (UserHasRootAccess())
        {
            return children;
        }

        // child entities for users without root access include:
        // - entities that are descendants-or-self to a start node
        // - entities that are ancestors to a start node (required for browsing to the actual start nodes - will be marked as "no access")
        return children
            .Where(child => IsDescendantOrSelfToUserStartNode(child) || IsAncestorToUserStartNode(child))
            .ToArray();
    }

    protected override TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        if (UserHasRootAccess())
        {
            return base.MapTreeItemViewModels(parentKey, entities);
        }

        // tree items for users without root access - pseudo code:
        // - if the entity is descendant-or-self to a user start node, add a regular item
        // - else if the entity is an ancestor to a user start node, add a "no access" item
        // - else remove the item
        TItem[] contentTreeItemViewModels = entities.Select(entity =>
        {
            if (IsDescendantOrSelfToUserStartNode(entity))
            {
                return MapTreeItemViewModel(parentKey, entity);
            }

            if (IsAncestorToUserStartNode(entity))
            {
                return MapTreeItemViewModelAsNoAccess(parentKey, entity);
            }

            return null;
        }).WhereNotNull().ToArray();

        return contentTreeItemViewModels;
    }

    private bool IsDescendantOrSelfToUserStartNode(IEntitySlim entity)
        => UserStartNodePaths.Any(path => entity.Path.StartsWith(path));

    private bool IsAncestorToUserStartNode(IEntitySlim entity)
        => UserStartNodePaths.Any(path => path.StartsWith(entity.Path));

    // child items for current user pseudo code:
    // - if the child item is descendant-or-self of a user start node, add the child item as a regular item
    // - else if the child item is an ancestor of a user start node, add the child item as a "no access" item
    // - else remove the child item
    protected PagedResult<TItem> GetPagedChildrenViewModelsForCurrentUser(Guid parentKey, long pageNumber, int pageSize)
    {
        IEntitySlim[] children = GetPagedChildEntities(parentKey, pageNumber, pageSize, out var totalItems);
        TItem[] contentTreeItemViewModels = children.Select(child =>
        {
            if (UserStartNodePaths.Any(path => child.Path.StartsWith(path)))
            {
                return MapTreeItemViewModel(parentKey, child);
            }

            if (UserStartNodePaths.Any(path => path.StartsWith(child.Path)))
            {
                return MapTreeItemViewModelAsNoAccess(parentKey, child);
            }

            return null;
        }).WhereNotNull().ToArray();

        // NOTE: totalItems and pageSize won't be accurate here if some children where filtered out .. but we can't get any closer at the moment
        PagedResult<TItem> result = PagedTreeItemViewModels(contentTreeItemViewModels, 0, pageSize, totalItems);
        return result;
    }

    // root items for current user pseudo code:
    // - if the root item is set explicitly as a user start node, add the root item as a regular item
    // - else if the root item is an ancestor of a user start node, add the root item as a "no access" item
    // - else remove the root item
    protected PagedResult<TItem> GetUserRootViewModelsForCurrentUser()
    {
        IEntitySlim[] userStartEntities = EntityService.GetAll(ItemObjectType, UserStartNodeIds).ToArray();

        IEntitySlim[] allowedTopmostEntities = userStartEntities.Where(entity => entity.Level == 1).ToArray();
        var nonAllowedTopmostEntityIds = userStartEntities.Except(allowedTopmostEntities)
            .Select(entity => int.TryParse(entity.Path.Split(Constants.CharArrays.Comma).Skip(1).FirstOrDefault(), out var id) ? id : 0)
            .Where(id => id > 0)
            .ToArray();
        IEntitySlim[] nonAllowedTopmostEntities = nonAllowedTopmostEntityIds.Any()
            ? EntityService.GetAll(ItemObjectType, nonAllowedTopmostEntityIds).ToArray()
            : Array.Empty<IEntitySlim>();

        TItem[] contentTreeItemViewModels = MapTreeItemViewModels(null, allowedTopmostEntities)
            .Union(MapTreeItemViewModelsAsNoAccess(null, nonAllowedTopmostEntities))
            .ToArray();

        var totalItems = contentTreeItemViewModels.Count();
        PagedResult<TItem> result = PagedTreeItemViewModels(contentTreeItemViewModels, 0, totalItems, totalItems);
        return result;
    }

    protected TItem[] MapTreeItemViewModelsAsNoAccess(Guid? parentKey, IEnumerable<IEntitySlim> entities)
        => entities.Select(entity =>
        {
            TItem viewModel = MapTreeItemViewModel(parentKey, entity);
            viewModel.NoAccess = true;
            return viewModel;
        }).ToArray();

    protected TItem MapTreeItemViewModelAsNoAccess(Guid? parentKey, IEntitySlim entity)
    {
        TItem viewModel = MapTreeItemViewModel(parentKey, entity);
        viewModel.NoAccess = true;
        return viewModel;
    }
}
