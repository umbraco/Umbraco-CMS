using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
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
/// API controller for handling copy operations on documents in Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class CopyDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

/// <summary>
/// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.CopyDocumentController"/> class, which handles document copy operations in the management API.
/// </summary>
/// <param name="authorizationService">Service used to authorize user actions.</param>
/// <param name="contentEditingService">Service for editing and managing content.</param>
/// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public CopyDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a copy of an existing document identified by the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document to copy.</param>
    /// <param name="copyDocumentRequestModel">The request model containing details about the copy operation, such as the target location and copy options.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the copy operation.</returns>
    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Copies a document.")]
    [EndpointDescription("Creates a duplicate of an existing document identified by the provided Id.")]
    public async Task<IActionResult> Copy(
        CancellationToken cancellationToken,
        Guid id,
        CopyDocumentRequestModel copyDocumentRequestModel)
    {
        AuthorizationResult sourceAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionCopy.ActionLetter, [id]),
            AuthorizationPolicies.ContentPermissionByResource);
        AuthorizationResult destinationAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionNew.ActionLetter, [copyDocumentRequestModel.Target?.Id]),
            AuthorizationPolicies.ContentPermissionByResource);

        if (sourceAuthorizationResult.Succeeded is false || destinationAuthorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentEditingService.CopyAsync(
            id,
            copyDocumentRequestModel.Target?.Id,
            copyDocumentRequestModel.RelateToOriginal,
            copyDocumentRequestModel.IncludeDescendants,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyDocumentController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : ContentEditingOperationStatusResult(result.Status);
    }
}
