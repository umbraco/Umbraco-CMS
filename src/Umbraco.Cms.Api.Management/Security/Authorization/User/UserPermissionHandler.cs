using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

/// <summary>
///     Authorizes that the current user has the correct permission access to perform actions on the user account(s) specified in the request.
/// </summary>
public class UserPermissionHandler : MustSatisfyRequirementAuthorizationHandler<UserPermissionRequirement, UserPermissionResource>
{
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IUserPermissionAuthorizer _userPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPermissionHandler" /> class.
    /// </summary>
    /// <param name="userPermissionAuthorizer">Authorizer for user access.</param>
    /// <param name="authorizationHelper">The authorization helper.</param>
    public UserPermissionHandler(IUserPermissionAuthorizer userPermissionAuthorizer, IAuthorizationHelper authorizationHelper)
    {
        _userPermissionAuthorizer = userPermissionAuthorizer;
        _authorizationHelper = authorizationHelper;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        UserPermissionRequirement requirement,
        UserPermissionResource resource)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(context.User);

        return await _userPermissionAuthorizer.IsDeniedAsync(user, resource.UserKeys) is false;
    }
}
