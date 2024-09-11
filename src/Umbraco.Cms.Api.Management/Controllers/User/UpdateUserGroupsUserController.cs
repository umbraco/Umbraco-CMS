using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.UserGroup;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.User;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.User;

// This controller is a bit of a weird case, for all intents and purposes this should be a UserGroupController
// It uses the UserGroupService to manipulate the members of a user group, however, from the frontend perspective it is a user(s) operation
// In order to not have to re-implement all the UserGroupOperationStatusResults this controller inherits from UserGroupControllerBase
// But manually specifies its route and APIExplorerSettings to be under users.
[ApiVersion("1.0")]
[VersionedApiBackOfficeRoute("user")]
[ApiExplorerSettings(GroupName = "User")]
public class UpdateUserGroupsUserController : UserGroupControllerBase
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
    public async Task<IActionResult> UpdateUserGroups(
        CancellationToken cancellationToken,
        UpdateUserGroupsOnUserRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserPermissionResource.WithKeys(requestModel.UserIds.Select(x => x.Id)),
            AuthorizationPolicies.UserPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<UserGroupOperationStatus> result = await _userGroupService.UpdateUserGroupsOnUsersAsync(
            requestModel.UserGroupIds.Select(x => x.Id).ToHashSet(),
            requestModel.UserIds.Select(x => x.Id).ToHashSet());

        return result.Success
            ? Ok()
            : UserGroupOperationStatusResult(result.Result);
    }
}
