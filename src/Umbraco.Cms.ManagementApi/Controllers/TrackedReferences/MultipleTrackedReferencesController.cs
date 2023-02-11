using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.TrackedReferences;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.ManagementApi.Controllers.TrackedReferences;

public class MultipleTrackedReferencesController : TrackedReferencesControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public MultipleTrackedReferencesController(ITrackedReferencesService trackedReferencesSkipTakeService, IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a page list of the items used in any kind of relation from selected integer ids.
    /// </summary>
    /// <remarks>
    ///     Used when bulk deleting content/media and bulk unpublishing content (delete and unpublish on List view).
    ///     This is basically finding children of relations.
    /// </remarks>
    [HttpGet("multiple")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RelationItemViewModel>>> GetPagedReferencedItems([FromQuery]int[] ids, long skip, long take, bool? filterMustBeIsDependency)
    {
        PagedModel<RelationItemModel> relationItems = _trackedReferencesSkipTakeService.GetPagedItemsWithRelations(ids, skip, take, filterMustBeIsDependency ?? true);
        var pagedViewModel = new PagedViewModel<RelationItemViewModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, RelationItemViewModel>(relationItems.Items),
        };

        return await Task.FromResult(pagedViewModel);
    }
}
