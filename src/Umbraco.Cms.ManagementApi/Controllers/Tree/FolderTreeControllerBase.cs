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

    protected void RenderFoldersOnly(bool foldersOnly) => _foldersOnly = foldersOnly;

    protected override IEntitySlim[] GetPagedRootEntities(long pageNumber, int pageSize, out long totalItems)
        => GetEntities(
            Constants.System.Root,
            pageNumber,
            pageSize,
            out totalItems);

    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, long pageNumber, int pageSize, out long totalItems)
    {
        // EntityService is only able to get paged children by parent ID, so we must first map parent key to parent ID
        Attempt<int> parentId = EntityService.GetId(parentKey, FolderObjectType);
        if (parentId.Success == false)
        {
            parentId = EntityService.GetId(parentKey, ItemObjectType);
            if (parentId.Success == false)
            {
                // not much else we can do here but return nothing
                totalItems = 0;
                return Array.Empty<IEntitySlim>();
            }
        }

        return GetEntities(
            parentId.Result,
            pageNumber,
            pageSize,
            out totalItems);
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

    private IEntitySlim[] GetEntities(int parentId, long pageNumber, int pageSize, out long totalItems)
    {
        totalItems = 0;

        // EntityService is not able to paginate children of multiple item types, so we will only paginate the
        // item type entities and always return all folders as part of the the first result page
        IEntitySlim[] folderEntities = pageNumber == 0
            ? EntityService.GetChildren(parentId, FolderObjectType).OrderBy(c => c.Name).ToArray()
            : Array.Empty<IEntitySlim>();
        IEntitySlim[] itemEntities = _foldersOnly
            ? Array.Empty<IEntitySlim>()
            : EntityService.GetPagedChildren(
                    parentId,
                    ItemObjectType,
                    pageNumber,
                    pageSize,
                    out totalItems,
                    ordering: ItemOrdering)
                .ToArray();

        return folderEntities.Union(itemEntities).ToArray();
    }
}
