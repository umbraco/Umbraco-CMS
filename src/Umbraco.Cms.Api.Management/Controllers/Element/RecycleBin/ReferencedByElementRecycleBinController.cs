using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

/// <summary>
/// API controller responsible for retrieving tracked references for elements in the recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedByElementRecycleBinController : ElementRecycleBinControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedByElementRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    /// <param name="trackedReferencesService">Service for tracking content references.</param>
    /// <param name="relationTypePresentationFactory">Factory for creating relation type presentation models.</param>
    public ReferencedByElementRecycleBinController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory,
        ITrackedReferencesService trackedReferencesService,
        IRelationTypePresentationFactory relationTypePresentationFactory)
        : base(entityService, elementPresentationFactory)
    {
        _trackedReferencesService = trackedReferencesService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
    }

    /// <summary>
    /// Gets a paginated collection of tracked references for all items in the element recycle bin.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to return for pagination.</param>
    /// <returns>A paginated collection of reference response models.</returns>
    [HttpGet("referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets items referencing an element in the recycle bin.")]
    [EndpointDescription("Gets a paginated collection of items that reference the element in the recycle bin.")]
    public async Task<ActionResult<PagedViewModel<IReferenceResponseModel>>> ReferencedBy(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesService
            .GetPagedRelationsForRecycleBinAsync(UmbracoObjectTypes.Element, skip, take, true);

        var pagedViewModel = new PagedViewModel<IReferenceResponseModel>
        {
            Total = relationItems.Total,
            Items = await _relationTypePresentationFactory.CreateReferenceResponseModelsAsync(relationItems.Items),
        };

        return pagedViewModel;
    }
}
