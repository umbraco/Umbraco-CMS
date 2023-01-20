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
}
