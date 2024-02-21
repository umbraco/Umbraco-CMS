using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Paging;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class EntityTreeControllerBase<TItem> : ManagementApiControllerBase
    where TItem : EntityTreeItemResponseModel, new()
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

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetChildren(Guid parentId, int skip, int take)
    {
        if (PaginationService.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize, out ProblemDetails? error) == false)
        {
            return BadRequest(error);
        }

        IEntitySlim[] children = GetPagedChildEntities(parentId, pageNumber, pageSize, out var totalItems);

        TItem[] treeItemViewModels = MapTreeItemViewModels(parentId, children);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected virtual async Task<ActionResult<IEnumerable<TItem>>> GetAncestors(Guid descendantKey)
    {
        IEntitySlim[] ancestorEntities = await GetAncestorEntitiesAsync(descendantKey);

        TItem[] result = ancestorEntities
            .Select(ancestor =>
            {
                IEntitySlim? parent = ancestor.ParentId > 0
                    ? ancestorEntities.Single(a => a.Id == ancestor.ParentId)
                    : null;

                return MapTreeItemViewModel(parent?.Key, ancestor);
            })
            .ToArray();

        return Ok(result);
    }

    protected virtual async Task<IEntitySlim[]> GetAncestorEntitiesAsync(Guid descendantKey)
    {
        IEntitySlim? entity = EntityService.Get(descendantKey, ItemObjectType);
        if (entity is null)
        {
            // not much else we can do here but return nothing
            return await Task.FromResult(Array.Empty<IEntitySlim>());
        }

        var ancestorIds = entity.AncestorIds();
        IEntitySlim[] ancestors = ancestorIds.Any()
            ? EntityService
                .GetAll(ItemObjectType, ancestorIds)
                .OrderBy(item => item.Level)
                .ToArray()
            : Array.Empty<IEntitySlim>();

        return ancestors;
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
        // EntityService is only able to get paged children by parent ID, so we must first map parent id to parent ID
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

    protected virtual TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
        => entities.Select(entity => MapTreeItemViewModel(parentKey, entity)).ToArray();

    protected virtual TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        var viewModel = new TItem
        {
            Id = entity.Key,
            Type = _itemUdiType,
            HasChildren = entity.HasChildren,
            Parent = parentKey.HasValue
                ? new ReferenceByIdModel
                {
                    Id = parentKey.Value
                }
                : null
        };

        return viewModel;
    }

    protected PagedViewModel<TItem> PagedViewModel(IEnumerable<TItem> treeItemViewModels, long totalItems)
        => new() { Total = totalItems, Items = treeItemViewModels };
}
