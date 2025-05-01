using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.References;

[ApiVersion("1.0")]
public class ReferencedByMediaController : MediaControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;

    public ReferencedByMediaController(ITrackedReferencesService trackedReferencesService, IRelationTypePresentationFactory relationTypePresentationFactory)
    {
        _trackedReferencesService = trackedReferencesService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
    }

    /// <summary>
    ///     Gets a page list of tracked references for the current item, so you can see where an item is being used.
    /// </summary>
    /// <remarks>
    ///     Used by info tabs on content, media etc. and for the delete and unpublish of single items.
    ///     This is basically finding parents of relations.
    /// </remarks>
    [HttpGet("{id:guid}/referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
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
}
