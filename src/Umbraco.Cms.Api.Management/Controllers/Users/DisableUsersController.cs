using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class DisableUsersController : UsersControllerBase
{
    private readonly IUserService _userService;

    public DisableUsersController(IUserService userService) => _userService = userService;

    [HttpPost("disable")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableUsers(DisableUserRequestModel model)
    {
        // FIXME: use the actual currently logged in user key
        UserOperationStatus result = await _userService.DisableAsync(Constants.Security.SuperUserKey, model.UserIds);

        return result is UserOperationStatus.Success
            ? Ok()
            : UserOperationStatusResult(result);
    }
}
