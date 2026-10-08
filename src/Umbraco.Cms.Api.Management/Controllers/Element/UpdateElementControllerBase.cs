using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Serves as the base controller for handling update operations on elements within the Umbraco CMS Management API.
/// Provides common functionality for derived controllers that manage element updates.
/// </summary>
public abstract class UpdateElementControllerBase : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateElementControllerBase"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to element update operations.</param>
    protected UpdateElementControllerBase(IAuthorizationService authorizationService)
        => _authorizationService = authorizationService;

    protected async Task<IActionResult> HandleRequest(Guid id, UpdateElementRequestModel requestModel, Func<Task<IActionResult>> authorizedHandler)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementUpdate.ActionLetter, id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        return await authorizedHandler();
    }
}
