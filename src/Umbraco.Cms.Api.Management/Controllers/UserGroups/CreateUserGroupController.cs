using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class CreateUserGroupController : UserGroupsControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserGroupViewModelFactory _userGroupViewModelFactory;

    public CreateUserGroupController(IUserService userService, IUserGroupViewModelFactory userGroupViewModelFactory)
    {
        _userService = userService;
        _userGroupViewModelFactory = userGroupViewModelFactory;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(UserGroupSaveModel userGroupSaveModel)
    {
        // TODO: Validation etc...

        IUserGroup group = _userGroupViewModelFactory.Create(userGroupSaveModel);
        _userService.Save(group);
        return await Task.FromResult(CreatedAtAction<ByKeyUserGroupController>(controller => nameof(controller.ByKey), group.Key));
    }
}
