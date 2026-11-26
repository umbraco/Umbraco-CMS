using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentDraftBranch;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides an API endpoint for promoting a document draft branch.
/// </summary>
[ApiVersion("1.0")]
public class PromoteDocumentDraftBranchController : DocumentDraftBranchControllerBase
{
    private readonly IContentDraftBranchService _contentDraftBranchService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromoteDocumentDraftBranchController"/> class.
    /// </summary>
    public PromoteDocumentDraftBranchController(
        IContentDraftBranchService contentDraftBranchService,
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentDraftBranchService = contentDraftBranchService;
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Promotes a document draft branch: its edited values are copied forward onto the node's actual default
    /// draft via a plain save. This does not publish anything.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch to promote.</param>
    /// <param name="requestModel">The cultures to promote.</param>
    [HttpPut("{branchId:guid}/promote")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Promotes a document draft branch.")]
    [EndpointDescription("Promotes a document draft branch identified by the provided Id, copying its content forward onto the node's default draft. This does not publish the document - publishing only ever happens as a separate editorial action or via the document's own scheduled publish date.")]
    public async Task<IActionResult> Promote(CancellationToken cancellationToken, Guid id, Guid branchId, PromoteDocumentDraftBranchRequestModel requestModel)
    {
        IContentDraftBranch? branch = _contentDraftBranchService.GetByKey(id, branchId);
        if (branch is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionPublish.ActionLetter, branch.ContentKey, requestModel.Cultures),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentDraftBranchOperationStatus> result =
            await _contentDraftBranchService.PromoteAsync(id, branchId, requestModel.Cultures, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : MapFailure(result.Result);
    }
}
