using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class DeleteUserGroupController : UserGroupsControllerBase
{
    private readonly IUserGroupService _userGroupService;

    public DeleteUserGroupController(IUserGroupService userGroupService)
    {
        _userGroupService = userGroupService;
    }

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        Attempt<UserGroupOperationStatus> result = await _userGroupService.DeleteAsync(id);

        return result.Success
            ? Ok()
            : UserGroupOperationStatusResult(result.Result);
    }
}
