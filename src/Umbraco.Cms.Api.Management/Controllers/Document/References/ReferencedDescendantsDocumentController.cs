using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Document.References;

/// <summary>
/// Controller responsible for managing documents that are referenced as descendants.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedDescendantsDocumentController : DocumentControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedDescendantsDocumentController"/> class.
    /// </summary>
    /// <param name="trackedReferencesSkipTakeService">Service used to retrieve tracked references to documents, supporting pagination via skip and take operations.</param>
    /// <param name="umbracoMapper">The mapper used for converting between Umbraco domain models and API models.</param>
    public ReferencedDescendantsDocumentController(
        ITrackedReferencesService trackedReferencesSkipTakeService,
        IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// [Obsolete] Retrieves a paged list of documents that are referenced as descendants by the specified document ID.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document whose referenced descendants are to be retrieved.</param>
    /// <param name="skip">The number of items to skip for paging.</param>
    /// <param name="take">The maximum number of items to return for paging.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{ReferenceByIdModel}"/> of referenced descendant documents.</returns>
    /// <remarks>
    /// This method is obsolete. Use <c>ReferencedDescendants2</c> instead. Scheduled for removal in Umbraco 19.
    /// </remarks>
    [Obsolete("Use the ReferencedDescendants2 action method instead. Scheduled for removal in Umbraco 19, when ReferencedDescendants2 will be renamed back to ReferencedDescendants.")]
    [NonAction]
    public async Task<ActionResult<PagedViewModel<ReferenceByIdModel>>> ReferencedDescendants(
    CancellationToken cancellationToken,
    Guid id,
    int skip = 0,
    int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(id, skip, take, true);
        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, ReferenceByIdModel>(relationItems.Items),
        };

        return pagedViewModel;
    }

/// <summary>
///     Gets a paged list of descendant nodes of the specified item that are involved in any kind of relation.
/// </summary>
/// <remarks>
///     Used when deleting or unpublishing a single item to check if it has any descendant items that participate in any relation.
///     This method identifies descendant items that are children in relations.
/// </remarks>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <param name="id">The unique identifier of the item whose referenced descendants are being retrieved.</param>
/// <param name="skip">The number of items to skip when paginating results.</param>
/// <param name="take">The maximum number of items to return in the paginated result.</param>
/// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paged list of referenced descendant documents.</returns>
    [HttpGet("{id:guid}/referenced-descendants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets document descendants that are referenced.")]
    [EndpointDescription("Gets a paginated collection of descendant documents that are referenced by other content.")]
    public async Task<IActionResult> ReferencedDescendants2(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> relationItemsAttempt = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(id, UmbracoObjectTypes.Document, skip, take, true);

        if (relationItemsAttempt.Success is false)
        {
            return GetReferencesOperationStatusResult(relationItemsAttempt.Status);
        }

        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = relationItemsAttempt.Result.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, ReferenceByIdModel>(relationItemsAttempt.Result.Items),
        };

        return Ok(pagedViewModel);
    }
}
