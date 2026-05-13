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

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

/// <summary>
/// API controller responsible for rolling back an element to a specific version.
/// </summary>
[ApiVersion("1.0")]
public class RollbackElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollbackElementVersionController"/> class.
    /// </summary>
    /// <param name="elementVersionService">Service for managing element versions.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="authorizationService">Service used to authorize rollback operations.</param>
    public RollbackElementVersionController(
        IElementVersionService elementVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _elementVersionService = elementVersionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Rolls back an element to the version identified by the specified unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element version to roll back to.</param>
    /// <param name="culture">Optional culture to target for variant elements.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the rollback operation.</returns>
    [MapToApiVersion("1.0")]
    [HttpPost("{id:guid}/rollback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Rolls back an element to a specific version.")]
    [EndpointDescription("Rolls back an element to the version indicated by the provided Id. This will archive the current version of the element and publish the provided one.")]
    public async Task<IActionResult> Rollback(CancellationToken cancellationToken, Guid id, string? culture)
    {
        Attempt<IElement?, ContentVersionOperationStatus> getContentAttempt = await _elementVersionService.GetAsync(id);
        if (getContentAttempt.Success is false || getContentAttempt.Result is null)
        {
            return MapFailure(getContentAttempt.Status);
        }

        IElement element = getContentAttempt.Result;
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementRollback.ActionLetter, element.Key),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<ContentVersionOperationStatus> rollBackAttempt =
            await _elementVersionService.RollBackAsync(id, culture, CurrentUserKey(_backOfficeSecurityAccessor));

        return rollBackAttempt.Success
            ? Ok()
            : MapFailure(rollBackAttempt.Result);
    }
}
