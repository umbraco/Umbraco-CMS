using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for user group access.
/// </summary>
public interface IUserGroupPermissionService
{
    /// <summary>
    ///     Authorize that the current user has access to the specified user group.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <param name="userGroupKey">The identifier of the user group to check for access.</param>
    /// <returns>A task resolving into a <see cref="UserGroupAuthorizationStatus"/>.</returns>
    Task<UserGroupAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, Guid userGroupKey)
        => AuthorizeAccessAsync(performingUser, new[] { userGroupKey });

    /// <summary>
    ///     Authorize that the current user belongs to these user groups.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <param name="userGroupKeys">The identifiers of the user groups to check for access.</param>
    /// <returns>A task resolving into a <see cref="UserGroupAuthorizationStatus"/>.</returns>
    Task<UserGroupAuthorizationStatus> AuthorizeAccessAsync(IUser performingUser, IEnumerable<Guid> userGroupKeys);
}
