using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class SetAvatarUsersController : UsersControllerBase
{
    private readonly IUserService _userService;

    public SetAvatarUsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("avatar/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAvatar(Guid id, SetAvatarRequestModel model)
    {
        UserOperationStatus result = await _userService.SetAvatarAsync(id, model.FileKey);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
