using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentDraftBranch;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides an API endpoint for abandoning a document draft branch.
/// </summary>
[ApiVersion("1.0")]
public class AbandonDocumentDraftBranchController : DocumentDraftBranchControllerBase
{
    private readonly IContentDraftBranchService _contentDraftBranchService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbandonDocumentDraftBranchController"/> class.
    /// </summary>
    public AbandonDocumentDraftBranchController(
        IContentDraftBranchService contentDraftBranchService,
        IAuthorizationService authorizationService)
    {
        _contentDraftBranchService = contentDraftBranchService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Abandons an active or stale document draft branch. The branch row is deleted immediately - it is not
    /// retained for the version-cleanup job to reap later.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch to abandon.</param>
    [HttpDelete("{branchId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Abandons a document draft branch.")]
    [EndpointDescription("Abandons a document draft branch identified by the provided Id, as an editorial deletion. This does not schedule a deletion - it takes effect immediately.")]
    public async Task<IActionResult> Abandon(CancellationToken cancellationToken, Guid id, Guid branchId)
    {
        IContentDraftBranch? branch = _contentDraftBranchService.GetByKey(id, branchId);
        if (branch is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionDelete.ActionLetter, branch.ContentKey),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentDraftBranchOperationStatus> result = _contentDraftBranchService.Abandon(id, branchId);

        return result.Success
            ? Ok()
            : MapFailure(result.Result);
    }
}
