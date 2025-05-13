using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.References;

[ApiVersion("1.0")]
public class AreReferencedMemberController : MemberControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AreReferencedMemberController(ITrackedReferencesService trackedReferencesSkipTakeService, IUmbracoMapper umbracoMapper)
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
    // [HttpGet("item")]
    [HttpGet("are-referenced")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<ReferenceByIdModel>>> GetPagedReferencedItems(
        CancellationToken cancellationToken,
        [FromQuery(Name="id")] HashSet<Guid> ids,
        int skip = 0,
        int take = 20)
    {
        PagedModel<Guid> distinctByKeyItemsWithReferencedRelations = await _trackedReferencesSkipTakeService.GetPagedKeysWithDependentReferencesAsync(ids, Constants.ObjectTypes.Member, skip, take);
        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = distinctByKeyItemsWithReferencedRelations.Total,
            Items = _umbracoMapper.MapEnumerable<Guid, ReferenceByIdModel>(distinctByKeyItemsWithReferencedRelations.Items),
        };

        return pagedViewModel;
    }
}
