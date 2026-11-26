using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.ElementDraftBranch;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Provides an API endpoint for promoting an element draft branch.
/// </summary>
[ApiVersion("1.0")]
public class PromoteElementDraftBranchController : ElementDraftBranchControllerBase
{
    private readonly IElementDraftBranchService _elementDraftBranchService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromoteElementDraftBranchController"/> class.
    /// </summary>
    public PromoteElementDraftBranchController(
        IElementDraftBranchService elementDraftBranchService,
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _elementDraftBranchService = elementDraftBranchService;
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Promotes an element draft branch: its edited values are copied forward onto the node's actual default
    /// draft via a plain save. This does not publish anything.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch to promote.</param>
    /// <param name="requestModel">The cultures to promote.</param>
    [HttpPut("{branchId:guid}/promote")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Promotes an element draft branch.")]
    [EndpointDescription("Promotes an element draft branch identified by the provided Id, copying its content forward onto the node's default draft. This does not publish the element - publishing only ever happens as a separate editorial action or via the element's own scheduled publish date.")]
    public async Task<IActionResult> Promote(CancellationToken cancellationToken, Guid id, Guid branchId, PromoteElementDraftBranchRequestModel requestModel)
    {
        IContentDraftBranch? branch = _elementDraftBranchService.GetByKey(id, branchId);
        if (branch is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementPublish.ActionLetter, branch.ContentKey, requestModel.Cultures),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentDraftBranchOperationStatus> result =
            await _elementDraftBranchService.PromoteAsync(id, branchId, requestModel.Cultures, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : MapFailure(result.Result);
    }
}
