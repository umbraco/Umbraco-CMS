using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

/// <summary>
/// Handles bulk deletion operations for user groups.
/// </summary>
[ApiVersion("1.0")]
public class BulkDeleteUserGroupsController : UserGroupControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserGroupService _userGroupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkDeleteUserGroupsController"/> class.
    /// </summary>
    /// <param name="authorizationService">The authorization service used for permission checks.</param>
    /// <param name="userGroupService">The user group service used to manage user groups.</param>
    public BulkDeleteUserGroupsController(IAuthorizationService authorizationService, IUserGroupService userGroupService)
    {
        _authorizationService = authorizationService;
        _userGroupService = userGroupService;
    }

    /// <summary>
    /// Deletes multiple user groups identified by the provided IDs.
    /// This operation cannot be undone.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <param name="model">The request model containing the collection of user group IDs to delete.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the delete operation.
    /// Returns <c>200 OK</c> if the deletion was successful, or <c>404 Not Found</c> if any user group was not found.
    /// </returns>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes multiple user groups.")]
    [EndpointDescription("Deletes multiple user groups identified by the provided Ids. This operation cannot be undone.")]
    public async Task<IActionResult> BulkDelete(CancellationToken cancellationToken, DeleteUserGroupsRequestModel model)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            new UserGroupPermissionResource(model.UserGroupIds.Select(x => x.Id)),
            AuthorizationPolicies.UserBelongsToUserGroupInRequest);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<UserGroupOperationStatus> result = await _userGroupService.DeleteAsync(model.UserGroupIds.Select(x => x.Id).ToHashSet());

        return result.Success
            ? Ok()
            : UserGroupOperationStatusResult(result.Result);
    }
}
