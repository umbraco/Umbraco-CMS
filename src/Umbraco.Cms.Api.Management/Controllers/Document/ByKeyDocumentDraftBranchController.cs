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
/// Provides an API endpoint for retrieving a document draft branch by its unique identifier.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyDocumentDraftBranchController : DocumentDraftBranchControllerBase
{
    private readonly IContentDraftBranchService _contentDraftBranchService;
    private readonly IDocumentDraftBranchPresentationFactory _documentDraftBranchPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyDocumentDraftBranchController"/> class.
    /// </summary>
    public ByKeyDocumentDraftBranchController(
        IContentDraftBranchService contentDraftBranchService,
        IDocumentDraftBranchPresentationFactory documentDraftBranchPresentationFactory,
        IAuthorizationService authorizationService)
    {
        _contentDraftBranchService = contentDraftBranchService;
        _documentDraftBranchPresentationFactory = documentDraftBranchPresentationFactory;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Retrieves a document draft branch by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch to retrieve.</param>
    [HttpGet("{branchId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentDraftBranchResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document draft branch.")]
    [EndpointDescription("Gets a document draft branch identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id, Guid branchId)
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

        return Ok(_documentDraftBranchPresentationFactory.Create(branch));
    }
}
