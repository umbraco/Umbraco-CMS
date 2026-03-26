using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Controller responsible for handling requests to clear the avatar of the currently authenticated user.
/// </summary>
[ApiVersion("1.0")]
public class ClearAvatarCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearAvatarCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for the current user.</param>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="userService">Service for managing user-related operations.</param>
    public ClearAvatarCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService,
        IUserService userService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
        _userService = userService;
    }

    /// <summary>
    /// Removes the avatar image for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [MapToApiVersion("1.0")]
    [HttpDelete("avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Clears the current user's avatar.")]
    [EndpointDescription("Removes the avatar image for the currently authenticated user.")]
    public async Task<IActionResult> ClearAvatar(CancellationToken cancellationToken)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(userKey),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        UserOperationStatus result = await _userService.ClearAvatarAsync(userKey);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
