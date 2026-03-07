using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document.RecycleBin;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

    /// <summary>
    /// Controller responsible for managing recycle bin operations related to documents that share the same parent (siblings) within the content tree.
    /// Provides endpoints for restoring or permanently deleting sibling documents from the recycle bin.
    /// </summary>
[ApiVersion("1.0")]
public class SiblingsDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsDocumentRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the recycle bin context.</param>
    /// <param name="documentPresentationFactory">Factory responsible for creating document presentation models.</param>
    public SiblingsDocumentRecycleBinController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
    }

    /// <summary>
    /// Retrieves sibling documents in the recycle bin that are at the same hierarchical level as the specified document ID.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The ID of the document whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling documents to include before the target document.</param>
    /// <param name="after">The number of sibling documents to include after the target document.</param>
    /// <param name="dataTypeId">An optional data type ID to filter the sibling documents.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{T}"/> of <see cref="DocumentRecycleBinItemResponseModel"/> representing the sibling documents.</returns>
    [HttpGet("siblings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(SubsetViewModel<DocumentRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets sibling documents in the recycle bin.")]
    [EndpointDescription("Gets a collection of sibling documents in the recycle bin at the same level as the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<DocumentRecycleBinItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after, Guid? dataTypeId = null)
        => await GetSiblings(target, before, after);
}
