using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

/// <summary>
/// Provides API endpoints for handling user password reset operations in the security management context.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
public class ResetPasswordController : SecurityControllerBase
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordController"/> class, which provides endpoints for resetting user passwords.
    /// </summary>
    /// <param name="userService">The service used to manage user accounts.</param>
    public ResetPasswordController(IUserService userService) => _userService = userService;


    /// <summary>
    /// Initiates the password reset process by attempting to send a reset link to the specified email address.
    /// For security reasons, this endpoint always returns a generic result and does not reveal whether the email exists in the system.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The request model containing the email address for which to request a password reset.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the password reset request. Returns <c>Ok</c> if the request was processed, or <c>BadRequest</c> if password reset is not allowed.
    /// </returns>
    [HttpPost("forgot-password")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Requests a password reset.")]
    [EndpointDescription("Initiates a password reset process by sending a reset link to the specified email address.")]
    [UserPasswordEnsureMinimumResponseTime]
    public async Task<IActionResult> RequestPasswordReset(CancellationToken cancellationToken, ResetPasswordRequestModel model)
    {
        Attempt<UserOperationStatus> result = await _userService.SendResetPasswordEmailAsync(model.Email);

        // If this feature is switched off in configuration, the UI will be amended to not make the request to reset password available.
        // So this is just a server-side secondary check.
        // Regardless of other status values, it will just return Ok, so you can't use this endpoint to determine whether the email exists in the system.
        return result.Result == UserOperationStatus.CannotPasswordReset
            ? BadRequest()
            : Ok();
    }
}
