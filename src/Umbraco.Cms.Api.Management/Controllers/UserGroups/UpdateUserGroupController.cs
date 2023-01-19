using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class UpdateUserGroupController : UserGroupsControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserGroupViewModelFactory _userGroupViewModelFactory;

    public UpdateUserGroupController(
        IUserService userService,
        IUserGroupViewModelFactory userGroupViewModelFactory)
    {
        _userService = userService;
        _userGroupViewModelFactory = userGroupViewModelFactory;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid key, UserGroupUpdateModel dataTypeUpdateModel)
    {
        // TODO: Validation etc...
        IUserGroup? existingUserGroup = _userService.GetUserGroupByKey(key);

        if (existingUserGroup is null)
        {
            return NotFound();
        }

        IUserGroup updated = _userGroupViewModelFactory.Update(existingUserGroup, dataTypeUpdateModel);

        _userService.Save(updated);
        return await Task.FromResult(Ok());
    }
}
