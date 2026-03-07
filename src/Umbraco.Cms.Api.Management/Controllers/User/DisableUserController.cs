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

namespace Umbraco.Cms.Api.Management.Controllers.User;

    /// <summary>
    /// Controller responsible for handling operations that disable user accounts.
    /// </summary>
[ApiVersion("1.0")]
public class DisableUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisableUserController"/> class, which handles user disable operations in the management API.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="userService">Service for managing user data and operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public DisableUserController(
        IAuthorizationService authorizationService,
        IUserService userService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Disables one or more user accounts specified by their unique identifiers.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The request model containing a collection of user Ids to disable.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that returns:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> if the users were successfully disabled.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid.</description></item>
    /// <item><description><c>404 Not Found</c> if any of the specified users do not exist.</description></item>
    /// </list>
    /// </returns>
    [HttpPost("disable")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Disables users.")]
    [EndpointDescription("Disables the user accounts identified by the provided Ids.")]
    public async Task<IActionResult> DisableUsers(CancellationToken cancellationToken, DisableUserRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(model.UserIds.Select(x => x.Id)),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        UserOperationStatus result = await _userService.DisableAsync(CurrentUserKey(_backOfficeSecurityAccessor), model.UserIds.Select(x => x.Id).ToHashSet());

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
