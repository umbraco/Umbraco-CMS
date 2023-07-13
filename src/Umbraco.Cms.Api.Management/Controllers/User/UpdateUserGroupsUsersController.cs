using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
[Authorize(Policy = "New" + AuthorizationPolicies.AdminUserEditsRequireAdmin)]
public class UpdateUserGroupsUserController : UserControllerBase
{
    private readonly IUserGroupService _userGroupService;

    public UpdateUserGroupsUserController(IUserGroupService userGroupService)
    {
        _userGroupService = userGroupService;
    }

    [HttpPost("set-user-groups")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserGroups(UpdateUserGroupsOnUserRequestModel requestModel)
    {
        await _userGroupService.UpdateUserGroupsOnUsers(requestModel.UserGroupIds, requestModel.UserIds);
        return Ok();
    }
}
