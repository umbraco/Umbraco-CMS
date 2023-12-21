using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

[ApiVersion("1.0")]
public class RemoveUsersFromUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public RemoveUsersFromUserGroupController(
        IUserGroupService userGroupService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _userGroupService = userGroupService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete("{id:guid}/users")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, Guid[] userIds)
    {
        UserGroupOperationStatus result = await _userGroupService.RemoveUsersFromUserGroupAsync(
            new UsersToUserGroupManipulationModel(id, userIds), CurrentUserKey(_backOfficeSecurityAccessor));

        return result == UserGroupOperationStatus.Success
            ? Ok()
            : UserGroupOperationStatusResult(result);
    }
}
