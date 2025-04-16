using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.References;

[ApiVersion("1.0")]
public class ReferencedDescendantsDocumentController : DocumentControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ReferencedDescendantsDocumentController(ITrackedReferencesService trackedReferencesSkipTakeService, IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paged list of the descendant nodes of the current item used in any kind of relation.
    /// </summary>
    /// <remarks>
    ///     Used when deleting and unpublishing a single item to check if this item has any descending items that are in any
    ///     kind of relation.
    ///     This is basically finding the descending items which are children in relations.
    /// </remarks>
    [HttpGet("{id:guid}/referenced-descendants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<ReferenceByIdModel>>> ReferencedDescendants(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(id, skip, take, true);
        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, ReferenceByIdModel>(relationItems.Items),
        };

        return pagedViewModel;
    }
}
