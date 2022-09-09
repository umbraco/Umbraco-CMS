using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.Tree;

public abstract class FolderTreeControllerBase<TItem> : EntityTreeControllerBase<TItem>
    where TItem : FolderTreeItemViewModel, new()
{
    private readonly Guid _folderObjectTypeId;
    private bool _foldersOnly;

    protected FolderTreeControllerBase(IEntityService entityService)
        : base(entityService) =>
        // ReSharper disable once VirtualMemberCallInConstructor
        _folderObjectTypeId = FolderObjectType.GetGuid();

    protected abstract UmbracoObjectTypes FolderObjectType { get; }

    protected async Task<ActionResult<PagedResult<TItem>>> GetRoot(long pageNumber, int pageSize, bool foldersOnly)
    {
        // save "folders only" state for GetPagedRootEntities
        _foldersOnly = foldersOnly;
        return await GetRoot(pageNumber, pageSize);
    }

    protected async Task<ActionResult<PagedResult<TItem>>> GetChildren(Guid parentKey, long pageNumber, int pageSize, bool foldersOnly)
    {
        // save "folders only" state for GetPagedChildEntities
        _foldersOnly = foldersOnly;
        return await GetChildren(parentKey, pageNumber, pageSize);
    }

    protected override IEntitySlim[] GetPagedRootEntities(long pageNumber, int pageSize, out long totalItems)
    {
        // for now we'll just get all root entities; perhaps later on it will make sense to use EntityService.GetPagedChildren
        IEntitySlim[] folderEntities = EntityService.GetRootEntities(FolderObjectType).ToArray();
        IEntitySlim[] itemEntities = _foldersOnly
            ? Array.Empty<IEntitySlim>()
            : EntityService.GetRootEntities(ItemObjectType).ToArray();

        IEntitySlim[] allEntities = folderEntities.Union(itemEntities).ToArray();
        totalItems = allEntities.Length;
        return allEntities;
    }

    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, long pageNumber, int pageSize, out long totalItems)
    {
        totalItems = 0;

        // TODO: make EntityService able to get paged children by parent key - this is a workaround for now
        IEntitySlim? parent = EntityService.Get(parentKey, FolderObjectType) ?? EntityService.Get(parentKey, ItemObjectType);
        if (parent == null)
        {
            // not much else we can do here but return nothing
            return Array.Empty<IEntitySlim>();
        }

        // TODO: expand EntityService.GetPagedChildren to be able to paginate multiple item types - for now we'll only paginate the items
        IEntitySlim[] folderEntities = EntityService.GetChildren(parent.Id, FolderObjectType).ToArray();
        IEntitySlim[] itemEntities = _foldersOnly
            ? Array.Empty<IEntitySlim>()
            : EntityService.GetPagedChildren(parent.Id, ItemObjectType, pageNumber, pageSize, out totalItems).ToArray();

        totalItems += folderEntities.Length;

        return folderEntities.Union(itemEntities).ToArray();
    }

    protected override TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        TItem viewModel = base.MapTreeItemViewModel(parentKey, entity);

        if (entity.NodeObjectType == _folderObjectTypeId)
        {
            viewModel.IsFolder = true;
            viewModel.Icon = Constants.Icons.Folder;
        }

        return viewModel;
    }
}
