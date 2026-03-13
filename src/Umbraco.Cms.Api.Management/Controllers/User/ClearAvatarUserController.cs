using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// API controller responsible for removing or clearing the avatar image associated with a user account.
/// </summary>
[ApiVersion("1.0")]
public class ClearAvatarUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearAvatarUserController"/> class.
    /// </summary>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> used to authorize user actions.</param>
    /// <param name="userService">The <see cref="IUserService"/> used to manage user data.</param>
    public ClearAvatarUserController(IAuthorizationService authorizationService, IUserService userService)
    {
        _authorizationService = authorizationService;
        _userService = userService;
    }

    /// <summary>
    /// Removes the avatar image for the user identified by the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the user whose avatar will be cleared.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> if the avatar was successfully cleared.</description></item>
    /// <item><description><c>404 Not Found</c> if the user does not exist.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid.</description></item>
    /// </list>
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpDelete("avatar/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Clears a user's avatar.")]
    [EndpointDescription("Removes the avatar image for the user identified by the provided Id.")]
    public async Task<IActionResult> ClearAvatar(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        UserOperationStatus result = await _userService.ClearAvatarAsync(id);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
