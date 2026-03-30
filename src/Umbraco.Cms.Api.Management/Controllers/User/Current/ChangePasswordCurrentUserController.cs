using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Controller for handling password change requests for the current user.
/// </summary>
[ApiVersion("1.0")]
public class ChangePasswordCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accesses the back office security context.</param>
    /// <param name="userService">Performs user management operations.</param>
    public ChangePasswordCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
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

        return response.Success
            ? Ok()
            : UserOperationStatusResult(response.Status, response.Result);
    }
}
