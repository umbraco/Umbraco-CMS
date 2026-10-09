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

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

/// <summary>
/// API controller responsible for updating the prevent-cleanup status of an element version.
/// </summary>
[ApiVersion("1.0")]
public class UpdatePreventCleanupElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePreventCleanupElementVersionController"/> class.
    /// </summary>
    /// <param name="elementVersionService">Service for managing element versions.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="authorizationService">Service for handling authorization checks for the current user.</param>
    [ActivatorUtilitiesConstructor]
    public UpdatePreventCleanupElementVersionController(
        IElementVersionService elementVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _elementVersionService = elementVersionService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePreventCleanupElementVersionController"/> class.
    /// </summary>
    /// <param name="elementVersionService">Service for managing element versions.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 20.")]
    public UpdatePreventCleanupElementVersionController(
        IElementVersionService elementVersionService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            elementVersionService,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Sets the prevent-cleanup status for an element version.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element version.</param>
    /// <param name="preventCleanup">Whether the version should be excluded from content history cleanup.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the operation.</returns>
    [MapToApiVersion("1.0")]
    [HttpPut("{id:guid}/prevent-cleanup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Sets the prevent clean up status for an element version.")]
    [EndpointDescription("Sets the prevent clean up boolean status for an element version to the provided value. This controls whether the version will be a candidate for removal in content history clean up.")]
    public async Task<IActionResult> Set(CancellationToken cancellationToken, Guid id, bool preventCleanup)
    {
        Attempt<IElement?, ContentVersionOperationStatus> getElementAttempt = await _elementVersionService.GetAsync(id);
        if (getElementAttempt.Success is false || getElementAttempt.Result is null)
        {
            return MapFailure(getElementAttempt.Status);
        }

        IElement element = getElementAttempt.Result;
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementRollback.ActionLetter, element.Key),
            AuthorizationPolicies.ElementPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<ContentVersionOperationStatus> attempt =
            await _elementVersionService.SetPreventCleanupAsync(id, preventCleanup, CurrentUserKey(_backOfficeSecurityAccessor));

        return attempt.Success
            ? Ok()
            : MapFailure(attempt.Result);
    }
}
