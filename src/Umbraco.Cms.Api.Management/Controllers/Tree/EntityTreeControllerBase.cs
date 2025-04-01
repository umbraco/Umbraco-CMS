using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
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
    protected EntityTreeControllerBase(IEntityService entityService)
        => EntityService = entityService;

    protected IEntityService EntityService { get; }

    protected abstract UmbracoObjectTypes ItemObjectType { get; }

    protected virtual Ordering ItemOrdering => Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.Text));

    protected Task<ActionResult<PagedViewModel<TItem>>> GetRoot(int skip, int take)
    {
        IEntitySlim[] rootEntities = GetPagedRootEntities(skip, take, out var totalItems);

        TItem[] treeItemViewModels = MapTreeItemViewModels(null, rootEntities);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);
        return Task.FromResult<ActionResult<PagedViewModel<TItem>>>(Ok(result));
    }

    protected Task<ActionResult<PagedViewModel<TItem>>> GetChildren(Guid parentId, int skip, int take)
    {
        IEntitySlim[] children = GetPagedChildEntities(parentId, skip, take, out var totalItems);

        TItem[] treeItemViewModels = MapTreeItemViewModels(parentId, children);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);
        return Task.FromResult<ActionResult<PagedViewModel<TItem>>>(Ok(result));
    }

    protected virtual async Task<ActionResult<IEnumerable<TItem>>> GetAncestors(Guid descendantKey, bool includeSelf = true)
    {
        IEntitySlim[] ancestorEntities = await GetAncestorEntitiesAsync(descendantKey, includeSelf);

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

    protected virtual Task<IEntitySlim[]> GetAncestorEntitiesAsync(Guid descendantKey, bool includeSelf)
    {
        IEntitySlim? entity = EntityService.Get(descendantKey, ItemObjectType);
        if (entity is null)
        {
            // not much else we can do here but return nothing
            return Task.FromResult(Array.Empty<IEntitySlim>());
        }

        var ancestorIds = entity.AncestorIds();

        IEnumerable<IEntitySlim> ancestors = ancestorIds.Any()
            ? EntityService.GetAll(ItemObjectType, ancestorIds)
            : Array.Empty<IEntitySlim>();
        ancestors = ancestors.Union(includeSelf ? new[] { entity } : Array.Empty<IEntitySlim>());

        return Task.FromResult(ancestors.OrderBy(item => item.Level).ToArray());
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
