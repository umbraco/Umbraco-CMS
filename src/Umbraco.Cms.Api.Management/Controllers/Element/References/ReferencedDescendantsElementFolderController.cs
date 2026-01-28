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

namespace Umbraco.Cms.Api.Management.Controllers.Element.References;

[ApiVersion("1.0")]
public class ReferencedDescendantsElementFolderController : ElementControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ReferencedDescendantsElementFolderController(
        ITrackedReferencesService trackedReferencesService,
        IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesService = trackedReferencesService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paged list of the descendant nodes of the element folder used in any kind of relation.
    /// </summary>
    /// <remarks>
    ///     Used when deleting an element folder to check if it has any descending items that are in any kind of relation.
    /// </remarks>
    [HttpGet("folder/{id:guid}/referenced-descendants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReferencedDescendants(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> relationItemsAttempt =
            await _trackedReferencesService.GetPagedDescendantsInReferencesAsync(
                id,
                UmbracoObjectTypes.ElementContainer,
                skip,
                take,
                true);

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
