using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class InviteUsersController : UsersControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserPresentationFactory _userPresentationFactory;

    public InviteUsersController(
        IUserService userService,
        IUserPresentationFactory userPresentationFactory)
    {
        _userService = userService;
        _userPresentationFactory = userPresentationFactory;
    }


    [HttpPost("invite")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Invite(InviteUserRequestModel model)
    {
        UserInviteModel userInvite = await _userPresentationFactory.CreateInviteModelAsync(model);

        // FIXME: use the actual currently logged in user key
        Attempt<UserInvitationResult, UserOperationStatus> result = await _userService.InviteAsync(Constants.Security.SuperUserKey, userInvite);

        if (result.Success)
        {
            return Ok();
        }

        return result.Status is UserOperationStatus.UnknownFailure
            ? FormatErrorMessageResult(result.Result)
            : UserOperationStatusResult(result.Status);
    }
}
