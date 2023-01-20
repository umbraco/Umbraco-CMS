using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroups;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroups;

public class CreateUserGroupController : UserGroupsControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserGroupViewModelFactory _userGroupViewModelFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateUserGroupController(
        IUserService userService,
        IUserGroupViewModelFactory userGroupViewModelFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userService = userService;
        _userGroupViewModelFactory = userGroupViewModelFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(UserGroupSaveModel userGroupSaveModel)
    {
        IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        if (currentUser is null)
        {
            return UserGroupOperationStatusResult(UserGroupOperationStatus.MissingUser);
        }

        IUserGroup group = _userGroupViewModelFactory.Create(userGroupSaveModel);

        Attempt<IUserGroup, UserGroupOperationStatus> result = _userService.Create(group, currentUser.Id);
        if (result.Success)
        {
            CreatedAtAction<ByKeyUserGroupController>(controller => nameof(controller.ByKey), group.Key);
        }

        return UserGroupOperationStatusResult(result.Status);
    }
}
