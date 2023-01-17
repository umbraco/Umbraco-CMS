using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class GetAllUserGroupController : UserGroupsControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserGroupViewModelFactory _userViewModelFactory;

    public GetAllUserGroupController(
        IUserService userService,
        IUserGroupViewModelFactory userViewModelFactory)
    {
        _userService = userService;
        _userViewModelFactory = userViewModelFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<UserGroupViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserGroupViewModel>>> GetAll()
    {
        // FIXME: In the old controller this endpoint had a switch "onlyCurrentUserGroup"
        // If this was enabled we'd only return the groups the current user was in
        // and even if it was set to false we'd still remove the admin group
        // This cannot be implemented until auth is further implemented (currently there's no way to get the current user)
        IEnumerable<IUserGroup> userGroups = _userService.GetAllUserGroups();
        return _userViewModelFactory.CreateMultiple(userGroups).ToList();
    }
}
