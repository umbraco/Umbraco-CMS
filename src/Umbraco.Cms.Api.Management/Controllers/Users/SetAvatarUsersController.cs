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
        IUser? user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        UserOperationStatus result = await _userService.SetAvatarAsync(user, model.FileId);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
