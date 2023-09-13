using System.Security.Principal;

namespace Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;

/// <summary>
///     Authorizes user group access.
/// </summary>
public interface IUserGroupAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified user group(s).
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="userGroupKeys">The keys of the user groups to check against.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAsync(IPrincipal currentUser, IEnumerable<Guid> userGroupKeys);
}
