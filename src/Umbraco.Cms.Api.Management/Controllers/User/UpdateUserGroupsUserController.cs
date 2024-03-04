using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

[ApiVersion("1.0")]
public class UpdateUserGroupsUserController : UserControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserGroupService _userGroupService;

    public UpdateUserGroupsUserController(IAuthorizationService authorizationService, IUserGroupService userGroupService)
    {
        _authorizationService = authorizationService;
        _userGroupService = userGroupService;
    }

    [HttpPost("set-user-groups")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserGroups(UpdateUserGroupsOnUserRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(requestModel.UserIds),
            AuthorizationPolicies.AdminUserEditsRequireAdmin);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        UserGroupOperationStatus status = await _userGroupService.UpdateUserGroupsOnUsersAsync(requestModel.UserGroupIds, requestModel.UserIds);

        // This is a bit of weird case, since it's really more of a user group operation, so our base method does not really work.
        return status switch
        {
            UserGroupOperationStatus.Success => Ok(),
            UserGroupOperationStatus.AdminGroupCannotBeEmpty => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Admin group cannot be empty")
                .WithDetail("The admin group cannot be empty.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown user group operation status.")
                .Build()),
        };
    }
}
