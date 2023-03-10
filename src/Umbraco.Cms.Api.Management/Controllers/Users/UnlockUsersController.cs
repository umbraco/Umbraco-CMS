using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class UnlockUsersController : UsersControllerBase
{
    private readonly IUserService _userService;

    public UnlockUsersController(IUserService userService) => _userService = userService;

    [HttpPatch("unlock")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnlockUsers(UnlockUsersRequestModel model)
    {
        Attempt<UserUnlockResult, UserOperationStatus> attempt = await _userService.UnlockAsync(-1, model.UserKeys.ToArray());

        if (attempt.Success)
        {
            return Ok();
        }

        return attempt.Status is UserOperationStatus.UnknownFailure
            ? FormatErrorMessageResult(attempt.Result)
            : UserOperationStatusResult(attempt.Status);
    }
}
