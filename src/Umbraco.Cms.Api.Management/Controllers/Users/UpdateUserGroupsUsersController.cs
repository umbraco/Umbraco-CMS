using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Users;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Users;

public class UpdateUserGroupsUsersController : UsersControllerBase
{
    private readonly IUserGroupService _userGroupService;

    public UpdateUserGroupsUsersController(IUserGroupService userGroupService)
    {
        _userGroupService = userGroupService;
    }

    [HttpPatch("set-user-groups")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserGroups(UpdateUserGroupsOnUserRequestModel requestModel)
    {
        await _userGroupService.UpdateUserGroupsOnUsers(requestModel.UserGroupKeys, requestModel.UserKeys);
        return Ok();
    }
}
