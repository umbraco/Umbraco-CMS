using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

namespace Umbraco.Cms.Api.Management.Controllers.TrackedReference;

[ApiVersion("1.0")]
public class ItemsTrackedReferenceController : TrackedReferenceControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ItemsTrackedReferenceController(ITrackedReferencesService trackedReferencesSkipTakeService, IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a page list of the items used in any kind of relation from selected keys.
    /// </summary>
    /// <remarks>
    ///     Used when bulk deleting content/media and bulk unpublishing content (delete and unpublish on List view).
    ///     This is basically finding children of relations.
    /// </remarks>
    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RelationItemResponseModel>>> GetPagedReferencedItems([FromQuery(Name="id")]HashSet<Guid> ids, long skip = 0, long take = 20, bool filterMustBeIsDependency = true)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesSkipTakeService.GetPagedItemsWithRelationsAsync(ids, skip, take, filterMustBeIsDependency);
        var pagedViewModel = new PagedViewModel<RelationItemResponseModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, RelationItemResponseModel>(relationItems.Items),
        };

        return await Task.FromResult(pagedViewModel);
    }
}
