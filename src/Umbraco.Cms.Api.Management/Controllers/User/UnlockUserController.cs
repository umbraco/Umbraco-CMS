using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// API controller responsible for handling requests to unlock user accounts in the system.
/// </summary>
[ApiVersion("1.0")]
public class UnlockUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.User.UnlockUserController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions within the controller.</param>
    /// <param name="userService">Service for managing and retrieving user information.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and operations.</param>
    public UnlockUserController(
        IAuthorizationService authorizationService,
        IUserService userService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost("unlock")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Unlocks users.")]
    [EndpointDescription("Unlocks the user accounts identified by the provided Ids.")]
    public async Task<IActionResult> UnlockUsers(CancellationToken cancellationToken, UnlockUsersRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(model.UserIds.Select(x => x.Id)),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<UserUnlockResult, UserOperationStatus> attempt = await _userService.UnlockAsync(CurrentUserKey(_backOfficeSecurityAccessor), model.UserIds.Select(x => x.Id).ToArray());

        return attempt.Success
            ? Ok()
            : UserOperationStatusResult(attempt.Status, attempt.Result);
    }
}
