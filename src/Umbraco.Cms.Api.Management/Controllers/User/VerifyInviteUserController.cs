using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Controller responsible for verifying the invitations of users in the system.
/// </summary>
[ApiVersion("1.0")]
public class VerifyInviteUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IPasswordConfigurationPresentationFactory _passwordConfigurationPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="VerifyInviteUserController"/> class, which handles verification of user invitations.
    /// </summary>
    /// <param name="userService">Service used for user management operations.</param>
    /// <param name="passwordConfigurationPresentationFactory">Factory for creating password configuration presentations.</param>
    public VerifyInviteUserController(IUserService userService, IPasswordConfigurationPresentationFactory passwordConfigurationPresentationFactory)
    {
        _userService = userService;
        _passwordConfigurationPresentationFactory = passwordConfigurationPresentationFactory;
    }

    /// <summary>
    /// Verifies whether the provided invitation token is valid for creating a new user account.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The request model containing the user identifier and invitation token to verify.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="VerifyInviteUserResponseModel"/> with password configuration details if the token is valid;
    /// otherwise, a <see cref="ProblemDetails"/> result indicating the error (e.g., not found or bad request).
    /// </returns>
    [AllowAnonymous]
    [HttpPost("invite/verify")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(VerifyInviteUserResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Verifies a user invitation.")]
    [EndpointDescription("Verifies that the invitation token is valid for creating a new user account.")]
    public async Task<IActionResult> Invite(CancellationToken cancellationToken, VerifyInviteUserRequestModel model)
    {
        Attempt<UserOperationStatus> result = await _userService.VerifyInviteAsync(model.User.Id, model.Token);

        return result.Success
            ? Ok(new VerifyInviteUserResponseModel()
            {
                PasswordConfiguration = _passwordConfigurationPresentationFactory.CreatePasswordConfigurationResponseModel(),
            })
            : UserOperationStatusResult(result.Result);
    }
}
