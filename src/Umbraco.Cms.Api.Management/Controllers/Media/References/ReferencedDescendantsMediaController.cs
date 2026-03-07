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

namespace Umbraco.Cms.Api.Management.Controllers.Media.References;

    /// <summary>
    /// Controller responsible for managing media items that are referenced as descendants.
    /// </summary>
[ApiVersion("1.0")]
public class ReferencedDescendantsMediaController : MediaControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedDescendantsMediaController"/> class.
    /// </summary>
    /// <param name="trackedReferencesSkipTakeService">Service for retrieving tracked references with skip/take pagination.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco domain models to API models.</param>
    public ReferencedDescendantsMediaController(
        ITrackedReferencesService trackedReferencesSkipTakeService,
        IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
    }

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
///     Gets a paginated list of descendant media items of the specified item that are referenced in any kind of relation.
/// </summary>
/// <remarks>
///     Used when deleting or unpublishing a single media item to check if it has any descendant items that are referenced in any relation.
///     This operation finds descendant items that are children in relations.
/// </remarks>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <param name="id">The unique identifier of the media item whose referenced descendants are to be retrieved.</param>
/// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
/// <param name="take">The maximum number of items to return (used for pagination).</param>
/// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paginated list of referenced descendant media items.</returns>
    [HttpGet("{id:guid}/referenced-descendants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets media descendants that are referenced.")]
    [EndpointDescription("Gets a paginated collection of descendant media items that are referenced by other content.")]
    public async Task<IActionResult> ReferencedDescendants2(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> relationItemsAttempt = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(id, UmbracoObjectTypes.Media, skip, take, true);

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
