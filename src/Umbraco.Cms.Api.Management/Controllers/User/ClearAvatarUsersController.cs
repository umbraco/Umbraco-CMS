using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

public class ClearAvatarUserController : UserControllerBase
{
    private readonly IUserService _userService;

    public ClearAvatarUserController(IUserService userService) => _userService = userService;

    [HttpDelete("avatar/{id:guid}")]
    public async Task<IActionResult> ClearAvatar(Guid id)
    {
        UserOperationStatus result = await _userService.ClearAvatarAsync(id);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
