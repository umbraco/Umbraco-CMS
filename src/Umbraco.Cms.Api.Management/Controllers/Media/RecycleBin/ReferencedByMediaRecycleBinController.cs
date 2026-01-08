using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

[ApiVersion("1.0")]
public class ReferencedByMediaRecycleBinController : MediaRecycleBinControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IRelationTypePresentationFactory _relationTypePresentationFactory;

    public ReferencedByMediaRecycleBinController(
        IEntityService entityService,
        IMediaPresentationFactory mediaPresentationFactory,
        ITrackedReferencesService trackedReferencesService,
        IRelationTypePresentationFactory relationTypePresentationFactory)
        : base(entityService, mediaPresentationFactory)
    {
        _trackedReferencesService = trackedReferencesService;
        _relationTypePresentationFactory = relationTypePresentationFactory;
    }

    /// <summary>
    ///     Gets a paged list of tracked references for all items in the media recycle bin, so you can see where an item is being used.
    /// </summary>
    [HttpGet("referenced-by")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<IReferenceResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<IReferenceResponseModel>>> ReferencedBy(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesService.GetPagedRelationsForRecycleBinAsync(UmbracoObjectTypes.Media, skip, take, true);

        var pagedViewModel = new PagedViewModel<IReferenceResponseModel>
        {
            Total = relationItems.Total,
            Items = await _relationTypePresentationFactory.CreateReferenceResponseModelsAsync(relationItems.Items),
        };

        return pagedViewModel;
    }
}
