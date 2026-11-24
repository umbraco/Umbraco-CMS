using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.DocumentDraftBranch;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
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

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides an API endpoint for saving edits to a document draft branch's own content.
/// </summary>
[ApiVersion("1.0")]
public class UpdateDocumentDraftBranchController : DocumentDraftBranchControllerBase
{
    private readonly IContentDraftBranchService _contentDraftBranchService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDocumentDraftBranchController"/> class.
    /// </summary>
    public UpdateDocumentDraftBranchController(
        IContentDraftBranchService contentDraftBranchService,
        IContentEditingService contentEditingService,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory,
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentDraftBranchService = contentDraftBranchService;
        _contentEditingService = contentEditingService;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Saves edits to a document draft branch's own content. This is a plain save only - it never publishes.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document the draft branch belongs to.</param>
    /// <param name="branchId">The unique identifier of the draft branch to update.</param>
    /// <param name="requestModel">The model containing the updated document details.</param>
    [HttpPut("{branchId:guid}/content")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a document draft branch.")]
    [EndpointDescription("Saves edits to a document draft branch identified by the provided Id. This is a plain save only - it never publishes.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        Guid branchId,
        UpdateDocumentRequestModel requestModel)
    {
        IContentDraftBranch? branch = _contentDraftBranchService.GetByKey(id, branchId);
        if (branch is null)
        {
            return MapFailure(ContentDraftBranchOperationStatus.BranchNotFound);
        }

        // We intentionally don't pass in cultures here, matching UpdateDocumentControllerBase: this is to
        // support the client sending values for all cultures even if the user doesn't have access to the
        // language. Values for unauthorized languages are later ignored in the ContentEditingService.
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionUpdate.ActionLetter, branch.ContentKey),
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

        ContentUpdateModel model = _documentEditingPresentationFactory.MapUpdateModel(requestModel);
        Attempt<ContentUpdateResult, ContentEditingOperationStatus> mapAttempt =
            await _contentEditingService.MapUpdateAsync(content, model);
        if (mapAttempt.Success is false)
        {
            return MapContentEditingFailure(mapAttempt.Status);
        }

        Attempt<ContentDraftBranchOperationStatus> saveAttempt = await _contentDraftBranchService.SaveAsync(
            id,
            branchId,
            mapAttempt.Result.Content!,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return saveAttempt.Success
            ? Ok()
            : MapFailure(saveAttempt.Result);
    }
}
