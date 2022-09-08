using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Server;

namespace Umbraco.Cms.ManagementApi.Controllers.TrackedReferences;

[ApiVersion("1.0")]
public class GetTrackedReferencesController : TrackedReferencesControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;

    public GetTrackedReferencesController(ITrackedReferencesService trackedReferencesService)
    {
        _trackedReferencesService = trackedReferencesService;
    }

    /// <summary>
    ///     Gets a page list of tracked references for the current item, so you can see where an item is being used.
    /// </summary>
    /// <remarks>
    ///     Used by info tabs on content, media etc. and for the delete and unpublish of single items.
    ///     This is basically finding parents of relations.
    /// </remarks>

    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ServerStatusViewModel), StatusCodes.Status200OK)]
    public ActionResult<PagedResult<RelationItem>> GetPagedReferences(
        int id,
        int pageNumber = 1,
        int pageSize = 100,
        bool filterMustBeIsDependency = false)
    {
        if (pageNumber <= 0 || pageSize <= 0)
        {
            return BadRequest("Both pageNumber and pageSize must be greater than zero");
        }

        return _trackedReferencesService.GetPagedRelationsForItem(id, pageNumber - 1, pageSize, filterMustBeIsDependency);
    }
}
