using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Services.Paging;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.RecycleBin;

namespace Umbraco.Cms.ManagementApi.Controllers.RecycleBin;

public abstract class RecycleBinControllerBase<TItem> : Controller
    where TItem : RecycleBinItemViewModel, new()
{
    private readonly IEntityService _entityService;
    private readonly string _itemUdiType;

    protected RecycleBinControllerBase(IEntityService entityService)
    {
        _entityService = entityService;
        // ReSharper disable once VirtualMemberCallInConstructor
        _itemUdiType = ItemObjectType.GetUdiType();
    }

    protected abstract UmbracoObjectTypes ItemObjectType { get; }

    protected abstract int RecycleBinRootId { get; }

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetRoot(int skip, int take)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        IEntitySlim[] rootEntities = GetPagedRootEntities(pageNumber, pageSize, out var totalItems);

        TItem[] treeItemViewModels = MapRecycleBinViewModels(null, rootEntities);

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

        TItem[] treeItemViewModels = MapRecycleBinViewModels(parentKey, children);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);

        return await Task.FromResult(Ok(result));
    }

    protected virtual TItem MapRecycleBinViewModel(Guid? parentKey, IEntitySlim entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

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

    private IEntitySlim[] GetPagedRootEntities(long pageNumber, int pageSize, out long totalItems)
    {
        IEntitySlim[] rootEntities = _entityService
            .GetPagedTrashedChildren(RecycleBinRootId, ItemObjectType, pageNumber, pageSize, out totalItems)
            .ToArray();

        return rootEntities;
    }

    private IEntitySlim[] GetPagedChildEntities(Guid parentKey, long pageNumber, int pageSize, out long totalItems)
    {
        IEntitySlim? parent = _entityService.Get(parentKey, ItemObjectType);
        if (parent == null || parent.Trashed == false)
        {
            // not much else we can do here but return nothing
            totalItems = 0;
            return Array.Empty<IEntitySlim>();
        }

        IEntitySlim[] children = _entityService
            .GetPagedTrashedChildren(parent.Id, ItemObjectType, pageNumber, pageSize, out totalItems)
            .ToArray();

        return children;
    }

    private TItem[] MapRecycleBinViewModels(Guid? parentKey, IEntitySlim[] entities)
        => entities.Select(entity => MapRecycleBinViewModel(parentKey, entity)).ToArray();

    private PagedViewModel<TItem> PagedViewModel(IEnumerable<TItem> treeItemViewModels, long totalItems)
        => new() { Total = totalItems, Items = treeItemViewModels };
}
