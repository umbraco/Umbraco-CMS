using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;


public class ByKeyUserGroupController : UserGroupsControllerBase
{
    private readonly IUserService _userService;

    public ByKeyUserGroupController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DataTypeViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid key)
    {
        _userService.GetUserGroupByKey(key);
        return Ok();
    }
}
