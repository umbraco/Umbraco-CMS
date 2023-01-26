using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IUserGroupAuthorizationService
{
    /// <summary>
    /// Authorize that a user is not adding a section to the group that they don't have access to.
    /// </summary>
    /// <param name="performingUser">The user performing the action.</param>
    /// <param name="userGroup">The UserGroup being created or updated.</param>
    /// <returns>An attempt with an operation status.</returns>
    Attempt<UserGroupOperationStatus> AuthorizeSectionAccess(IUser performingUser, IUserGroup userGroup);

    /// <summary>
    /// Authorize that the user is not changing to a start node that they don't have access to.
    /// </summary>
    /// <param name="performingUser">The user performing the action.</param>
    /// <param name="userGroup">The UserGroup being created or updated.</param>
    /// <returns>An attempt with an operation status.</returns>
    Attempt<UserGroupOperationStatus> AuthorizeStartNodeChanges(IUser performingUser, IUserGroup userGroup);

    /// <summary>
    /// Ensures that a user has access to the user section.
    /// </summary>
    /// <param name="user">The user performing the action.</param>
    /// <returns>An attempt with an operation status.</returns>
    Attempt<UserGroupOperationStatus> HasAccessToUserSection(IUser user);

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
}
