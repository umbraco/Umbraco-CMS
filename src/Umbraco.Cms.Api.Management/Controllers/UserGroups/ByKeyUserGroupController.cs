using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;


public class ByKeyUserGroupController : UserGroupsControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserGroupViewModelFactory _userGroupViewModelFactory;

    public ByKeyUserGroupController(
        IUserService userService,
        IUserGroupViewModelFactory userGroupViewModelFactory)
    {
        _userService = userService;
        _userGroupViewModelFactory = userGroupViewModelFactory;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserGroupViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserGroupViewModel>> ByKey(Guid key)
    {
        IUserGroup? userGroup = _userService.GetUserGroupByKey(key);

        if (userGroup is null)
        {
            return NotFound();
        }

        return _userGroupViewModelFactory.Create(userGroup);
    }
}
