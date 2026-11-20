using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Controller for handling password change requests for the current user.
/// </summary>
[ApiVersion("1.0")]
public class ChangePasswordCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accesses the back office security context.</param>
    /// <param name="userService">Performs user management operations.</param>
    /// <param name="backOfficeUserManager">Manages back office identity users.</param>
    /// <param name="backOfficeSignInManager">Manages the back office sign-in.</param>
    [ActivatorUtilitiesConstructor]
    public ChangePasswordCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IBackOfficeUserManager backOfficeUserManager,
        IBackOfficeSignInManager backOfficeSignInManager)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
        _backOfficeUserManager = backOfficeUserManager;
        _backOfficeSignInManager = backOfficeSignInManager;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accesses the back office security context.</param>
    /// <param name="userService">Performs user management operations.</param>
    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 21.")]
    public ChangePasswordCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService)
        : this(
            backOfficeSecurityAccessor,
            userService,
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficeUserManager>(),
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficeSignInManager>())
    {
    }

    /// <summary>
    /// Changes the password for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="model">A <see cref="ChangePasswordCurrentUserRequestModel"/> containing the old and new password values.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the password change operation:
    /// returns <c>200 OK</c> if the password was changed successfully, or <c>400 Bad Request</c> with details if the operation failed.
    /// </returns>
    [HttpPost("change-password")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Changes the current user's password.")]
    [EndpointDescription("Changes the password for the currently authenticated user.")]
    public async Task<IActionResult> ChangePassword(
        CancellationToken cancellationToken,
        ChangePasswordCurrentUserRequestModel model)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        var changeModel = new ChangeUserPasswordModel
        {
            NewPassword = model.NewPassword,
            OldPassword = model.OldPassword,
            UserKey = userKey,
        };

        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.ChangePasswordAsync(userKey, changeModel);

        if (response.Success is false)
        {
            return UserOperationStatusResult(response.Status, response.Result);
        }

        // The password change renews the security stamp, which ends the session of the cookie that made the request.
        // Re-issue it, so the user stays signed in on this device. Other sessions of the user remain ended.
        AuthenticateResult cookieAuthentication = await HttpContext.AuthenticateAsync(Constants.Security.BackOfficeAuthenticationType);
        if (cookieAuthentication.Succeeded)
        {
            BackOfficeIdentityUser? identityUser = await _backOfficeUserManager.GetUserAsync(cookieAuthentication.Principal!);
            if (identityUser is not null)
            {
                await _backOfficeSignInManager.RefreshSignInAsync(identityUser);
            }
        }

        return Ok();
    }
}
