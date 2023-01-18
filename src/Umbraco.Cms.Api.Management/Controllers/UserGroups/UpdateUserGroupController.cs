using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class UpdateUserGroupController : UserGroupsControllerBase
{
    [HttpPut]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid key, DataTypeUpdateModel dataTypeUpdateModel)
    {
        return await Task.FromResult(Ok());
    }
}
