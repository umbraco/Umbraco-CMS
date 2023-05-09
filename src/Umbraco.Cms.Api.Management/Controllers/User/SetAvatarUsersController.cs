using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class SetAvatarUserController : UserControllerBase
{
    private readonly IUserService _userService;

    public SetAvatarUserController(IUserService userService)
    {
        _userService = userService;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("avatar/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAvatar(Guid id, SetAvatarRequestModel model)
    {
        UserOperationStatus result = await _userService.SetAvatarAsync(id, model.FileId);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
