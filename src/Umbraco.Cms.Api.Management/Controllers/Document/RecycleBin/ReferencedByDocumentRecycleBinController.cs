using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

    /// <summary>
    /// Controller for handling documents that are referenced by items in the document recycle bin.
    /// </summary>
[ApiVersion("1.0")]
public class ReferencedByDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByDocumentRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities within the system.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="trackedReferencesService">Service for tracking and retrieving references between entities.</param>
    /// <param name="relationTypePresentationFactory">Factory for creating relation type presentation models.</param>
    public ReferencedByDocumentRecycleBinController(
        IEntityService entityService,
        IDocumentPresentationFactory documentPresentationFactory,
        ITrackedReferencesService trackedReferencesService,
        IRelationTypePresentationFactory relationTypePresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
        _trackedReferencesService = trackedReferencesService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
    }

/// <summary>
/// Gets a paged list of tracked references for all items in the document recycle bin, allowing you to see where each item is referenced.
/// </summary>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <param name="skip">The number of items to skip before starting to collect the result set.</param>
/// <param name="take">The maximum number of items to return.</param>
/// <returns>
/// A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{IReferenceResponseModel}"/> of referencing items.
/// </returns>
    [HttpGet("referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets items referencing a document in the recycle bin.")]
    [EndpointDescription("Gets a paginated collection of items that reference the document in the recycle bin.")]
    public async Task<ActionResult<PagedViewModel<IReferenceResponseModel>>> ReferencedBy(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesService.GetPagedRelationsForRecycleBinAsync(UmbracoObjectTypes.Document, skip, take, true);

        var pagedViewModel = new PagedViewModel<IReferenceResponseModel>
        {
            Total = relationItems.Total,
            Items = await _relationTypePresentationFactory.CreateReferenceResponseModelsAsync(relationItems.Items),
        };

        return pagedViewModel;
    }
}
