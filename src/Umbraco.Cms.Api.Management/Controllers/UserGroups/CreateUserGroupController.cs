using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class CreateUserGroupController : UserGroupsControllerBase
{
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(UserGroupSaveModel userGroupSaveModel)
    {
        return await Task.FromResult(CreatedAtAction<ByKeyUserGroupController>(controller => nameof(controller.ByKey), Guid.Empty));
    }
}
