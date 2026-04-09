using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Controller responsible for creating a new user and assigning an initial password.
/// Handles the logic for user creation with password setup in the management API.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.DenyLocalLoginIfConfigured)]
public class CreateInitialPasswordUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IOpenIddictTokenManager _tokenManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInitialPasswordUserController"/> class.
    /// </summary>
    /// <param name="userService">The <see cref="IUserService"/> to manage users.</param>
    /// <param name="tokenManager">The <see cref="IOpenIddictTokenManager"/> for managing OpenIddict tokens.</param>
    public CreateInitialPasswordUserController(IUserService userService, IOpenIddictTokenManager tokenManager)
    {
        _userService = userService;
        _tokenManager = tokenManager;
    }

    /// <summary>
    /// Creates an initial password for a newly invited user using the provided token.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="model">A <see cref="CreateInitialPasswordUserRequestModel"/> containing the user ID, invitation token, and the new password.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns <c>200 OK</c> if successful, or an error response if the operation fails.</returns>
    [AllowAnonymous]
    [HttpPost("invite/create-password")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Creates an initial password for a user.")]
    [EndpointDescription("Creates an initial password for a newly invited user using the provided token.")]
    public async Task<IActionResult> CreateInitialPassword(
        CancellationToken cancellationToken,
        CreateInitialPasswordUserRequestModel model)
    {
        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.CreateInitialPasswordAsync(model.User.Id, model.Token, model.Password);

        if (response.Success is false)
        {
            return UserOperationStatusResult(response.Status, response.Result);
        }

        await _tokenManager.RevokeUmbracoUserTokens(model.User.Id);
        return Ok();
    }
}
