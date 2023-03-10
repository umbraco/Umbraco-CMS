using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class EnableUsersController : UsersControllerBase
{
    private readonly IUserService _userService;

    public EnableUsersController(IUserService userService) => _userService = userService;

    [HttpPatch("enable")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableUser(EnableUserRequestModel model)
    {
        UserOperationStatus result = await _userService.EnableAsync(-1, model.UserKeys.ToArray());

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
