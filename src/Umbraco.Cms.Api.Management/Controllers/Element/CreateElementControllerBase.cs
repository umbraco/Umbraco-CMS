using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Serves as the base controller for element creation endpoints in the Umbraco CMS Management API.
/// Provides shared functionality for creating elements.
/// </summary>
public abstract class CreateElementControllerBase : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateElementControllerBase"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to element creation operations.</param>
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
