using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class ClearAvatarUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserService _userService;

    public ClearAvatarUserController(IAuthorizationService authorizationService, IUserService userService)
    {
        _authorizationService = authorizationService;
        _userService = userService;
    }

    [MapToApiVersion("1.0")]
    [HttpDelete("avatar/{id:guid}")]
    public async Task<IActionResult> ClearAvatar(Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, new[] { id },
            $"New{AuthorizationPolicies.AdminUserEditsRequireAdmin}");

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        UserOperationStatus result = await _userService.ClearAvatarAsync(id);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
