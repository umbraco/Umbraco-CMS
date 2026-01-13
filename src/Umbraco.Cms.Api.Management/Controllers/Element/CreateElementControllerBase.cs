using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

public abstract class CreateElementControllerBase : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    protected CreateElementControllerBase(IAuthorizationService authorizationService)
        => _authorizationService = authorizationService;

    protected async Task<IActionResult> HandleRequest(CreateElementRequestModel requestModel, Func<Task<IActionResult>> authorizedHandler)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementNew.ActionLetter, requestModel.Parent?.Id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        return await authorizedHandler();
    }
}
