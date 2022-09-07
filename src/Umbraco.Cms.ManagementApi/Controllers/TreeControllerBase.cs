using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers;

public abstract class TreeControllerBase<TItem> : Controller
    where TItem : TreeItemViewModel, new()
{
    private readonly string _itemUdiType;

    protected TreeControllerBase(IEntityService entityService)
    {
        EntityService = entityService;

        // ReSharper disable once VirtualMemberCallInConstructor
        _itemUdiType = ItemObjectType.GetUdiType();
    }

    protected IEntityService EntityService { get; }

    protected abstract UmbracoObjectTypes ItemObjectType { get; }

    protected async Task<ActionResult> GetRoot(long pageNumber, int pageSize)
    {
        IEntitySlim[] rootEntities = GetPagedRootEntities(pageNumber, pageSize, out var totalItems);

        TItem[] treeItemViewModels = MapRootTreeItemViewModels(rootEntities);

        PagedResult<TItem> result = PagedTreeItemViewModels(treeItemViewModels, pageNumber, pageSize, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult> GetChildren(Guid parentKey, long pageNumber, int pageSize)
    {
        IEntitySlim[] children = GetPagedChildEntities(parentKey, pageNumber, pageSize, out var totalItems);

        TItem[] treeItemViewModels = MapChildTreeItemViewModels(parentKey, children);

        // NOTE: totalItems won't be accurate here if some where filtered out .. but we can't get any closer at the moment
        PagedResult<TItem> result = PagedTreeItemViewModels(treeItemViewModels, pageNumber, pageSize, totalItems);

        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult> GetItems(Guid[] keys)
    {
        if (keys.IsCollectionEmpty())
        {
            return await Task.FromResult(Ok(PagedTreeItemViewModels(Array.Empty<TItem>(), 0, 0, 0)));
        }

        IEntitySlim[] itemEntities = EntityService.GetAll(ItemObjectType, keys).ToArray();

        TItem[] treeItemViewModels = MapTreeItemViewModels(null, itemEntities);

        var totalItems = itemEntities.Count();

        PagedResult<TItem> result = PagedTreeItemViewModels(treeItemViewModels, 0, totalItems, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected virtual IEntitySlim[] GetPagedRootEntities(long pageNumber, int pageSize, out long totalItems)
    {
        // for now we'll just get all root entities; perhaps later on it will make sense to use EntityService.GetPagedChildren
        IEntitySlim[] rootEntities = EntityService
            .GetRootEntities(ItemObjectType)
            .ToArray();

        totalItems = rootEntities.Length;
        return rootEntities;
    }

    protected virtual IEntitySlim[] GetPagedChildEntities(Guid parentKey, long pageNumber, int pageSize, out long totalItems)
    {
        // TODO: make EntityService able to get paged children by parent key - this is a workaround for now
        IEntitySlim? parent = EntityService.Get(parentKey, ItemObjectType);
        if (parent == null)
        {
            // not much else we can do here but return nothing
            totalItems = 0;
            return Array.Empty<IEntitySlim>();
        }

        IEntitySlim[] children = EntityService.GetPagedChildren(
                parent.Id,
                ItemObjectType,
                pageNumber,
                pageSize,
                out totalItems)
            .ToArray();
        return children;
    }

    protected PagedResult<TItem> PagedTreeItemViewModels(IEnumerable<TItem> treeItemViewModels, long pageNumber, int pageSize, long totalItems)
        => new(totalItems, pageNumber, pageSize) { Items = treeItemViewModels };

    protected virtual TItem[] MapRootTreeItemViewModels(IEntitySlim[] entities)
        => MapTreeItemViewModels(null, entities);

    protected virtual TItem[] MapChildTreeItemViewModels(Guid parentKey, IEntitySlim[] entities)
        => MapTreeItemViewModels(parentKey, entities);

    protected virtual TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        TItem[] treeItemViewModels = entities.Select(entity => MapTreeItemViewModel(parentKey, entity)).ToArray();
        return treeItemViewModels;
    }

    protected virtual TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        var treeItemViewModel = new TItem
        {
            Icon = _itemUdiType,
            Name = entity.Name!,
            Key = entity.Key,
            Type = _itemUdiType,
            HasChildren = entity.HasChildren,
            IsTrashed = entity.Trashed,
            IsContainer = entity.IsContainer,
            ParentKey = parentKey
        };

        return treeItemViewModel;
    }
}
