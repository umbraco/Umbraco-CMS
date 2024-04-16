using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class DisableTwoFactorProviderUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    public DisableTwoFactorProviderUserController(
        IAuthorizationService authorizationService,
        IUserTwoFactorLoginService userTwoFactorLoginService)
    {
        _authorizationService = authorizationService;
        _userTwoFactorLoginService = userTwoFactorLoginService;
    }

    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}/2fa/{providerName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableTwoFactorProvider(
        CancellationToken cancellationToken,
        Guid id,
        string providerName)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<TwoFactorOperationStatus> result = await _userTwoFactorLoginService.DisableAsync(id,providerName);

        return result.Success
            ? Ok()
            : TwoFactorOperationStatusResult(result.Result);
    }
}
