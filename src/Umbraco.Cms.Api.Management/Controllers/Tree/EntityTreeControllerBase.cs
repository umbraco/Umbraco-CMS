using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class EntityTreeControllerBase<TItem> : ManagementApiControllerBase
    where TItem : EntityTreeItemResponseModel, new()
{
    protected EntityTreeControllerBase(IEntityService entityService)
        => EntityService = entityService;

    protected IEntityService EntityService { get; }

    protected abstract UmbracoObjectTypes ItemObjectType { get; }

    protected virtual Ordering ItemOrdering => Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.Text));

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetRoot(int skip, int take)
    {
        IEntitySlim[] rootEntities = GetPagedRootEntities(skip, take, out var totalItems);

        TItem[] treeItemViewModels = MapTreeItemViewModels(null, rootEntities);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetChildren(Guid parentId, int skip, int take)
    {
        IEntitySlim[] children = GetPagedChildEntities(parentId, skip, take, out var totalItems);

        TItem[] treeItemViewModels = MapTreeItemViewModels(parentId, children);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<IEnumerable<TItem>>> GetItems(Guid[] ids)
    {
        if (ids.IsCollectionEmpty())
        {
            return await Task.FromResult(Ok(PagedViewModel(Array.Empty<TItem>(), 0)));
        }

        IEntitySlim[] itemEntities = GetEntities(ids);

        TItem[] treeItemViewModels = MapTreeItemViewModels(null, itemEntities);

        return await Task.FromResult(Ok(treeItemViewModels));
    }

    protected virtual IEntitySlim[] GetPagedRootEntities(int skip, int take, out long totalItems)
        => EntityService
            .GetPagedChildren(
                Constants.System.RootKey,
                ItemObjectType,
                skip,
                take,
                out totalItems,
                ordering: ItemOrdering)
            .ToArray();

    protected virtual IEntitySlim[] GetPagedChildEntities(Guid parentKey, int skip, int take, out long totalItems) =>
        EntityService.GetPagedChildren(
                parentKey,
                ItemObjectType,
                skip,
                take,
                out totalItems,
                ordering: ItemOrdering)
            .ToArray();

    protected virtual IEntitySlim[] GetEntities(Guid[] ids) => EntityService.GetAll(ItemObjectType, ids).ToArray();

    protected virtual TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
        => entities.Select(entity => MapTreeItemViewModel(parentKey, entity)).ToArray();

    protected virtual TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        var viewModel = new TItem
        {
            Id = entity.Key,
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
