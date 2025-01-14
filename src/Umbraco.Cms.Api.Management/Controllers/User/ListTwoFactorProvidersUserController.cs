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

[ApiVersion("1.0")]
public class ListTwoFactorProvidersUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserTwoFactorLoginService _userTwoFactorLoginService;

    public ListTwoFactorProvidersUserController(
        IAuthorizationService authorizationService,
        IUserTwoFactorLoginService userTwoFactorLoginService)
    {
        _authorizationService = authorizationService;
        _userTwoFactorLoginService = userTwoFactorLoginService;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/2fa")]
    [ProducesResponseType(typeof(IEnumerable<UserTwoFactorProviderModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
