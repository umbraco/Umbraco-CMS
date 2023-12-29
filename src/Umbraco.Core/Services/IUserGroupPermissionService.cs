using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for user group access.
/// </summary>
public interface IUserGroupPermissionService
{
    /// <summary>
    ///     Authorize that a user belongs to a user group.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="userGroupKey">The identifier of the user group to check for access.</param>
    /// <returns>A task resolving into a <see cref="UserGroupAuthorizationStatus"/>.</returns>
    Task<UserGroupAuthorizationStatus> AuthorizeAccessAsync(IUser user, Guid userGroupKey)
        => AuthorizeAccessAsync(user, userGroupKey.Yield());

    /// <summary>
    ///     Authorize that a user belongs to user groups.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="userGroupKeys">The identifiers of the user groups to check for access.</param>
    /// <returns>A task resolving into a <see cref="UserGroupAuthorizationStatus"/>.</returns>
    Task<UserGroupAuthorizationStatus> AuthorizeAccessAsync(IUser user, IEnumerable<Guid> userGroupKeys);

    /// <summary>
    ///     Authorize that a user is allowed to create a new user group.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="userGroup">The user group to be created.</param>
    /// <returns>A task resolving into a <see cref="UserGroupAuthorizationStatus"/>.</returns>
    Task<UserGroupAuthorizationStatus> AuthorizeCreateAsync(IUser user, IUserGroup userGroup);

    /// <summary>
    ///     Authorize that a user is allowed to update an existing user group.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to authorize.</param>
    /// <param name="userGroup">The user group to be updated.</param>
    /// <returns>A task resolving into a <see cref="UserGroupAuthorizationStatus"/>.</returns>
    Task<UserGroupAuthorizationStatus> AuthorizeUpdateAsync(IUser user, IUserGroup userGroup);
}
