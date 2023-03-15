using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class ClearAvatarUsersController : UsersControllerBase
{
    private readonly IUserService _userService;

    public ClearAvatarUsersController(IUserService userService) => _userService = userService;

    [HttpDelete("avatar/{userKey:guid}")]
    public async Task<IActionResult> ClearAvatar(Guid userKey)
    {
        UserOperationStatus result = await _userService.ClearAvatarAsync(userKey);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
