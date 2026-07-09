using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

/// <summary>
/// Controller responsible for managing operations related to reset password tokens.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
public class ResetPasswordTokenController : SecurityControllerBase
{
    private readonly IUserService _userService;
    private readonly IOpenIddictTokenManager _tokenManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResetPasswordTokenController"/> class, which manages reset password token operations.
    /// </summary>
    /// <param name="userService">The <see cref="IUserService"/> used for user management operations.</param>
    /// <param name="tokenManager">The <see cref="IOpenIddictTokenManager"/> used for managing OpenIddict tokens.</param>
    public ResetPasswordTokenController(IUserService userService, IOpenIddictTokenManager tokenManager)
    {
        _userService = userService;
        _tokenManager = tokenManager;
    }

    /// <summary>
    /// Resets the password for a user using a reset code and new password.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The request model containing the user information, reset code, and new password.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the operation:
    /// returns 204 No Content on success, 400 Bad Request for invalid input, or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpPost("forgot-password/reset")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status404NotFound)]
    [EndpointSummary("Initiates password reset.")]
    [EndpointDescription("Initiates a password reset process for the user with the provided email.")]
    [UserPasswordEnsureMinimumResponseTime]
    public async Task<IActionResult> ResetPasswordToken(CancellationToken cancellationToken, ResetPasswordTokenRequestModel model)
    {
        Attempt<PasswordChangedModel, UserOperationStatus> result = await _userService.ResetPasswordAsync(model.User.Id, model.ResetCode, model.Password);

        if (result.Success is false)
        {
            return UserOperationStatusResult(result.Status, result.Result);
        }

        await _tokenManager.RevokeUmbracoUserTokens(model.User.Id);
        return Ok();

    }
}
