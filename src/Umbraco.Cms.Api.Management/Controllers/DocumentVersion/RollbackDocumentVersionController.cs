using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

/// <summary>
/// Provides API endpoints for rolling back document versions in the Umbraco CMS.
/// Handles requests to revert documents to previous versions.
/// </summary>
[ApiVersion("1.0")]
public class RollbackDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollbackDocumentVersionController"/> class.
    /// </summary>
    /// <param name="contentVersionService">Service for managing content versions and performing rollback operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context, used for authentication and authorization.</param>
    /// <param name="authorizationService">Service for handling authorization checks for the current user.</param>
    public RollbackDocumentVersionController(
        IContentVersionService contentVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _contentVersionService = contentVersionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Rolls back a document to the specified version by its unique identifier. The current version of the document will be archived, and the selected version will be restored. Optionally, a specific culture can be rolled back if provided.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document version to roll back to.</param>
    /// <param name="culture">An optional culture identifier. If specified, only the content for the given culture will be rolled back; otherwise, the entire document version is restored.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the rollback operation. Returns 200 OK if successful, 404 Not Found if the document or version does not exist, or 400 Bad Request for invalid requests.</returns>
    [MapToApiVersion("1.0")]
    [HttpPost("{id:guid}/rollback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Rolls back a document to a specific version.")]
    [EndpointDescription("Rolls back a document to the version indicated by the provided Id. This will archive the current version of the document and publish the provided one.")]
    public async Task<IActionResult> Rollback(CancellationToken cancellationToken, Guid id, string? culture)
    {
        Attempt<IContent?, ContentVersionOperationStatus> getContentAttempt =
            await _contentVersionService.GetAsync(id);
        if (getContentAttempt.Success is false || getContentAttempt.Result is null)
        {
            return MapFailure(getContentAttempt.Status);
        }

        IContent content = getContentAttempt.Result;
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionRollback.ActionLetter, content.Key),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentVersionOperationStatus> rollBackAttempt =
            await _contentVersionService.RollBackAsync(id, culture, CurrentUserKey(_backOfficeSecurityAccessor));

        return rollBackAttempt.Success
            ? Ok()
            : MapFailure(rollBackAttempt.Result);
    }
}
