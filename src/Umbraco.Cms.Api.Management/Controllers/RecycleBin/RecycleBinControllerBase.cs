using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Querying.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.RecycleBin;

public abstract class RecycleBinControllerBase<TItem> : ContentControllerBase
    where TItem : RecycleBinItemResponseModelBase, new()
{
    private readonly IEntityService _entityService;

    protected RecycleBinControllerBase(IEntityService entityService)
        => _entityService = entityService;

    protected abstract UmbracoObjectTypes ItemObjectType { get; }

    protected abstract Guid RecycleBinRootKey { get; }

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetRoot(int skip, int take)
    {
        IEntitySlim[] rootEntities = GetPagedRootEntities(skip, take, out var totalItems);

        TItem[] treeItemViewModels = MapRecycleBinViewModels(null, rootEntities);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);
        return await Task.FromResult(Ok(result));
    }

    protected async Task<ActionResult<PagedViewModel<TItem>>> GetChildren(Guid parentKey, int skip, int take)
    {
        IEntitySlim[] children = GetPagedChildEntities(parentKey, skip, take, out var totalItems);

        TItem[] treeItemViewModels = MapRecycleBinViewModels(parentKey, children);

        PagedViewModel<TItem> result = PagedViewModel(treeItemViewModels, totalItems);

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
            Id = entity.Key,
            CreateDate = entity.CreateDate,
            HasChildren = entity.HasChildren,
            Parent = parentKey.HasValue
                ? new ItemReferenceByIdResponseModel
                {
                    Id = parentKey.Value
                }
                : null
        };

        return viewModel;
    }

    protected IActionResult OperationStatusResult(OperationResult result) =>
        OperationStatusResult(result.Result, problemDetailsBuilder => result.Result switch
        {
            OperationResultType.FailedCancelledByEvent => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown operation status.")
                .Build()),
        });

    protected IActionResult MapRecycleBinQueryAttemptFailure(RecycleBinQueryResultType status, string contentType)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            RecycleBinQueryResultType.NotFound => NotFound(problemDetailsBuilder
                .WithTitle($"The {contentType} could not be found")
                .Build()),
            RecycleBinQueryResultType.NotTrashed => BadRequest(problemDetailsBuilder
                .WithTitle($"The {contentType} is not trashed")
                .WithDetail($"The {contentType} needs to be trashed for the parent-before-recycled relation to be created.")
                .Build()),
            RecycleBinQueryResultType.NoParentRecycleRelation => NotFound(problemDetailsBuilder
                .WithTitle("The parent relation could not be found")
                .WithDetail($"The relation between the parent and the {contentType} that should have been created when the {contentType} was deleted could not be found.")
                .Build()),
            RecycleBinQueryResultType.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle($"The original {contentType} parent could not be found")
                .Build()),
            RecycleBinQueryResultType.ParentIsTrashed => NotFound(problemDetailsBuilder
                .WithTitle($"The original {contentType} parent is in the recycle bin")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown recycle bin query type.")
                .Build()),
        });

    private IEntitySlim[] GetPagedRootEntities(int skip, int take, out long totalItems)
    {
        IEntitySlim[] rootEntities = _entityService
            .GetPagedTrashedChildren(RecycleBinRootKey, ItemObjectType, skip, take, out totalItems)
            .ToArray();

        return rootEntities;
    }

    private IEntitySlim[] GetPagedChildEntities(Guid parentKey, int skip, int take, out long totalItems)
    {
        IEntitySlim? parent = _entityService.Get(parentKey, ItemObjectType);
        if (parent == null || parent.Trashed == false)
        {
            // not much else we can do here but return nothing
            totalItems = 0;
            return Array.Empty<IEntitySlim>();
        }

        IEntitySlim[] children = _entityService
            .GetPagedTrashedChildren(parentKey, ItemObjectType, skip, take, out totalItems)
            .ToArray();

        return children;
    }

    private TItem[] MapRecycleBinViewModels(Guid? parentKey, IEntitySlim[] entities)
        => entities.Select(entity => MapRecycleBinViewModel(parentKey, entity)).ToArray();

    private PagedViewModel<TItem> PagedViewModel(IEnumerable<TItem> treeItemViewModels, long totalItems)
        => new() { Total = totalItems, Items = treeItemViewModels };
}
