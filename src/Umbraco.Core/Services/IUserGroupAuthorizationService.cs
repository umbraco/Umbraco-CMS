using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

// FIXME: Start using <see cref="UserGroupAuthorizationStatus"/> for all authorization things
// + refactor to return only <see cref="UserGroupAuthorizationStatus"/>, without the Attempt
public interface IUserGroupAuthorizationService
{
    /// <summary>
    /// Authorizes a user to create a new user group.
    /// </summary>
    /// <param name="performingUser">The user performing the create operation.</param>
    /// <param name="userGroup">The user group to be created.</param>
    /// <returns>An attempt with an operation status.</returns>
    Attempt<UserGroupOperationStatus> AuthorizeUserGroupCreation(IUser performingUser, IUserGroup userGroup);

    /// <summary>
    /// Authorizes a user to update an existing user group.
    /// </summary>
    /// <param name="performingUser">The user performing the update operation.</param>
    /// <param name="userGroup">The user group to be created.</param>
    /// <returns>An attempt with an operation.</returns>
    Attempt<UserGroupOperationStatus> AuthorizeUserGroupUpdate(IUser performingUser, IUserGroup userGroup);

    /// <summary>
    ///     Authorize that the current user belongs to these groups.
    /// </summary>
    /// <param name="performingUser">The user performing the operation.</param>
    /// <param name="userGroups">The user groups to check for access.</param>
    /// <returns><see cref="UserGroupOperationStatus"/>.</returns>
    UserGroupOperationStatus AuthorizeGroupAccess(IUser performingUser, IEnumerable<IUserGroup> userGroups);
}
