using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Server;
using Umbraco.Cms.ManagementApi.ViewModels.TrackedReferences;
using Umbraco.New.Cms.Core.Models.TrackedReferences;

namespace Umbraco.Cms.ManagementApi.Controllers.TrackedReferences;

[ApiVersion("1.0")]
public class GetTrackedReferencesController : TrackedReferencesControllerBase
{
    private readonly ITrackedReferencesSkipTakeService _trackedReferencesService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetTrackedReferencesController(ITrackedReferencesSkipTakeService trackedReferencesService, IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesService = trackedReferencesService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a page list of tracked references for the current item, so you can see where an item is being used.
    /// </summary>
    /// <remarks>
    ///     Used by info tabs on content, media etc. and for the delete and unpublish of single items.
    ///     This is basically finding parents of relations.
    /// </remarks>
    [HttpGet("{id:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerStatusViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RelationItemViewModel>>> Get(
        int id,
        long skip,
        long take,
        bool? filterMustBeIsDependency)
    {

        PagedViewModel<RelationItemModel> relationItems = _trackedReferencesService.GetPagedRelationsForItem(id, skip, take, filterMustBeIsDependency ?? false);
        return new PagedViewModel<RelationItemViewModel>
        {
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, RelationItemViewModel>(relationItems.Items),
            Total = relationItems.Total
        };
    }
}
