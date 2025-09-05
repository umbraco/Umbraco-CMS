using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class EntityTreeControllerBase<TItem> : ManagementApiControllerBase
    where TItem : EntityTreeItemResponseModel, new()
{
    private readonly SignProviderCollection _signProviders;

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    protected EntityTreeControllerBase(IEntityService entityService)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<SignProviderCollection>())
    {
    }

    protected EntityTreeControllerBase(IEntityService entityService, SignProviderCollection signProviders)
    {
        EntityService = entityService;
        _signProviders = signProviders;
    }

    protected IEntityService EntityService { get; }

    protected abstract UmbracoObjectTypes ItemObjectType { get; }

    protected virtual Ordering ItemOrdering => Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.Text));

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetRoot(int skip, int take)
    {
        IEntitySlim[] rootEntities = GetPagedRootEntities(skip, take, out var totalItems);

        TItem[] treeItemViewModels = MapTreeItemViewModels(null, rootEntities);

        await PopulateSigns(treeItemViewModels);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);

        return Ok(result);
    }

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetChildren(Guid parentId, int skip, int take)
    {
        IEntitySlim[] children = GetPagedChildEntities(parentId, skip, take, out var totalItems);

        TItem[] treeItemViewModels = MapTreeItemViewModels(parentId, children);

        await PopulateSigns(treeItemViewModels);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);

        return Ok(result);
    }

    protected async Task<ActionResult<SubsetViewModel<TItem>>> GetSiblings(Guid target, int before, int after)
    {
        IEntitySlim[] siblings = GetSiblingEntities(target, before, after, out var totalBefore, out var totalAfter);
        if (siblings.Length == 0)
        {
            return NotFound();
        }

        IEntitySlim? entity = siblings.FirstOrDefault();
        Guid? parentKey = GetParentKey(entity);

        TItem[] treeItemViewModels = MapTreeItemViewModels(parentKey, siblings);

        await PopulateSigns(treeItemViewModels);

        SubsetViewModel<TItem> result = SubsetViewModel(treeItemViewModels, totalBefore, totalAfter);

        return Ok(result);
    }

    /// <summary>
    /// Gets the parent key for an entity, or root if null or no parent.
    /// </summary>
    protected virtual Guid? GetParentKey(IEntitySlim? entity) =>
        entity?.ParentId > 0
            ? EntityService.GetKey(entity.ParentId, ItemObjectType).Result
            : Constants.System.RootKey;

    protected virtual async Task<ActionResult<IEnumerable<TItem>>> GetAncestors(Guid descendantKey, bool includeSelf = true)
    {
        IEntitySlim[] ancestorEntities = await GetAncestorEntitiesAsync(descendantKey, includeSelf);

        TItem[] treeItemViewModels = ancestorEntities
            .Select(ancestor =>
            {
                IEntitySlim? parent = ancestor.ParentId > 0
                    ? ancestorEntities.Single(a => a.Id == ancestor.ParentId)
                    : null;

                return MapTreeItemViewModel(parent?.Key, ancestor);
            })
            .ToArray();

        await PopulateSigns(treeItemViewModels);

        return Ok(treeItemViewModels);
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
        EntityService
            .GetPagedChildren(
                parentKey,
                ItemObjectType,
                skip,
                take,
                out totalItems,
                ordering: ItemOrdering)
            .ToArray();

    protected virtual IEntitySlim[] GetSiblingEntities(Guid target, int before, int after, out long totalBefore, out long totalAfter) =>
        EntityService
            .GetSiblings(
                target,
                [ItemObjectType],
                before,
                after,
                out totalBefore,
                out totalAfter,
                ordering: ItemOrdering)
        .ToArray();

    protected virtual TItem[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
        => entities.Select(entity => MapTreeItemViewModel(parentKey, entity)).ToArray();

    protected virtual async Task PopulateSigns(TItem[] treeItemViewModels)
    {
        foreach (ISignProvider signProvider in _signProviders.Where(x => x.CanProvideSigns<TItem>()))
        {
            await signProvider.PopulateSignsAsync(treeItemViewModels);
        }
    }

    protected virtual TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        var viewModel = new TItem
        {
            Id = entity.Key,
            HasChildren = entity.HasChildren,
            Parent = parentKey.HasValue
                ? new ReferenceByIdModel
                {
                    Id = parentKey.Value,
                }
                : null,
        };

        return viewModel;
    }

    protected PagedViewModel<TItem> PagedViewModel(IEnumerable<TItem> treeItemViewModels, long totalItems)
        => new() { Total = totalItems, Items = treeItemViewModels };

    protected SubsetViewModel<TItem> SubsetViewModel(IEnumerable<TItem> treeItemViewModels, long totalBefore, long totalAfter)
        => new() { TotalBefore = totalBefore, TotalAfter = totalAfter, Items = treeItemViewModels };
}
