using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Services.Paging;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Tree;

public abstract class EntityTreeControllerBase<TItem> : ManagementApiControllerBase
    where TItem : EntityTreeItemViewModel, new()
{
    private readonly string _itemUdiType;

    protected EntityTreeControllerBase(IEntityService entityService)
    {
        EntityService = entityService;

        // ReSharper disable once VirtualMemberCallInConstructor
        _itemUdiType = ItemObjectType.GetUdiType();
    }

    protected IEntityService EntityService { get; }

    protected abstract UmbracoObjectTypes ItemObjectType { get; }

    protected virtual Ordering ItemOrdering => Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.Text));

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetRoot(int skip, int take)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        IEntitySlim[] rootEntities = GetPagedRootEntities(pageNumber, pageSize, out var totalItems);

        TItem[] treeItemViewModels = MapTreeItemViewModels(null, rootEntities);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetChildren(Guid parentKey, int skip, int take)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        IEntitySlim[] children = GetPagedChildEntities(parentKey, pageNumber, pageSize, out var totalItems);

        TItem[] treeItemViewModels = MapTreeItemViewModels(parentKey, children);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<IEnumerable<TItem>>> GetItems(Guid[] keys)
    {
        if (keys.IsCollectionEmpty())
        {
            return await Task.FromResult(Ok(PagedViewModel(Array.Empty<TItem>(), 0)));
        }

        IEntitySlim[] itemEntities = GetEntities(keys);

        TItem[] treeItemViewModels = MapTreeItemViewModels(null, itemEntities);

        return await Task.FromResult(Ok(treeItemViewModels));
    }

    protected virtual IEntitySlim[] GetPagedRootEntities(long pageNumber, int pageSize, out long totalItems)
        => EntityService
            .GetPagedChildren(
                Constants.System.Root,
                ItemObjectType,
                pageNumber,
                pageSize,
                out totalItems,
                ordering: ItemOrdering)
            .ToArray();

    protected virtual IEntitySlim[] GetPagedChildEntities(Guid parentKey, long pageNumber, int pageSize, out long totalItems)
    {
        // EntityService is only able to get paged children by parent ID, so we must first map parent key to parent ID
        Attempt<int> parentId = EntityService.GetId(parentKey, ItemObjectType);
        if (parentId.Success == false)
        {
            // not much else we can do here but return nothing
            totalItems = 0;
            return Array.Empty<IEntitySlim>();
        }

        IEntitySlim[] children = EntityService.GetPagedChildren(
                parentId.Result,
                ItemObjectType,
                pageNumber,
                pageSize,
                out totalItems,
                ordering: ItemOrdering)
            .ToArray();
        return children;
    }

    protected virtual IEntitySlim[] GetEntities(Guid[] keys) => EntityService.GetAll(ItemObjectType, keys).ToArray();

    protected virtual TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
        => entities.Select(entity => MapTreeItemViewModel(parentKey, entity)).ToArray();

    protected virtual TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        var viewModel = new TItem
        {
            Icon = _itemUdiType,
            Name = entity.Name!,
            Key = entity.Key,
            Type = _itemUdiType,
            HasChildren = entity.HasChildren,
            IsContainer = entity.IsContainer,
            ParentKey = parentKey
        };

        return viewModel;
    }

    protected PagedViewModel<TItem> PagedViewModel(IEnumerable<TItem> treeItemViewModels, long totalItems)
        => new() { Total = totalItems, Items = treeItemViewModels };
}
