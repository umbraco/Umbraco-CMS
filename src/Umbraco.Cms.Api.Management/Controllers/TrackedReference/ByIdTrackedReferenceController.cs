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
public class ByIdTrackedReferenceController : TrackedReferenceControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByIdTrackedReferenceController(ITrackedReferencesService trackedReferencesService, IUmbracoMapper umbracoMapper)
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
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RelationItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RelationItemResponseModel>>> Get(
        Guid id,
        int skip = 0,
        int take = 20,
        bool filterMustBeIsDependency = false)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesService.GetPagedRelationsForItemAsync(id, skip, take, filterMustBeIsDependency);

        var pagedViewModel = new PagedViewModel<RelationItemResponseModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, RelationItemResponseModel>(relationItems.Items),
        };

        return await Task.FromResult(pagedViewModel);
    }
}
