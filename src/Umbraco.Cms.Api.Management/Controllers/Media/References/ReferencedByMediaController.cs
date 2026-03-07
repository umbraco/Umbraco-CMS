using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Media.References;

/// <summary>
/// Controller for managing references to a specific media item.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedByMediaController : MediaControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByMediaController"/> class.
    /// </summary>
    /// <param name="trackedReferencesService">Service for retrieving and managing tracked references to media items.</param>
    /// <param name="relationTypePresentationFactory">Factory for creating presentation models of relation types between media entities.</param>
    public ReferencedByMediaController(
        ITrackedReferencesService trackedReferencesService,
        IRelationTypePresentationFactory relationTypePresentationFactory)
    {
        _trackedReferencesService = trackedReferencesService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
    }

    /// <summary>
    /// Retrieves a paged list of media items that reference the specified media item.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the media item for which referencing items are retrieved.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for paging).</param>
    /// <param name="take">The maximum number of items to return (used for paging).</param>
    /// <returns>A task representing the asynchronous operation. The result contains a <see cref="PagedViewModel{IReferenceResponseModel}"/> with media items that reference the specified item.</returns>
    /// <remarks>
    /// This method is obsolete. Use <c>ReferencedBy2</c> instead. Scheduled for removal in Umbraco 19, when <c>ReferencedBy2</c> will be renamed back to <c>ReferencedBy</c>.
    /// </remarks>
    [Obsolete("Use the ReferencedBy2 action method instead. Scheduled for removal in Umbraco 19, when ReferencedBy2 will be renamed back to ReferencedBy.")]
    [NonAction]
    public async Task<ActionResult<PagedViewModel<IReferenceResponseModel>>> ReferencedBy(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesService.GetPagedRelationsForItemAsync(id, skip, take, true);

        var pagedViewModel = new PagedViewModel<IReferenceResponseModel>
        {
            Total = relationItems.Total,
            Items = await _relationTypePresentationFactory.CreateReferenceResponseModelsAsync(relationItems.Items),
        };

        return pagedViewModel;
    }

/// <summary>
///     Retrieves a paginated list of items that reference the specified media item.
/// </summary>
/// <remarks>
///     Used by info tabs on content, media, etc., and during the delete and unpublish operations for single items.
///     This method essentially finds parent items in relations.
/// </remarks>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <param name="id">The unique identifier of the media item to find references for.</param>
/// <param name="skip">The number of items to skip before starting to collect the result set (for pagination).</param>
/// <param name="take">The number of items to return in the result set (for pagination).</param>
/// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paginated list of references to the specified media item.</returns>
    [HttpGet("{id:guid}/referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a collection of items that reference a media item.")]
    [EndpointDescription("Gets a paginated collection of items that reference the media item identified by the provided Id.")]
    public async Task<IActionResult> ReferencedBy2(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> relationItemsAttempt = await _trackedReferencesService.GetPagedRelationsForItemAsync(id, UmbracoObjectTypes.Media, skip, take, true);

        if (relationItemsAttempt.Success is false)
        {
            return GetReferencesOperationStatusResult(relationItemsAttempt.Status);
        }

        var pagedViewModel = new PagedViewModel<IReferenceResponseModel>
        {
            Total = relationItemsAttempt.Result.Total,
            Items = await _relationTypePresentationFactory.CreateReferenceResponseModelsAsync(relationItemsAttempt.Result.Items),
        };

        return Ok(pagedViewModel);
    }
}
