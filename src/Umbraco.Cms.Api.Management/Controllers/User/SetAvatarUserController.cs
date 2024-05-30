using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class SetAvatarUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;

    public SetAvatarUserController(IUserService userService, IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _userService = userService;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("avatar/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvatar(CancellationToken cancellationToken, Guid id, SetAvatarRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        UserOperationStatus result = await _userService.SetAvatarAsync(id, model.File.Id);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
