using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for user access.
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    ///     Authorize that the current user has access to perform actions on the specified user account.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <param name="userKey">The identifier of the user account to check for access.</param>
    /// <returns>A task resolving into a <see cref="UserAuthorizationStatus"/>.</returns>
    Task<UserAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, Guid userKey)
        => AuthorizeAccessAsync(performingUser, new[] { userKey });

    /// <summary>
    ///     Authorize that the current user has access to perform actions on these user accounts.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <param name="userKeys">The identifiers of the user accounts to check for access.</param>
    /// <returns>A task resolving into a <see cref="UserAuthorizationStatus"/>.</returns>
    Task<UserAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, IEnumerable<Guid> userKeys);
}
