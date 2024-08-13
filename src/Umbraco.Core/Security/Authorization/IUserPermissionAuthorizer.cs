using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Authorizes user access.
/// </summary>
public interface IUserPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified user account.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="userKey">The key of the user to check for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, Guid userKey)
        => IsDeniedAsync(currentUser, userKey.Yield());

    /// <summary>
    ///     Authorizes whether the current user has access to the specified user account(s).
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="userKeys">The keys of the users to check for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> userKeys);
}
