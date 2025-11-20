using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

[ApiVersion("1.0")]
public class ChildrenDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    public ChildrenDocumentRecycleBinController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
    }

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
