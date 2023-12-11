using System.Security.Principal;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;

/// <summary>
///     Authorizes user group access.
/// </summary>
public interface IUserGroupPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified user group.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="userGroupKey">The key of the user group to check against.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAsync(IPrincipal currentUser, Guid userGroupKey)
        => IsAuthorizedAsync(currentUser, userGroupKey.Yield());

    /// <summary>
    ///     Authorizes whether the current user has access to the specified user group(s).
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="userGroupKeys">The keys of the user groups to check against.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> userGroupKeys);
}
