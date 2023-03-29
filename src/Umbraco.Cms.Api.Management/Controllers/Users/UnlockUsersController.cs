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

    [HttpPost("unlock")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnlockUsers(UnlockUsersRequestModel model)
    {
        // FIXME: use the actual currently logged in user key
        Attempt<UserUnlockResult, UserOperationStatus> attempt = await _userService.UnlockAsync(Constants.Security.SuperUserKey, model.UserIds.ToArray());

        if (attempt.Success)
        {
            return Ok();
        }

        return attempt.Status is UserOperationStatus.UnknownFailure
            ? FormatErrorMessageResult(attempt.Result)
            : UserOperationStatusResult(attempt.Status);
    }
}
