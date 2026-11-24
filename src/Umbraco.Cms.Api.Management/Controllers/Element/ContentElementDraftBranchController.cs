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
/// Provides an API endpoint for retrieving an element draft branch's own content, for editing.
/// </summary>
[ApiVersion("1.0")]
public class ContentElementDraftBranchController : ElementDraftBranchControllerBase
{
    private readonly IElementDraftBranchService _elementDraftBranchService;
    private readonly IElementPresentationFactory _elementPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentElementDraftBranchController"/> class.
    /// </summary>
    public ContentElementDraftBranchController(
        IElementDraftBranchService elementDraftBranchService,
        IElementPresentationFactory elementPresentationFactory,
        IAuthorizationService authorizationService)
    {
        _elementDraftBranchService = elementDraftBranchService;
        _elementPresentationFactory = elementPresentationFactory;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Retrieves an element draft branch's own content, for editing. The returned element reflects the
    /// branch's own version, not the node's default draft.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch whose content is to be retrieved.</param>
    [HttpGet("{branchId:guid}/content")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ElementResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets an element draft branch's content.")]
    [EndpointDescription("Gets the content of an element draft branch identified by the provided Id, for editing.")]
    public async Task<IActionResult> Content(CancellationToken cancellationToken, Guid id, Guid branchId)
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

        IElement? element = _elementDraftBranchService.GetContent(id, branchId);
        if (element is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchVersionNotFound);
        }

        ElementResponseModel model = _elementPresentationFactory.CreateResponseModel(element, new ContentScheduleCollection());
        return Ok(model);
    }
}
