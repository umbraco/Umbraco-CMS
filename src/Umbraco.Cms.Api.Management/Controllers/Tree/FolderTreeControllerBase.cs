﻿using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class FolderTreeControllerBase<TItem> : NamedEntityTreeControllerBase<TItem>
    where TItem : FolderTreeItemResponseModel, new()
{
    private readonly Guid _folderObjectTypeId;
    private bool _foldersOnly;



    protected override Ordering ItemOrdering
    {
        get
        {
            // Override to order by type (folder vs item) before the text
            var ordering = Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.NodeObjectType));
            ordering.Next = Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.Text));

            return ordering;
        }
    }

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

    protected override async Task<IEntitySlim[]> GetAncestorEntitiesAsync(Guid descendantKey, bool includeSelf = true)
    {
        IEntitySlim? entity = EntityService.Get(descendantKey, ItemObjectType)
                              ?? EntityService.Get(descendantKey, FolderObjectType);
        if (entity is null)
        {
            // not much else we can do here but return nothing
            return await Task.FromResult(Array.Empty<IEntitySlim>());
        }

        var ancestorIds = entity.AncestorIds();
        // annoyingly we can't use EntityService.GetAll() with container object types, so we have to get them one by one
        IEntitySlim[] containers = ancestorIds.Select(id => EntityService.Get(id, FolderObjectType)).WhereNotNull().ToArray();
        IEnumerable<IEntitySlim> ancestors = ancestorIds.Any()
            ? EntityService
                .GetAll(ItemObjectType, ancestorIds)
                .Union(containers)
            : Array.Empty<IEntitySlim>();
        ancestors = ancestors.Union(includeSelf ? new[] { entity } : Array.Empty<IEntitySlim>());

        return ancestors.OrderBy(item => item.Level).ToArray();
    }

    private IEntitySlim[] GetEntities(Guid? parentKey, int skip, int take, out long totalItems)
    {
        totalItems = 0;

        UmbracoObjectTypes[] childObjectTypes = _foldersOnly ? [FolderObjectType] : [FolderObjectType, ItemObjectType];

        IEntitySlim[] itemEntities = EntityService.GetPagedChildren(
                    parentKey,
                    new [] { FolderObjectType, ItemObjectType },
                    childObjectTypes,
                    skip,
                    take,
                    false,
                    out totalItems,
                    ordering: ItemOrdering)
                .ToArray();

        return itemEntities;
    }
}
