using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Headers;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class UpdateGroupsUsersController : UserControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserPresentationFactory _userPresentationFactory;

    public UpdateGroupsUsersController(
        IUserGroupService userGroupService,
        IUserPresentationFactory userPresentationFactory)
    {
        _userGroupService = userGroupService;
        _userPresentationFactory = userPresentationFactory;
    }

    [HttpPatch("user-group-ids")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<UserResponseModel>), StatusCodes.Status200OK)]
    [MultiResourceListPatch]
    public async Task<IActionResult> UpdateUserGroups(PatchResourceListRequestModel requestModel)
    {
        IUser[] updatedUsers = await _userGroupService.UpdateUserGroupsOnUsers(
            requestModel.Resources,
            requestModel.Add,
            requestModel.Remove);
        return Ok(updatedUsers.Select(user => _userPresentationFactory.CreateResponseModel(user)));
    }
}
