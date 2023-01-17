using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class DeleteUserGroupController : UserGroupsControllerBase
{
    private readonly IUserService _userService;

    public DeleteUserGroupController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpDelete("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid key)
    {
        IUserGroup? group = _userService.GetUserGroupByKey(key);

        if (group is null)
        {
            return NotFound();
        }

        _userService.DeleteUserGroup(group);
        return Ok();
    }
}
