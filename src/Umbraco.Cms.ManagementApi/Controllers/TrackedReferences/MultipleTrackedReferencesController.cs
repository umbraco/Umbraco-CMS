using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.TrackedReferences;
using Umbraco.New.Cms.Core.Models.TrackedReferences;

namespace Umbraco.Cms.ManagementApi.Controllers.TrackedReferences;

public class MultipleTrackedReferencesController : TrackedReferencesControllerBase
{
    private readonly ITrackedReferencesSkipTakeService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public MultipleTrackedReferencesController(ITrackedReferencesSkipTakeService trackedReferencesSkipTakeService, IUmbracoMapper umbracoMapper)
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
    [HttpPost("multiple")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RelationItemViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RelationItemViewModel>>> GetPagedReferencedItems([FromBody] int[] ids, long skip, long take, bool? filterMustBeIsDependency)
    {
        PagedViewModel<RelationItemModel> relationItems =  _trackedReferencesSkipTakeService.GetPagedItemsWithRelations(ids, skip, take, filterMustBeIsDependency ?? true);

        return new PagedViewModel<RelationItemViewModel>
        {
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, RelationItemViewModel>(relationItems.Items),
            Total = relationItems.Total
        };
    }
}
