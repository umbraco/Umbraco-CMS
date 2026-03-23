using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Controller for deleting public access settings from documents.
/// </summary>
[ApiVersion("1.0")]
public class DeletePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPublicAccessService _publicAccessService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePublicAccessDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to document operations.</param>
    /// <param name="publicAccessService">Service used to manage public access settings for documents.</param>
    public DeletePublicAccessDocumentController(IAuthorizationService authorizationService, IPublicAccessService publicAccessService)
    {
        _authorizationService = authorizationService;
        _publicAccessService = publicAccessService;
    }

    /// <summary>
    /// Removes public access protection and rules from the specified document.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document from which to remove public access.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// returns <c>200 OK</c> if successful, or <c>404 Not Found</c> if the document or public access settings do not exist.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}/public-access")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Removes public access settings for a document.")]
    [EndpointDescription("Removes public access protection/rules for the document identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionProtect.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<PublicAccessOperationStatus> attempt = await _publicAccessService.DeleteAsync(id);

        return attempt.Success ? Ok() : PublicAccessOperationStatusResult(attempt.Result);
    }
}
