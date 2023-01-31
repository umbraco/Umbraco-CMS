using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class GetAllUserGroupController : UserGroupsControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IUserGroupViewModelFactory _userViewModelFactory;

    public GetAllUserGroupController(
        IUserGroupService userGroupService,
        IUserGroupViewModelFactory userViewModelFactory)
    {
        _userGroupService = userGroupService;
        _userViewModelFactory = userViewModelFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<UserGroupViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<UserGroupViewModel>>> GetAll(int skip = 0, int take = 100)

    {
        // FIXME: In the old controller this endpoint had a switch "onlyCurrentUserGroup"
        // If this was enabled we'd only return the groups the current user was in
        // and even if it was set to false we'd still remove the admin group
        // This cannot be implemented until auth is further implemented (currently there's no way to get the current user)
        IEnumerable<IUserGroup> userGroups = await _userGroupService.GetAllAsync(skip, take);

        var viewModels = _userViewModelFactory.CreateMultiple(userGroups).ToList();
        return new PagedViewModel<UserGroupViewModel> {Total = viewModels.Count, Items = viewModels};
    }
}
