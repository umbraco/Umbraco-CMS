using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentDraftBranch;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides an API endpoint for retrieving a document draft branch's own content, for editing.
/// </summary>
[ApiVersion("1.0")]
public class ContentDocumentDraftBranchController : DocumentDraftBranchControllerBase
{
    private readonly IContentDraftBranchService _contentDraftBranchService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentDocumentDraftBranchController"/> class.
    /// </summary>
    public ContentDocumentDraftBranchController(
        IContentDraftBranchService contentDraftBranchService,
        IDocumentPresentationFactory documentPresentationFactory,
        IAuthorizationService authorizationService)
    {
        _contentDraftBranchService = contentDraftBranchService;
        _documentPresentationFactory = documentPresentationFactory;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Retrieves a document draft branch's own content, for editing. The returned document reflects the
    /// branch's own version, not the node's default draft.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch whose content is to be retrieved.</param>
    [HttpGet("{branchId:guid}/content")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document draft branch's content.")]
    [EndpointDescription("Gets the content of a document draft branch identified by the provided Id, for editing.")]
    public async Task<IActionResult> Content(CancellationToken cancellationToken, Guid id, Guid branchId)
    {
        IContentDraftBranch? branch = _contentDraftBranchService.GetByKey(id, branchId);
        if (branch is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, branch.ContentKey),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IContent? content = _contentDraftBranchService.GetContent(id, branchId);
        if (content is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchVersionNotFound);
        }

        DocumentResponseModel model = await _documentPresentationFactory.CreateResponseModelAsync(content, new ContentScheduleCollection());
        return Ok(model);
    }
}
