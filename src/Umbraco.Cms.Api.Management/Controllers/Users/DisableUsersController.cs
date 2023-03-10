using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class DisableUsersController : UsersControllerBase
{
    private readonly IUserService _userService;

    public DisableUsersController(IUserService userService) => _userService = userService;

    [HttpPatch("disable")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableUser(DisableUserRequestModel model)
    {
        UserOperationStatus result = await _userService.DisableAsync(-1, model.UserKeys.ToArray());

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
