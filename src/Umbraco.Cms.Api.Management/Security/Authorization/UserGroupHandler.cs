using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization;

/// <summary>
///     Authorizes that the current user has access to the user group specified in the request.
/// </summary>
public class UserGroupHandler : MustSatisfyRequirementAuthorizationHandler<UserGroupRequirement, IEnumerable<Guid>>
{
    private readonly IUserGroupAuthorizer _userGroupAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupHandler" /> class.
    /// </summary>
    /// <param name="userGroupAuthorizer">Authorizer for user group access.</param>
    public UserGroupHandler(IUserGroupAuthorizer userGroupAuthorizer)
        => _userGroupAuthorizer = userGroupAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        UserGroupRequirement requirement,
        IEnumerable<Guid> resource) =>
        await _userGroupAuthorizer.IsAuthorizedAsync(context.User, resource);
}
