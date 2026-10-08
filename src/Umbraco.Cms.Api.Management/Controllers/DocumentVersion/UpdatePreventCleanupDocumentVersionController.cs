using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

/// <summary>
/// Controller for updating the 'prevent cleanup' status of a document version.
/// </summary>
[ApiVersion("1.0")]
public class UpdatePreventCleanupDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePreventCleanupDocumentVersionController"/> class,
    /// which manages requests to update the prevent cleanup status of document versions.
    /// </summary>
    /// <param name="contentVersionService">Service used to manage content versions.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="authorizationService">Service for handling authorization checks for the current user.</param>
    [ActivatorUtilitiesConstructor]
    public UpdatePreventCleanupDocumentVersionController(
        IContentVersionService contentVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _contentVersionService = contentVersionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePreventCleanupDocumentVersionController"/> class,
    /// which manages requests to update the prevent cleanup status of document versions.
    /// </summary>
    /// <param name="contentVersionService">Service used to manage content versions.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public UpdatePreventCleanupDocumentVersionController(
        IContentVersionService contentVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            contentVersionService,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Sets the prevent cleanup status for a specific document version.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document version to update.</param>
    /// <param name="preventCleanup">If <c>true</c>, marks the document version to be excluded from content history cleanup; otherwise, allows it to be cleaned up.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns 200 OK on success, or a problem response if the document version is not found or the request is invalid.</returns>
    [MapToApiVersion("1.0")]
    [HttpPut("{id:guid}/prevent-cleanup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Sets the prevent clean up status for a document version.")]
    [EndpointDescription("Sets the prevent clean up boolean status for a document version to the provided value. This controls whether the version will be a candidate for removal in content history clean up.")]
    public async Task<IActionResult> Set(CancellationToken cancellationToken, Guid id, bool preventCleanup)
    {
        Attempt<IContent?, ContentVersionOperationStatus> getContentAttempt =
            await _contentVersionService.GetAsync(id, cancellationToken);
        if (getContentAttempt.Success is false || getContentAttempt.Result is null)
        {
            return MapFailure(getContentAttempt.Status);
        }

        IContent content = getContentAttempt.Result;
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionRollback.ActionLetter, content.Key),
            AuthorizationPolicies.ContentPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<ContentVersionOperationStatus> attempt =
            await _contentVersionService.SetPreventCleanupAsync(id, preventCleanup, CurrentUserKey(_backOfficeSecurityAccessor));

        return attempt.Success
            ? Ok()
            : MapFailure(attempt.Result);
    }
}
