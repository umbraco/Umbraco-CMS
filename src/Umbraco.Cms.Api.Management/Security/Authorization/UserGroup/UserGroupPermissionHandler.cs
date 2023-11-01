using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;

/// <summary>
///     Authorizes that the current user has access to the user group(s) specified in the request.
/// </summary>
public class UserGroupPermissionHandler : MustSatisfyRequirementAuthorizationHandler<UserGroupPermissionRequirement, IEnumerable<Guid>>
{
    private readonly IUserGroupPermissionAuthorizer _userGroupPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupPermissionHandler" /> class.
    /// </summary>
    /// <param name="userGroupPermissionAuthorizer">Authorizer for user group access.</param>
    public UserGroupPermissionHandler(IUserGroupPermissionAuthorizer userGroupPermissionAuthorizer)
        => _userGroupPermissionAuthorizer = userGroupPermissionAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        UserGroupPermissionRequirement requirement,
        IEnumerable<Guid> resource) =>
        await _userGroupPermissionAuthorizer.IsAuthorizedAsync(context.User, resource);
}
