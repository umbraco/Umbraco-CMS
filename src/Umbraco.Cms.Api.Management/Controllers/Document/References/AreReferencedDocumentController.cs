using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.References;

/// <summary>
/// Controller responsible for checking whether documents are referenced by other entities.
/// </summary>
[ApiVersion("1.0")]
public class AreReferencedDocumentController : DocumentControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.References.AreReferencedDocumentController"/> class.
    /// </summary>
    /// <param name="trackedReferencesSkipTakeService">The service used to retrieve tracked references with skip and take (paging) functionality.</param>
    /// <param name="umbracoMapper">The <see cref="IUmbracoMapper"/> instance used for mapping objects within Umbraco.</param>
    public AreReferencedDocumentController(ITrackedReferencesService trackedReferencesSkipTakeService, IUmbracoMapper umbracoMapper)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
    }

/// <summary>
///     Gets a paged list of items that reference the specified documents via any kind of relation.
/// </summary>
/// <remarks>
///     Used when bulk deleting content/media and bulk unpublishing content (delete and unpublish on List view).
///     This is essentially finding the children in relations where the selected documents are referenced.
/// </remarks>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <param name="ids">The set of document IDs to find referencing items for.</param>
/// <param name="skip">The number of items to skip when paging results.</param>
/// <param name="take">The maximum number of items to return in the result set.</param>
/// <returns>A paged list of items that reference the specified documents.</returns>
    [HttpGet("are-referenced")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of items that reference documents.")]
    [EndpointDescription("Gets a paginated collection of items that reference the documents identified by the provided Ids.")]
    public async Task<ActionResult<PagedViewModel<ReferenceByIdModel>>> GetPagedReferencedItems(
        CancellationToken cancellationToken,
        [FromQuery(Name="id")]HashSet<Guid> ids,
        int skip = 0,
        int take = 20)
    {
        PagedModel<Guid> distinctByKeyItemsWithReferencedRelations = await _trackedReferencesSkipTakeService.GetPagedKeysWithDependentReferencesAsync(ids, Constants.ObjectTypes.Document, skip, take);
        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = distinctByKeyItemsWithReferencedRelations.Total,
            Items = _umbracoMapper.MapEnumerable<Guid, ReferenceByIdModel>(distinctByKeyItemsWithReferencedRelations.Items),
        };

        return pagedViewModel;
    }
}
