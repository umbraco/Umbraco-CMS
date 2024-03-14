﻿using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class FolderTreeControllerBase<TItem> : NamedEntityTreeControllerBase<TItem>
    where TItem : FolderTreeItemResponseModel, new()
{
    private readonly Guid _folderObjectTypeId;
    private bool _foldersOnly;

    protected FolderTreeControllerBase(IEntityService entityService)
        : base(entityService) =>
        // ReSharper disable once VirtualMemberCallInConstructor
        _folderObjectTypeId = FolderObjectType.GetGuid();

    protected abstract UmbracoObjectTypes FolderObjectType { get; }

    protected void RenderFoldersOnly(bool foldersOnly) => _foldersOnly = foldersOnly;

    protected override IEntitySlim[] GetPagedRootEntities(int skip, int take, out long totalItems)
        => GetEntities(
            Constants.System.RootKey,
            skip,
            take,
            out totalItems);

    protected override IEntitySlim[] GetPagedChildEntities(Guid parentKey, int skip, int take, out long totalItems) =>
        GetEntities(
            parentKey,
            skip,
            take,
            out totalItems);

    protected override TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        TItem viewModel = base.MapTreeItemViewModel(parentKey, entity);

        if (entity.NodeObjectType == _folderObjectTypeId)
        {
            viewModel.IsFolder = true;
        }

        return viewModel;
    }

    private IEntitySlim[] GetEntities(Guid? parentKey, int skip, int take, out long totalItems)
    {
        totalItems = 0;

        if (take == 0)
        {
            totalItems = _foldersOnly
                ? EntityService.CountChildren(parentKey, FolderObjectType)
                : EntityService.CountChildren(parentKey, FolderObjectType)
                  + EntityService.CountChildren(parentKey, ItemObjectType);
            return Array.Empty<IEntitySlim>();
        }

        // EntityService is not able to paginate children of multiple item types, so we will only paginate the
        // item type entities and always return all folders as part of the the first result "page" i.e. when skip is 0
        IEntitySlim[] folderEntities = skip == 0
            ? EntityService.GetChildren(parentKey, FolderObjectType).OrderBy(c => c.Name).ToArray()
            : Array.Empty<IEntitySlim>();
        IEntitySlim[] itemEntities = _foldersOnly
            ? Array.Empty<IEntitySlim>()
            : EntityService.GetPagedChildren(
                    parentKey,
                    ItemObjectType,
                    skip,
                    take,
                    out totalItems,
                    ordering: ItemOrdering)
                .ToArray();

        // the GetChildren for folders does not return an amount and does not get executed when beyond the first page
        // but the items still count towards the total, so add these to either 0 when only folders, or the out param from paged
        totalItems += skip == 0
            ? folderEntities.Length
            : EntityService.CountChildren(parentKey, FolderObjectType);

        return folderEntities.Union(itemEntities).ToArray();
    }
}
