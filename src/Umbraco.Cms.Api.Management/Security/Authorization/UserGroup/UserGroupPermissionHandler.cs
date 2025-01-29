using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;

/// <summary>
///     Authorizes that the current user has access to the user group(s) specified in the request.
/// </summary>
public class UserGroupPermissionHandler : MustSatisfyRequirementAuthorizationHandler<UserGroupPermissionRequirement, UserGroupPermissionResource>
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IUserGroupPermissionAuthorizer _userGroupPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupPermissionHandler" /> class.
    /// </summary>
    /// <param name="userGroupPermissionAuthorizer">Authorizer for user group access.</param>
    /// <param name="authorizationHelper">The authorization helper.</param>
    public UserGroupPermissionHandler(IUserGroupPermissionAuthorizer userGroupPermissionAuthorizer, IAuthorizationHelper authorizationHelper)
    {
        _userGroupPermissionAuthorizer = userGroupPermissionAuthorizer;
        _authorizationHelper = authorizationHelper;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        UserGroupPermissionRequirement requirement,
        UserGroupPermissionResource resource)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(context.User);

        return await _userGroupPermissionAuthorizer.IsDeniedAsync(user, resource.UserGroupKeys) is false;
    }
}
