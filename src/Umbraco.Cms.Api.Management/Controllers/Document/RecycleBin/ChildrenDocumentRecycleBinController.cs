using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

    /// <summary>
    /// Controller responsible for managing operations related to the child documents within the recycle bin.
    /// </summary>
[ApiVersion("1.0")]
public class ChildrenDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenDocumentRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the recycle bin.</param>
    /// <param name="documentPresentationFactory">Factory responsible for creating document presentation models.</param>
    public ChildrenDocumentRecycleBinController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of documents that are children of the specified parent document in the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentId">The unique identifier (GUID) of the parent document whose children are to be retrieved.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set. Defaults to 0.</param>
    /// <param name="take">The maximum number of items to return. Defaults to 100.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="PagedViewModel{T}"/> of <see cref="DocumentRecycleBinItemResponseModel"/> representing the child documents in the recycle bin.
    /// </returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of documents in the recycle bin.")]
    [EndpointDescription("Gets a paginated collection of documents that are children of the provided parent in the recycle bin.")]
    public async Task<ActionResult<PagedViewModel<DocumentRecycleBinItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100)
        => await GetChildren(parentId, skip, take);
}
