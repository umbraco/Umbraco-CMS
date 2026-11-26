using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.ElementDraftBranch;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Provides an API endpoint for retrieving an element draft branch by its unique identifier.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyElementDraftBranchController : ElementDraftBranchControllerBase
{
    private readonly IElementDraftBranchService _elementDraftBranchService;
    private readonly IElementDraftBranchPresentationFactory _elementDraftBranchPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyElementDraftBranchController"/> class.
    /// </summary>
    public ByKeyElementDraftBranchController(
        IElementDraftBranchService elementDraftBranchService,
        IElementDraftBranchPresentationFactory elementDraftBranchPresentationFactory,
        IAuthorizationService authorizationService)
    {
        _elementDraftBranchService = elementDraftBranchService;
        _elementDraftBranchPresentationFactory = elementDraftBranchPresentationFactory;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Retrieves an element draft branch by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch to retrieve.</param>
    [HttpGet("{branchId:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ElementDraftBranchResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets an element draft branch.")]
    [EndpointDescription("Gets an element draft branch identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id, Guid branchId)
    {
        IContentDraftBranch? branch = _elementDraftBranchService.GetByKey(id, branchId);
        if (branch is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementBrowse.ActionLetter, branch.ContentKey),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        return Ok(_elementDraftBranchPresentationFactory.Create(branch));
    }
}
