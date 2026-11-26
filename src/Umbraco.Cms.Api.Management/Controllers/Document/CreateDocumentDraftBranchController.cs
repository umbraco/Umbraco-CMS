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
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides an API endpoint for creating a new named draft branch of a document.
/// </summary>
[ApiVersion("1.0")]
public class CreateDocumentDraftBranchController : DocumentDraftBranchControllerBase
{
    private readonly IContentDraftBranchService _contentDraftBranchService;
    private readonly IDocumentDraftBranchPresentationFactory _documentDraftBranchPresentationFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDocumentDraftBranchController"/> class.
    /// </summary>
    public CreateDocumentDraftBranchController(
        IContentDraftBranchService contentDraftBranchService,
        IDocumentDraftBranchPresentationFactory documentDraftBranchPresentationFactory,
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentDraftBranchService = contentDraftBranchService;
        _documentDraftBranchPresentationFactory = documentDraftBranchPresentationFactory;
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a new named draft branch of a document, forked from its current default draft.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document to fork the new draft branch from.</param>
    /// <param name="requestModel">The name of the new draft branch.</param>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new document draft branch.")]
    [EndpointDescription("Creates a new named draft branch for a document, forked from its current default draft (which may already carry unpublished edits) rather than from its published version.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        Guid id,
        CreateDocumentDraftBranchRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionUpdate.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IContentDraftBranch?, ContentDraftBranchOperationStatus> result = await _contentDraftBranchService.CreateAsync(
            id,
            requestModel.Name,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByKeyDocumentDraftBranchController>(
                controller => nameof(controller.ByKey),
                new { id, branchId = result.Result!.Key },
                result.Result!.Key.ToString())
            : MapFailure(result.Status);
    }
}
