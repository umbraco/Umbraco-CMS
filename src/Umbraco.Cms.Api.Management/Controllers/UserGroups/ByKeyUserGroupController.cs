using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;


public class ByKeyUserGroupController : UserGroupsControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupViewModelFactory _userGroupViewModelFactory;

    public ByKeyUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupViewModelFactory userGroupViewModelFactory)
    {
        _userGroupService = userGroupService;
        _userGroupViewModelFactory = userGroupViewModelFactory;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(UserGroupViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserGroupViewModel>> ByKey(Guid key)
    {
        IUserGroup? userGroup = await _userGroupService.GetAsync(key);

        if (userGroup is null)
        {
            return NotFound();
        }

        return await _userGroupViewModelFactory.CreateAsync(userGroup);
    }
}
