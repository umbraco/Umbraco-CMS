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
/// Controller responsible for handling document move operations within the Umbraco CMS API.
/// </summary>
[ApiVersion("1.0")]
public class MoveDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.MoveDocumentController"/> class,
    /// providing services required for document move operations.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions related to document movement.</param>
    /// <param name="contentEditingService">Service responsible for editing and managing content documents.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and user information.</param>
    public MoveDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Moves the specified document to a new location within the content tree.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document to move.</param>
    /// <param name="moveDocumentRequestModel">The request model containing information about the target location.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the move operation:
    /// returns <c>200 OK</c> if the move is successful, or <c>404 Not Found</c> if the document or target location does not exist.
    /// </returns>
    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves a document.")]
    [EndpointDescription("Moves a document identified by the provided Id to a different location.")]
    public async Task<IActionResult> Move(CancellationToken cancellationToken, Guid id, MoveDocumentRequestModel moveDocumentRequestModel)
    {
        AuthorizationResult sourceAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionMove.ActionLetter, [id]),
            AuthorizationPolicies.ContentPermissionByResource);
        AuthorizationResult destinationAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionNew.ActionLetter, [moveDocumentRequestModel.Target?.Id]),
            AuthorizationPolicies.ContentPermissionByResource);

        if (sourceAuthorizationResult.Succeeded is false || destinationAuthorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<IContent?, ContentEditingOperationStatus> result = await _contentEditingService.MoveAsync(
            id,
            moveDocumentRequestModel.Target?.Id,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
