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
public class DescendantsTrackedReferenceController : TrackedReferenceControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public DescendantsTrackedReferenceController(ITrackedReferencesService trackedReferencesSkipTakeService, IUmbracoMapper umbracoMapper)
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
    [HttpGet("descendants/{parentId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RelationItemResponseModel>>> Descendants(Guid parentId, int skip = 0, int take = 20, bool filterMustBeIsDependency = true)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(parentId, skip, take, filterMustBeIsDependency);
        var pagedViewModel = new PagedViewModel<RelationItemResponseModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, RelationItemResponseModel>(relationItems.Items),
        };

        return await Task.FromResult(pagedViewModel);
    }
}
