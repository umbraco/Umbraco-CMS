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
/// API controller responsible for handling requests to enable user accounts in Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class EnableUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnableUserController"/> class, which handles enabling users in the Umbraco backoffice.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="userService">Service for managing user-related operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for backoffice security context.</param>
    public EnableUserController(
        IAuthorizationService authorizationService,
        IUserService userService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _userService = userService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Enables one or more user accounts specified by their unique identifiers.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="model">The request model containing a collection of user Ids to enable.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that returns <c>200 OK</c> if the users were enabled successfully, or a <see cref="ProblemDetails"/> result with appropriate status code if the operation failed (e.g., <c>400 Bad Request</c> or <c>404 Not Found</c>).
    /// </returns>
    [HttpPost("enable")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Enables users.")]
    [EndpointDescription("Enables the user accounts identified by the provided Ids.")]
    public async Task<IActionResult> EnableUsers(CancellationToken cancellationToken, EnableUserRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(model.UserIds.Select(x => x.Id)),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        UserOperationStatus result = await _userService.EnableAsync(CurrentUserKey(_backOfficeSecurityAccessor), model.UserIds.Select(x => x.Id).ToHashSet());

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
