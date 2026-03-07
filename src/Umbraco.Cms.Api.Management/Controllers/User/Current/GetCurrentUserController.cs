using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

    /// <summary>
    /// Controller responsible for retrieving information about the currently authenticated user.
    /// </summary>
[ApiVersion("1.0")]
public class GetCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office security context.</param>
    /// <param name="authorizationService">Service used to check user permissions.</param>
    /// <param name="userService">Service for performing user management operations.</param>
    /// <param name="userPresentationFactory">Factory for creating user presentation models.</param>
    public GetCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService,
        IUserService userService,
        IUserPresentationFactory userPresentationFactory)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
    }

    /// <summary>
    /// Retrieves information and permissions for the currently authenticated back office user.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="CurrentUserResponseModel"/> with the current user's information and permissions if the user is authorized and authenticated;
    /// otherwise, returns <c>403 Forbidden</c> if the user is not authorized, or <c>401 Unauthorized</c> if the user cannot be found.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(CurrentUserResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the current user.")]
    [EndpointDescription("Gets the currently authenticated back office user's information and permissions.")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);

        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(currentUserKey),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IUser? user = await _userService.GetAsync(currentUserKey);

        if (user is null)
        {
            return Unauthorized();
        }

        var responseModel = await _userPresentationFactory.CreateCurrentUserResponseModelAsync(user);
        return Ok(responseModel);
    }
}
