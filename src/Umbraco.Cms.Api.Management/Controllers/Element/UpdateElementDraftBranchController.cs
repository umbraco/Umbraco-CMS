using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.ElementDraftBranch;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Provides an API endpoint for saving edits to an element draft branch's own content.
/// </summary>
[ApiVersion("1.0")]
public class UpdateElementDraftBranchController : ElementDraftBranchControllerBase
{
    private readonly IElementDraftBranchService _elementDraftBranchService;
    private readonly IElementEditingService _elementEditingService;
    private readonly IElementEditingPresentationFactory _elementEditingPresentationFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateElementDraftBranchController"/> class.
    /// </summary>
    public UpdateElementDraftBranchController(
        IElementDraftBranchService elementDraftBranchService,
        IElementEditingService elementEditingService,
        IElementEditingPresentationFactory elementEditingPresentationFactory,
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _elementDraftBranchService = elementDraftBranchService;
        _elementEditingService = elementEditingService;
        _elementEditingPresentationFactory = elementEditingPresentationFactory;
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Saves edits to an element draft branch's own content. This is a plain save only - it never publishes.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch to update.</param>
    /// <param name="requestModel">The model containing the updated element details.</param>
    [HttpPut("{branchId:guid}/content")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates an element draft branch.")]
    [EndpointDescription("Saves edits to an element draft branch identified by the provided Id. This is a plain save only - it never publishes.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        Guid branchId,
        UpdateElementRequestModel requestModel)
    {
        IContentDraftBranch? branch = _elementDraftBranchService.GetByKey(id, branchId);
        if (branch is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementUpdate.ActionLetter, branch.ContentKey),
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

        ElementUpdateModel model = _elementEditingPresentationFactory.MapUpdateModel(requestModel);
        Attempt<ElementUpdateResult, ContentEditingOperationStatus> mapAttempt =
            await _elementEditingService.MapUpdateAsync(element, model);
        if (mapAttempt.Success is false)
        {
            return MapContentEditingFailure(mapAttempt.Status);
        }

        Attempt<ContentDraftBranchOperationStatus> saveAttempt = await _elementDraftBranchService.SaveAsync(
            id,
            branchId,
            mapAttempt.Result.Content!,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return saveAttempt.Success
            ? Ok()
            : MapFailure(saveAttempt.Result);
    }
}
