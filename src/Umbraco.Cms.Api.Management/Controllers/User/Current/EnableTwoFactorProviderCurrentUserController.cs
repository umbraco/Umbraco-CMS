using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

    /// <summary>
    /// Controller for enabling two-factor authentication providers for the current user.
    /// </summary>
[ApiVersion("1.0")]
public class EnableTwoFactorProviderCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnableTwoFactorProviderCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to the back office security context for the current user.</param>
    /// <param name="userTwoFactorLoginService">Service for managing two-factor authentication providers for users.</param>
    public EnableTwoFactorProviderCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserTwoFactorLoginService userTwoFactorLoginService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userTwoFactorLoginService = userTwoFactorLoginService;
    }

    /// <summary>
    /// Enables a specified two-factor authentication provider for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="providerName">The name of the two-factor authentication provider to enable (e.g., "authenticator").</param>
    /// <param name="model">The request model containing the secret and verification code required to enable two-factor authentication.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// returns <c>200 OK</c> if successful, <c>400 Bad Request</c> if validation fails, or <c>404 Not Found</c> if the provider does not exist.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpPost("2fa/{providerName}")]
    [ProducesResponseType(typeof(ISetupTwoFactorModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Enables two-factor authentication for the current user.")]
    [EndpointDescription("Enables the specified two-factor authentication provider for the currently authenticated user.")]
    public async Task<IActionResult> EnableTwoFactorProvider(
        CancellationToken cancellationToken,
        string providerName,
        EnableTwoFactorRequestModel model)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        Attempt<TwoFactorOperationStatus> result = await _userTwoFactorLoginService.ValidateAndSaveAsync(providerName, userKey, model.Secret, model.Code);

        return result.Success
            ? Ok()
            : TwoFactorOperationStatusResult(result.Result);
    }
}

