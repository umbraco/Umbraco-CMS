using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Controller responsible for handling requests to disable two-factor authentication providers for a specified user.
/// </summary>
[ApiVersion("1.0")]
public class DisableTwoFactorProviderUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisableTwoFactorProviderUserController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions within the controller.</param>
    /// <param name="userTwoFactorLoginService">Service responsible for managing user two-factor authentication providers.</param>
    public DisableTwoFactorProviderUserController(
        IAuthorizationService authorizationService,
        IUserTwoFactorLoginService userTwoFactorLoginService)
    {
        _authorizationService = authorizationService;
        _userTwoFactorLoginService = userTwoFactorLoginService;
    }

    /// <summary>
    /// Disables the specified two-factor authentication provider for a user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="providerName">The name of the two-factor authentication provider to disable.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}/2fa/{providerName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Disables two-factor authentication for a user.")]
    [EndpointDescription("Disables the specified two-factor authentication provider for a user.")]
    public async Task<IActionResult> DisableTwoFactorProvider(
        CancellationToken cancellationToken,
        Guid id,
        string providerName)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<TwoFactorOperationStatus> result = await _userTwoFactorLoginService.DisableAsync(id,providerName);

        return result.Success
            ? Ok()
            : TwoFactorOperationStatusResult(result.Result);
    }
}
