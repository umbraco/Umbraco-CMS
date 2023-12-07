using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

/// <summary>
///     Authorizes that the current user has the correct permission access to perform actions on the user account(s) specified in the request.
/// </summary>
public class UserPermissionHandler : MustSatisfyRequirementAuthorizationHandler<UserPermissionRequirement, UserPermissionResource>
{
    private readonly IUserPermissionAuthorizer _userPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPermissionHandler" /> class.
    /// </summary>
    /// <param name="userPermissionAuthorizer">Authorizer for user access.</param>
    public UserPermissionHandler(IUserPermissionAuthorizer userPermissionAuthorizer)
        => _userPermissionAuthorizer = userPermissionAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        UserPermissionRequirement requirement,
        UserPermissionResource resource) =>
        await _userPermissionAuthorizer.IsAuthorizedAsync(context.User, resource.UserKeys);
}
