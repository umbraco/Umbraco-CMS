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

namespace Umbraco.Cms.Api.Management.Controllers.Element.References;

/// <summary>
/// API controller responsible for retrieving tracked references for a specific element.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedByElementController : ElementControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByElementController"/> class.
    /// </summary>
    /// <param name="trackedReferencesService">Service for tracking content references.</param>
    /// <param name="relationTypePresentationFactory">Factory for creating relation type presentation models.</param>
    public ReferencedByElementController(
        ITrackedReferencesService trackedReferencesService,
        IRelationTypePresentationFactory relationTypePresentationFactory)
    {
        _trackedReferencesService = trackedReferencesService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
    }

    /// <summary>
    ///     Gets a paged list of tracked references for the current item, so you can see where an element is being used.
    /// </summary>
    /// <remarks>
    ///     Used by info tabs on elements and for the delete and unpublish of single items.
    ///     This is basically finding parents of relations.
    /// </remarks>
    [HttpGet("{id:guid}/referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets items referencing an element.")]
    [EndpointDescription("Gets a paginated collection of items that reference the element identified by the provided Id.")]
    public async Task<IActionResult> ReferencedBy(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> relationItemsAttempt =
            await _trackedReferencesService.GetPagedRelationsForItemAsync(
                id,
                UmbracoObjectTypes.Element,
                skip,
                take,
                true);

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
