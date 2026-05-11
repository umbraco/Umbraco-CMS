using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Controller responsible for handling requests to set or update the avatar of the currently authenticated user.
/// </summary>
[ApiVersion("1.0")]
public class SetAvatarCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetAvatarCurrentUserController"/> class, which manages the current user's avatar operations.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for the current user.</param>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="userService">Service for managing user-related operations.</param>
    public SetAvatarCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService,
        IUserService userService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
        _userService = userService;
    }

    /// <summary>
    /// Sets or updates the avatar image for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <param name="model">The request model containing the avatar file information.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [MapToApiVersion("1.0")]
    [HttpPost("avatar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Sets the current user's avatar.")]
    [EndpointDescription("Sets or updates the avatar image for the currently authenticated user.")]
    public async Task<IActionResult> SetAvatar(CancellationToken cancellationToken, SetAvatarRequestModel model)
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

        UserOperationStatus result = await _userService.SetAvatarAsync(userKey, model.File.Id);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
