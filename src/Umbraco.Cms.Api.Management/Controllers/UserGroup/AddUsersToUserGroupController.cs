using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.UserGroup;

    /// <summary>
    /// API controller responsible for handling requests to add users to a specified user group.
    /// </summary>
[ApiVersion("1.0")]
public class AddUsersToUserGroupController : UserGroupControllerBase
{
    private readonly IUserGroupService _userGroupService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddUsersToUserGroupController"/> class, which manages the addition of users to user groups.
    /// </summary>
    /// <param name="userGroupService">Service used to manage user groups.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    public AddUsersToUserGroupController(
        IUserGroupService userGroupService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuthorizationService authorizationService)
    {
        _userGroupService = userGroupService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Adds the specified users to the user group identified by the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <param name="id">The unique identifier (<see cref="Guid"/>) of the user group to which users will be added.</param>
    /// <param name="userIds">An array of <see cref="ReferenceByIdModel"/> objects referencing the users to add to the user group.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation: <c>Ok</c> if successful, or an error response if the user group is not found or the operation fails.
    /// </returns>
    [HttpPost("{id:guid}/users")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Adds users to a user group.")]
    [EndpointDescription("Adds the specified users to the user group identified by the provided Id.")]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, ReferenceByIdModel[] userIds)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            UserGroupPermissionResource.WithKeys(id),
            AuthorizationPolicies.UserBelongsToUserGroupInRequest);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<UserGroupOperationStatus> result = await _userGroupService.AddUsersToUserGroupAsync(
            new UsersToUserGroupManipulationModel(id, userIds.Select(x => x.Id).ToArray()), CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : UserGroupOperationStatusResult(result.Result);
    }
}
