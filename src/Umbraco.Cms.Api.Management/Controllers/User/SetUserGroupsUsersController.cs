using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class SetGroupsUsersController : UserControllerBase
{
    private readonly IUserGroupService _userGroupService;

    public SetGroupsUsersController(
        IUserGroupService userGroupService,
        IUserPresentationFactory userPresentationFactory)
    {
        _userGroupService = userGroupService;
    }

    [HttpPut("user-group-ids")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SetUserGroups(PutResourceListRequestModel requestModel)
    {
        await _userGroupService.SetUserGroupsOnUsers(
            requestModel.Resources,
            requestModel.List);
        return Ok();
    }
}
