using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Security;

    /// <summary>
    /// Provides API endpoints for verifying reset password tokens as part of the security workflow.
    /// </summary>
[ApiVersion("1.0")]
public class VerifyResetPasswordTokenController : SecurityControllerBase
{
    private readonly IUserService _userService;
    private readonly IPasswordConfigurationPresentationFactory _passwordConfigurationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyResetPasswordTokenController"/> class.
    /// </summary>
    /// <param name="userService">Service used for user management operations.</param>
    /// <param name="passwordConfigurationPresentationFactory">Factory for creating password configuration presentation models.</param>
    public VerifyResetPasswordTokenController(IUserService userService, IPasswordConfigurationPresentationFactory passwordConfigurationPresentationFactory)
    {
        _userService = userService;
        _passwordConfigurationPresentationFactory = passwordConfigurationPresentationFactory;
    }

    /// <summary>
    /// Verifies whether the provided password reset token is valid for the specified user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The request model containing the user identifier and the reset token to verify.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="VerifyResetPasswordResponseModel"/> if verification succeeds, or an error response if it fails.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("forgot-password/verify")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(VerifyResetPasswordResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetailsBuilder), StatusCodes.Status404NotFound)]
    [EndpointSummary("Verifies a password reset token.")]
    [EndpointDescription("Verifies the provided password reset token for the specified user.")]
    [UserPasswordEnsureMinimumResponseTime]
    public async Task<IActionResult> VerifyResetPasswordToken(
        CancellationToken cancellationToken,
        VerifyResetPasswordTokenRequestModel model)
    {
        Attempt<UserOperationStatus> result = await _userService.VerifyPasswordResetAsync(model.User.Id, model.ResetCode);

        return result.Success
            ? Ok(new VerifyResetPasswordResponseModel()
            {
                PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),
            })
            : UserOperationStatusResult(result.Result);
    }
}
