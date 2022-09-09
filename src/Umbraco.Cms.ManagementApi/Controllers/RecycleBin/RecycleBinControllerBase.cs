using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
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

    protected async Task<ActionResult<PagedResult<TItem>>> GetRoot(long pageNumber, int pageSize)
    {
        IEntitySlim[] rootEntities = GetPagedRootEntities(pageNumber, pageSize, out var totalItems);

        TItem[] treeItemViewModels = MapRecycleBinViewModels(null, rootEntities);

        PagedResult<TItem> result = PagedResult(treeItemViewModels, pageNumber, pageSize, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<PagedResult<TItem>>> GetChildren(Guid parentKey, long pageNumber, int pageSize)
    {
        IEntitySlim[] children = GetPagedChildEntities(parentKey, pageNumber, pageSize, out var totalItems);

        TItem[] treeItemViewModels = MapRecycleBinViewModels(parentKey, children);

        PagedResult<TItem> result = PagedResult(treeItemViewModels, pageNumber, pageSize, totalItems);

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
        // TODO: EntityService needs to be expanded to get root identifier (key?) by UmbracoObjectType - for now this is a workaround
        var rootId = ItemObjectType switch
        {
            UmbracoObjectTypes.Document => Constants.System.RecycleBinContent,
            UmbracoObjectTypes.Media => Constants.System.RecycleBinMedia,
            _ => throw new ArgumentOutOfRangeException(nameof(ItemObjectType), "Only Document and Media are supported")
        };

        // TODO: EntityService needs to be able to return paged trashed children - for now we'll get all children
        // IEntitySlim[] rootEntities = EntityService
        //     .GetPagedChildren(rootId, ItemObjectType, pageNumber, pageSize, out totalItems)
        //     .ToArray();
        IEntitySlim[] rootEntities = _entityService
            .GetChildren(rootId, ItemObjectType)
            .ToArray();
        totalItems = rootEntities.Length;

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

        // TODO: EntityService needs to be able to return paged trashed children - for now we'll get all children
        // IEntitySlim[] children = EntityService
        //     .GetPagedChildren(parent.Id, ItemObjectType, pageNumber, pageSize, out totalItems)
        //     .ToArray();
        IEntitySlim[] children = _entityService
            .GetChildren(parent.Id, ItemObjectType)
            .ToArray();
        totalItems = children.Length;

        return children;
    }

    private TItem[] MapRecycleBinViewModels(Guid? parentKey, IEntitySlim[] entities)
        => entities.Select(entity => MapRecycleBinViewModel(parentKey, entity)).ToArray();

    private PagedResult<TItem> PagedResult(IEnumerable<TItem> treeItemViewModels, long pageNumber, int pageSize, long totalItems)
        => new(totalItems, pageNumber, pageSize) { Items = treeItemViewModels };
}
