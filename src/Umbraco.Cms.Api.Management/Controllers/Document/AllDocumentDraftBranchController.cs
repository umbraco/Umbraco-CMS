using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentDraftBranch;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides an API endpoint for retrieving all named draft branches of a document.
/// </summary>
[ApiVersion("1.0")]
public class AllDocumentDraftBranchController : DocumentDraftBranchControllerBase
{
    private readonly IContentDraftBranchService _contentDraftBranchService;
    private readonly IDocumentDraftBranchPresentationFactory _documentDraftBranchPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllDocumentDraftBranchController"/> class.
    /// </summary>
    public AllDocumentDraftBranchController(
        IContentDraftBranchService contentDraftBranchService,
        IDocumentDraftBranchPresentationFactory documentDraftBranchPresentationFactory)
    {
        _contentDraftBranchService = contentDraftBranchService;
        _documentDraftBranchPresentationFactory = documentDraftBranchPresentationFactory;
    }

    /// <summary>
    /// Retrieves all named draft branches for a document, in creation order.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document whose draft branches are to be retrieved.</param>
    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentDraftBranchResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets all draft branches for a document.")]
    [EndpointDescription("Gets all named draft branches for a document, in creation order.")]
    public Task<IActionResult> All(CancellationToken cancellationToken, Guid id)
    {
        IEnumerable<IContentDraftBranch> branches = _contentDraftBranchService.GetByContentKey(id);
        return Task.FromResult<IActionResult>(Ok(_documentDraftBranchPresentationFactory.CreateMultiple(branches)));
    }
}
