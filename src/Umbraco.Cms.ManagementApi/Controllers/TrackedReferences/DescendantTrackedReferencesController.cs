using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Server;
using Umbraco.Cms.ManagementApi.ViewModels.TrackedReferences;
using Umbraco.New.Cms.Core.Models.TrackedReferences;

namespace Umbraco.Cms.ManagementApi.Controllers.TrackedReferences;

public class DescendantTrackedReferencesController : TrackedReferencesControllerBase
{
    private readonly ITrackedReferencesSkipTakeService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public DescendantTrackedReferencesController(ITrackedReferencesSkipTakeService trackedReferencesSkipTakeService, IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a page list of the child nodes of the current item used in any kind of relation.
    /// </summary>
    /// <remarks>
    ///     Used when deleting and unpublishing a single item to check if this item has any descending items that are in any
    ///     kind of relation.
    ///     This is basically finding the descending items which are children in relations.
    /// </remarks>
    [HttpGet("descendants/{parentId:int}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ServerStatusViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RelationItemViewModel>>> Descendants(int parentId, long skip, long take, bool? filterMustBeIsDependency)
    {
        PagedViewModel<RelationItemModel> relationItems = _trackedReferencesSkipTakeService.GetPagedDescendantsInReferences(parentId, skip, take, filterMustBeIsDependency ?? true);
        return new PagedViewModel<RelationItemViewModel>
        {
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, RelationItemViewModel>(relationItems.Items),
            Total = relationItems.Total
        };
    }
}
