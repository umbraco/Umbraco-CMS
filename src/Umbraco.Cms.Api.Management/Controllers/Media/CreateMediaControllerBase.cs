using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Media;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

public class CreateMediaControllerBase : MediaControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    public CreateMediaControllerBase(IAuthorizationService authorizationService)
        => _authorizationService = authorizationService;

    protected async Task<IActionResult> HandleRequest(Guid? parentId, Func<Task<IActionResult>> authorizedHandler)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.WithKeys(parentId),
            AuthorizationPolicies.MediaPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        return await authorizedHandler();
    }
}
