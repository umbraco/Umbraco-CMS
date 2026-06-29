using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

/// <summary>
/// Controller responsible for retrieving the list of available two-factor authentication providers for a specific user.
/// </summary>
[ApiVersion("1.0")]
public class ListTwoFactorProvidersUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListTwoFactorProvidersUserController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="userTwoFactorLoginService">Service for managing user two-factor authentication providers.</param>
    public ListTwoFactorProvidersUserController(
        IAuthorizationService authorizationService,
        IUserTwoFactorLoginService userTwoFactorLoginService)
    {
        _authorizationService = authorizationService;
        _userTwoFactorLoginService = userTwoFactorLoginService;
    }

    /// <summary>
    /// Retrieves the list of available two-factor authentication providers for the specified user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the user whose two-factor providers are to be listed.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a collection of <see cref="UserTwoFactorProviderModel"/> representing the available two-factor authentication providers for the user, or a <see cref="ProblemDetails"/> result if the user is not found.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/2fa")]
    [ProducesResponseType(typeof(IEnumerable<UserTwoFactorProviderModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Lists two-factor providers for a user.")]
    [EndpointDescription("Gets a list of available two-factor authentication providers for the specified user.")]
    public async Task<IActionResult> ListTwoFactorProviders(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IEnumerable<UserTwoFactorProviderModel>, TwoFactorOperationStatus> result = await _userTwoFactorLoginService.GetProviderNamesAsync(id);

        return result.Success
            ? Ok(result.Result)
            : TwoFactorOperationStatusResult(result.Status);
    }
}
