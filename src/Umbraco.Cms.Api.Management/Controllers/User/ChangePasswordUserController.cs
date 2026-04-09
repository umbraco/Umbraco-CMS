using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Controller responsible for managing operations related to changing a user's password.
/// </summary>
[ApiVersion("1.0")]
public class ChangePasswordUserController : UserControllerBase
{
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangePasswordUserController"/> class.
    /// </summary>
    /// <param name="userService">Service used for user management operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and operations.</param>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    public ChangePasswordUserController(
        IUserService userService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Changes the password for the user identified by the specified ID.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the user whose password will be changed.</param>
    /// <param name="model">The request model containing the new password.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// returns 200 OK if the password was changed successfully, 400 Bad Request if the request is invalid, or 404 Not Found if the user does not exist.
    /// </returns>
    [HttpPost("{id:guid}/change-password")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Changes a user's password.")]
    [EndpointDescription("Changes the password for the user identified by the provided Id.")]
    public async Task<IActionResult> ChangePassword(
        CancellationToken cancellationToken,
        Guid id,
        ChangePasswordUserRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        var passwordModel = new ChangeUserPasswordModel
        {
            NewPassword = model.NewPassword,
            UserKey = id,
        };

        Attempt<PasswordChangedModel, UserOperationStatus> response = await _userService.ChangePasswordAsync(CurrentUserKey(_backOfficeSecurityAccessor), passwordModel);

        return response.Success
            ? Ok()
            : UserOperationStatusResult(response.Status, response.Result);
    }
}
