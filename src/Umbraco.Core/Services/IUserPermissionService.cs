using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for user access.
/// </summary>
public interface IUserPermissionService
{
    /// <summary>
    ///     Authorize that a user has access to user account.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="userKey">The identifier of the user account to check for access.</param>
    /// <returns>A task resolving into a <see cref="UserAuthorizationStatus"/>.</returns>
    Task<UserAuthorizationStatus> AuthorizeAccessAsync(IUser user, Guid userKey)
        => AuthorizeAccessAsync(user, userKey.Yield());

    /// <summary>
    ///     Authorize that a user has access to user accounts.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="userKeys">The identifiers of the user accounts to check for access.</param>
    /// <returns>A task resolving into a <see cref="UserAuthorizationStatus"/>.</returns>
    Task<UserAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> userKeys);
}
