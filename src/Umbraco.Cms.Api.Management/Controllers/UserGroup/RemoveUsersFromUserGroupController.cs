using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

/// <summary>
/// API controller responsible for handling requests to remove users from a specified user group.
/// </summary>
[ApiVersion("1.0")]
public class RemoveUsersFromUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveUsersFromUserGroupController"/> class, which handles requests to remove users from a user group.
    /// </summary>
    /// <param name="userGroupService">Service used to manage user groups.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features.</param>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    public RemoveUsersFromUserGroupController(
        IUserGroupService userGroupService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _userGroupService = userGroupService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Removes the specified users from the user group identified by the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the user group from which users will be removed.</param>
    /// <param name="userIds">An array of <see cref="ReferenceByIdModel"/> objects representing the users to remove from the user group.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> if the users were successfully removed.</description></item>
    /// <item><description><c>404 Not Found</c> if the user group does not exist.</description></item>
    /// </list>
    /// </returns>
    [HttpDelete("{id:guid}/users")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Removes users from a user group.")]
    [EndpointDescription("Removes the specified users from the user group identified by the provided Id.")]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, ReferenceByIdModel[] userIds)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserGroupPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserBelongsToUserGroupInRequest);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<UserGroupOperationStatus> result = await _userGroupService.RemoveUsersFromUserGroupAsync(
            new UsersToUserGroupManipulationModel(id, userIds.Select(x => x.Id).ToArray()), CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : UserGroupOperationStatusResult(result.Result);
    }
}
