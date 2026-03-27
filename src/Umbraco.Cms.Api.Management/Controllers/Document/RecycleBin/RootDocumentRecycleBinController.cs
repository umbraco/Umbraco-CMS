using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

/// <summary>
/// Provides API endpoints for managing items in the root document recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class RootDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootDocumentRecycleBinController"/> class, which manages operations related to the root of the document recycle bin.
    /// </summary>
    /// <param name="entityService">Service used for entity management operations.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    public RootDocumentRecycleBinController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
    }

    /// <summary>Gets a paginated collection of documents at the root level of the recycle bin.</summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip in the result set.</param>
    /// <param name="take">The number of items to take in the result set.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ActionResult with a paged view model of document recycle bin item response models.</returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets documents at the root of the recycle bin.")]
    [EndpointDescription("Gets a paginated collection of documents at the root level of the recycle bin.")]
    public async Task<ActionResult<PagedViewModel<DocumentRecycleBinItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
