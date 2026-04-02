using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

/// <summary>
/// Controller responsible for retrieving the list of available two-factor authentication providers for the current user.
/// </summary>
[ApiVersion("1.0")]
public class ListTwoFactorProvidersCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListTwoFactorProvidersCurrentUserController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for the current back office security context.</param>
    /// <param name="userUserTwoFactorLoginService">Service used to manage two-factor authentication providers for users.</param>
    public ListTwoFactorProvidersCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserTwoFactorLoginService userUserTwoFactorLoginService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userTwoFactorLoginService = userUserTwoFactorLoginService;
    }

    /// <summary>
    /// Retrieves the available two-factor authentication providers for the current user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> containing a list of <see cref="UserTwoFactorProviderModel"/> representing the available two-factor providers.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("2fa")]
    [ProducesResponseType(typeof(IEnumerable<UserTwoFactorProviderModel>),StatusCodes.Status200OK)]
    [EndpointSummary("Lists two-factor providers for the current user.")]
    [EndpointDescription("Gets a list of available two-factor authentication providers for the current user.")]
    public async Task<IActionResult> ListTwoFactorProvidersForCurrentUser(CancellationToken cancellationToken)
    {
        Guid userKey = CurrentUserKey(_backOfficeSecurityAccessor);

        Attempt<IEnumerable<UserTwoFactorProviderModel>, TwoFactorOperationStatus> result = await _userTwoFactorLoginService.GetProviderNamesAsync(userKey);

        return result.Success
            ? Ok(result.Result)
            : TwoFactorOperationStatusResult(result.Status);
    }
}
