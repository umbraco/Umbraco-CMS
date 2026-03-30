using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Controller responsible for handling requests to update or set a user's avatar.
/// </summary>
[ApiVersion("1.0")]
public class SetAvatarUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.User.SetAvatarUserController"/> class, which manages user avatar operations.
    /// </summary>
    /// <param name="userService">Service used for user management operations.</param>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    public SetAvatarUserController(IUserService userService, IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _userService = userService;
    }

    /// <summary>
    /// Sets or updates the avatar image for a specified user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the user whose avatar is to be set.</param>
    /// <param name="model">The request model containing the avatar file information.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [MapToApiVersion("1.0")]
    [HttpPost("avatar/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Sets a user's avatar.")]
    [EndpointDescription("Sets or updates the avatar image for the user identified by the provided Id.")]
    public async Task<IActionResult> SetAvatar(CancellationToken cancellationToken, Guid id, SetAvatarRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        UserOperationStatus result = await _userService.SetAvatarAsync(id, model.File.Id);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
