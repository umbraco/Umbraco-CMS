using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Signs;
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

    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    protected FolderTreeControllerBase(IEntityService entityService)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<SignProviderCollection>())
    {
    }

    protected FolderTreeControllerBase(IEntityService entityService, SignProviderCollection signProviders)
        : base(entityService, signProviders) =>
        _folderObjectTypeId = FolderObjectType.GetGuid();

    protected abstract UmbracoObjectTypes FolderObjectType { get; }

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
}
