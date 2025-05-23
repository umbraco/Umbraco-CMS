using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

public abstract class UpdateMediaControllerBase : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    protected UpdateMediaControllerBase(IAuthorizationService authorizationService)
        => _authorizationService = authorizationService;

    protected async Task<IActionResult> HandleRequest(Guid id, Func<Task<IActionResult>> authorizedHandler)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.WithKeys(id),
            AuthorizationPolicies.MediaPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        return await authorizedHandler();
    }
}
