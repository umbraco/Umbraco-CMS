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
    /// <summary>
    /// Controller responsible for updating the user groups assigned to a specific user.
    /// </summary>
[ApiVersion("1.0")]
[VersionedApiBackOfficeRoute("user")]
[ApiExplorerSettings(GroupName = "User")]
public class UpdateUserGroupsUserController : UserGroupControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserGroupService _userGroupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateUserGroupsUserController"/> class, which manages updating user groups for users.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user group update operations.</param>
    /// <param name="userGroupService">Service used to manage user group data and operations.</param>
    public UpdateUserGroupsUserController(IAuthorizationService authorizationService, IUserGroupService userGroupService)
    {
        _authorizationService = authorizationService;
        _userGroupService = userGroupService;
    }

    /// <summary>
    /// Updates the user group assignments for the specified users.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <param name="requestModel">The model containing the IDs of users and the user groups to assign to them.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the outcome of the operation: <c>Ok</c> if successful, or an error result if the update fails or is unauthorized.
    /// </returns>
    [HttpPost("set-user-groups")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Updates user group assignments.")]
    [EndpointDescription("Updates the user group assignments for the specified users.")]
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
