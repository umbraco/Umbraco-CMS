using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Controller for disabling two-factor authentication providers for the current user.
/// </summary>
[ApiVersion("1.0")]
public class DisableTwoFactorProviderCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    public DisableTwoFactorProviderCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserTwoFactorLoginService userTwoFactorLoginService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userTwoFactorLoginService = userTwoFactorLoginService;
    }

    /// <summary>
    /// Disables a specified two-factor authentication provider for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="providerName">The name of the two-factor authentication provider to disable (e.g., "AuthenticatorApp").</param>
    /// <param name="code">The verification code required to confirm disabling the provider.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// returns <c>200 OK</c> if successful, <c>404 Not Found</c> if the provider is not found, or <c>400 Bad Request</c> if the code is invalid or the operation fails.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpDelete("2fa/{providerName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Disables two-factor authentication for the current user.")]
    [EndpointDescription("Disables the specified two-factor authentication provider for the currently authenticated user.")]
    public async Task<IActionResult> DisableTwoFactorProvider(
        CancellationToken cancellationToken,
        string providerName,
        string code)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        Attempt<TwoFactorOperationStatus> result = await _userTwoFactorLoginService.DisableByCodeAsync(providerName, userKey, code);

        return result.Success
            ? Ok()
            : TwoFactorOperationStatusResult(result.Result);
    }
}
