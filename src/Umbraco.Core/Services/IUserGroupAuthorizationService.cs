using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

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
}
