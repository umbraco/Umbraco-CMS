using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.References;

/// <summary>
/// Controller for checking whether media items are referenced elsewhere in the system.
/// </summary>
[ApiVersion("1.0")]
public class AreReferencedMediaController : MediaControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Media.References.AreReferencedMediaController"/> class.
    /// </summary>
    /// <param name="trackedReferencesSkipTakeService">Service for retrieving tracked media references with support for pagination (skip and take).</param>
    /// <param name="umbracoMapper">The Umbraco mapper used for mapping between models.</param>
    public AreReferencedMediaController(ITrackedReferencesService trackedReferencesSkipTakeService, IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
    }

/// <summary>
///     Gets a paginated list of media items that are referenced in any kind of relation from the specified keys.
/// </summary>
/// <remarks>
///     Typically used when bulk deleting or unpublishing content or media (such as in List view operations).
///     This method finds media items that are children in relations, i.e., items that are referenced by the provided IDs.
/// </remarks>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <param name="ids">A set of media item IDs to check for referenced relations.</param>
/// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
/// <param name="take">The maximum number of items to return (used for pagination).</param>
/// <returns>A paged view model containing media items that are referenced by the specified IDs.</returns>
    // [HttpGet("item")]
    [HttpGet("are-referenced")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of referenced media items.")]
    [EndpointDescription("Gets a paginated collection of media items that are referenced, identified by the provided Ids.")]
    public async Task<ActionResult<PagedViewModel<ReferenceByIdModel>>> GetPagedReferencedItems(
        CancellationToken cancellationToken,
        [FromQuery(Name="id")] HashSet<Guid> ids,
        int skip = 0,
        int take = 20)
    {
        PagedModel<Guid> distinctByKeyItemsWithReferencedRelations = await _trackedReferencesSkipTakeService.GetPagedKeysWithDependentReferencesAsync(ids, Constants.ObjectTypes.Media, skip, take);
        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = distinctByKeyItemsWithReferencedRelations.Total,
            Items = _umbracoMapper.MapEnumerable<Guid, ReferenceByIdModel>(distinctByKeyItemsWithReferencedRelations.Items),
        };

        return pagedViewModel;
    }
}
