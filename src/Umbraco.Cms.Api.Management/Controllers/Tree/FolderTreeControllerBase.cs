using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Tree;

public abstract class FolderTreeControllerBase<TItem> : NamedEntityTreeControllerBase<TItem>
    where TItem : FolderTreeItemResponseModel, new()
{
    private readonly Guid _folderObjectTypeId;
    private bool _foldersOnly;

    protected abstract UmbracoObjectTypes FolderObjectType { get; }

    protected IEntitySearchService EntitySearchService { get; }

    protected IIdKeyMap IdKeyMap { get; }

    protected override Ordering ItemOrdering
    {
        get
        {
            // Override to order by type (folder vs item) before the text
            var ordering = Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.NodeObjectTypeColumnName);
            ordering.Next = Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.TextColumnName);

            return ordering;
        }
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    protected FolderTreeControllerBase(IEntityService entityService)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>())
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    protected FolderTreeControllerBase(IEntityService entityService, FlagProviderCollection flagProviders)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IEntitySearchService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>())
    {
    }

    protected FolderTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap)
        : base(entityService, flagProviders)
    {
        EntitySearchService = entitySearchService;
        IdKeyMap = idKeyMap;
        _folderObjectTypeId = FolderObjectType.GetGuid();
    }


    /// <inheritdoc/>
    protected override Guid? GetParentKey(IEntitySlim? entity)
    {
        if (entity is null || entity.ParentId <= 0)
        {
            return Constants.System.RootKey;
        }

        Attempt<Guid> getKeyAttempt = EntityService.GetKey(entity.ParentId, ItemObjectType);
        if (getKeyAttempt.Success)
        {
            return getKeyAttempt.Result;
        }

        // Parent could be a folder, so try that too.
        getKeyAttempt = EntityService.GetKey(entity.ParentId, FolderObjectType);
        if (getKeyAttempt.Success)
        {
            return getKeyAttempt.Result;
        }

        return Constants.System.RootKey;
    }

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

    protected override IEntitySlim[] GetSiblingEntities(Guid target, int before, int after, out long totalBefore, out long totalAfter)
    {
        totalBefore = 0;
        totalAfter = 0;

        UmbracoObjectTypes[] siblingObjectTypes = GetObjectTypes();

        return EntityService.GetSiblings(
                target,
                siblingObjectTypes,
                before,
                after,
                out totalBefore,
                out totalAfter,
                ordering: ItemOrdering)
            .ToArray();
    }

    protected override TItem MapTreeItemViewModel(Guid? parentKey, IEntitySlim entity)
    {
        TItem viewModel = base.MapTreeItemViewModel(parentKey, entity);

        if (entity.NodeObjectType == _folderObjectTypeId)
        {
            viewModel.IsFolder = true;
        }

        return viewModel;
    }

    protected override Task<IEntitySlim[]> GetAncestorEntitiesAsync(Guid descendantKey, bool includeSelf = true)
    {
        IEntitySlim? entity = EntityService.Get(descendantKey, ItemObjectType)
                              ?? EntityService.Get(descendantKey, FolderObjectType);
        if (entity is null)
        {
            // not much else we can do here but return nothing
            return Task.FromResult(Array.Empty<IEntitySlim>());
        }

        var ancestorIds = entity.AncestorIds();
        // annoyingly we can't use EntityService.GetAll() with container object types, so we have to get them one by one
        IEntitySlim[] containers = ancestorIds.Select(id => EntityService.Get(id, FolderObjectType)).WhereNotNull().ToArray();
        IEnumerable<IEntitySlim> ancestors = ancestorIds.Any()
            ? EntityService
                .GetAll(ItemObjectType, ancestorIds)
                .Union(containers)
            : Array.Empty<IEntitySlim>();
        if (includeSelf)
        {
            ancestors = ancestors.Append(entity);
        }

        return Task.FromResult(ancestors.OrderBy(item => item.Level).ToArray());
    }

    private IEntitySlim[] GetEntities(Guid? parentKey, int skip, int take, out long totalItems)
    {
        totalItems = 0;

        UmbracoObjectTypes[] childObjectTypes = GetObjectTypes();

        return EntityService.GetPagedChildren(
                parentKey,
                [FolderObjectType, ItemObjectType],
                childObjectTypes,
                skip,
                take,
                false,
                out totalItems,
                ordering: ItemOrdering)
            .ToArray();
    }

    private UmbracoObjectTypes[] GetObjectTypes() => _foldersOnly ? [FolderObjectType] : [FolderObjectType, ItemObjectType];

    protected async Task<ActionResult<PagedViewModel<TItem>>> SearchTreeEntities(
        string? query,
        int skip,
        int take,
        TreeItemKind itemKind)
    {
        PagedModel<IEntitySlim> itemSearchResult =
            query.IsNullOrWhiteSpace()
                ? EntitySearchService.Search(GetItemObjectTypes(itemKind), skip, take)
                : EntitySearchService.Search(GetItemObjectTypes(itemKind), query, skip, take);

        (IEntitySlim[] entities, long totalItems) =
            await FilterTreeEntities(itemSearchResult.Items.ToArray(), itemSearchResult.Total);

        TItem[] treeItemViewModels = MapSearchTreeItemViewModels(entities);

        await PopulateFlags(treeItemViewModels);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);

        return Ok(result);
    }

    protected virtual TItem[] MapSearchTreeItemViewModels(IEntitySlim[] entities)
        => entities.Select(entity => MapTreeItemViewModel(GetSearchResultParentKey(entity), entity)).ToArray();

    private Guid? GetSearchResultParentKey(IEntitySlim entity)
    {
        if (entity.ParentId == Constants.System.Root)
        {
            return null;
        }

        if (FolderObjectType != UmbracoObjectTypes.Unknown)
        {
            Attempt<Guid> getKeyAttempt = IdKeyMap.GetKeyForId(entity.ParentId, FolderObjectType);
            if (getKeyAttempt.Success)
            {
                return getKeyAttempt.Result;
            }
        }

        Attempt<Guid> itemKeyAttempt = IdKeyMap.GetKeyForId(entity.ParentId, ItemObjectType);
        if (itemKeyAttempt.Success)
        {
            return itemKeyAttempt.Result;
        }

        return null;
    }

    private IEnumerable<UmbracoObjectTypes> GetItemObjectTypes(TreeItemKind itemKind)
    {
        var types = new List<UmbracoObjectTypes>();

        if (itemKind.HasFlag(TreeItemKind.Item))
        {
            types.Add(ItemObjectType);
        }

        if (itemKind.HasFlag(TreeItemKind.Folder) && FolderObjectType != UmbracoObjectTypes.Unknown)
        {
            types.Add(FolderObjectType);
        }

        return types;
    }
}
