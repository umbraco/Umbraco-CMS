using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.ElementDraftBranch;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Provides an API endpoint for abandoning an element draft branch.
/// </summary>
[ApiVersion("1.0")]
public class AbandonElementDraftBranchController : ElementDraftBranchControllerBase
{
    private readonly IElementDraftBranchService _elementDraftBranchService;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbandonElementDraftBranchController"/> class.
    /// </summary>
    public AbandonElementDraftBranchController(
        IElementDraftBranchService elementDraftBranchService,
        IAuthorizationService authorizationService)
    {
        _elementDraftBranchService = elementDraftBranchService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Abandons an active or stale element draft branch. The branch row is deleted immediately - it is not
    /// retained for the version-cleanup job to reap later.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch to abandon.</param>
    [HttpDelete("{branchId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Abandons an element draft branch.")]
    [EndpointDescription("Abandons an element draft branch identified by the provided Id, as an editorial deletion. This does not schedule a deletion - it takes effect immediately.")]
    public async Task<IActionResult> Abandon(CancellationToken cancellationToken, Guid id, Guid branchId)
    {
        IContentDraftBranch? branch = _elementDraftBranchService.GetByKey(id, branchId);
        if (branch is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementDelete.ActionLetter, branch.ContentKey),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentDraftBranchOperationStatus> result = _elementDraftBranchService.Abandon(id, branchId);

        return result.Success
            ? Ok()
            : MapFailure(result.Result);
    }
}
