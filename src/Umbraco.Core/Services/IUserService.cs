using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the UserService, which is an easy access to operations involving <see cref="IProfile" /> and eventually
///     Users.
/// </summary>
public interface IUserService : IMembershipUserService
{
    /// <summary>
    ///     Creates a database entry for starting a new login session for a user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="requestingIpAddress"></param>
    /// <returns></returns>
    Guid CreateLoginSession(int userId, string requestingIpAddress);

    /// <summary>
    ///     Validates that a user login session is valid/current and hasn't been closed
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    bool ValidateLoginSession(int userId, Guid sessionId);

    /// <summary>
    ///     Removes the session's validity
    /// </summary>
    /// <param name="sessionId"></param>
    void ClearLoginSession(Guid sessionId);

    /// <summary>
    ///     Removes all valid sessions for the user
    /// </summary>
    /// <param name="userId"></param>
    int ClearLoginSessions(int userId);

    /// <summary>
    ///     This is basically facets of UserStates key = state, value = count
    /// </summary>
    IDictionary<UserState, int> GetUserStates();

    /// <summary>
    /// Creates a user based in a create model and persists it to the database.
    /// </summary>
    /// <remarks>
    /// This creates both the Umbraco user and the identity user.
    /// </remarks>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="model">Model to create the user from.</param>
    /// <param name="approveUser">Specifies if the user should be enabled be default. Defaults to false.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<UserCreationResult, UserOperationStatus>> CreateAsync(Guid performingUserKey, UserCreateModel model, bool approveUser = false);

    Task<Attempt<UserInvitationResult, UserOperationStatus>> InviteAsync(Guid performingUserKey, UserInviteModel model);

    Task<Attempt<UserOperationStatus>> VerifyInviteAsync(Guid userKey, string token);

    Task<Attempt<PasswordChangedModel, UserOperationStatus>> CreateInitialPasswordAsync(Guid userKey, string token, string password);

    Task<Attempt<IUser?, UserOperationStatus>> UpdateAsync(Guid performingUserKey, UserUpdateModel model);

    Task<UserOperationStatus> SetAvatarAsync(Guid userKey, Guid temporaryFileKey);

    Task<UserOperationStatus> DeleteAsync(Guid performingUserKey, ISet<Guid> keys);

    Task<UserOperationStatus> DeleteAsync(Guid performingUserKey, Guid key) => DeleteAsync(performingUserKey, new HashSet<Guid> { key });

    Task<UserOperationStatus> DisableAsync(Guid performingUserKey, ISet<Guid> keys);

    Task<UserOperationStatus> EnableAsync(Guid performingUserKey, ISet<Guid> keys);

    Task<Attempt<UserUnlockResult, UserOperationStatus>> UnlockAsync(Guid performingUserKey, params Guid[] keys);

    Task<Attempt<PasswordChangedModel, UserOperationStatus>> ChangePasswordAsync(Guid performingUserKey, ChangeUserPasswordModel model);

    Task<UserOperationStatus> ClearAvatarAsync(Guid userKey);

    Task<Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus>> GetLinkedLoginsAsync(Guid userKey);

    /// <summary>
    /// Gets all users that the requesting user is allowed to see.
    /// </summary>
    /// <param name="performingUserKey">The Key of the user requesting the users.</param>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>All users that the user is allowed to see.</returns>
    Task<Attempt<PagedModel<IUser>?, UserOperationStatus>> GetAllAsync(Guid performingUserKey, int skip, int take) => throw new NotImplementedException();

    public Task<Attempt<PagedModel<IUser>, UserOperationStatus>> FilterAsync(
        Guid userKey,
        UserFilter filter,
        int skip = 0,
        int take = 100,
        UserOrder orderBy = UserOrder.UserName,
        Direction orderDirection = Direction.Ascending) => throw new NotImplementedException();

    /// <summary>
    ///     Get paged users
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="orderBy"></param>
    /// <param name="orderDirection"></param>
    /// <param name="userState"></param>
    /// <param name="includeUserGroups">
    ///     A filter to only include user that belong to these user groups
    /// </param>
    /// <param name="excludeUserGroups">
    ///     A filter to only include users that do not belong to these user groups
    /// </param>
    /// <param name="filter"></param>
    /// <returns></returns>
    IEnumerable<IUser> GetAll(
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string orderBy,
        Direction orderDirection,
        UserState[]? userState = null,
        string[]? includeUserGroups = null,
        string[]? excludeUserGroups = null,
        IQuery<IUser>? filter = null);

    /// <summary>
    ///     Get paged users
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="totalRecords"></param>
    /// <param name="orderBy"></param>
    /// <param name="orderDirection"></param>
    /// <param name="userState"></param>
    /// <param name="userGroups">
    ///     A filter to only include user that belong to these user groups
    /// </param>
    /// <param name="filter"></param>
    /// <returns></returns>
    IEnumerable<IUser> GetAll(
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string orderBy,
        Direction orderDirection,
        UserState[]? userState = null,
        string[]? userGroups = null,
        string? filter = null);

    /// <summary>
    ///     Deletes or disables a User
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to delete</param>
    /// <param name="deletePermanently"><c>True</c> to permanently delete the user, <c>False</c> to disable the user</param>
    void Delete(IUser user, bool deletePermanently);

    /// <summary>
    ///     Gets an IProfile by User Id.
    /// </summary>
    /// <param name="id">Id of the User to retrieve</param>
    /// <returns>
    ///     <see cref="IProfile" />
    /// </returns>
    IProfile? GetProfileById(int id);

    /// <summary>
    ///     Gets a profile by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>
    ///     <see cref="IProfile" />
    /// </returns>
    IProfile? GetProfileByUserName(string username);

    /// <summary>
    /// Get a user by its key.
    /// </summary>
    /// <param name="key">The GUID key of the user.</param>
    /// <returns>The found user, or null if nothing was found.</returns>
    Task<IUser?> GetAsync(Guid key) => Task.FromResult(GetAll(0, int.MaxValue, out _).FirstOrDefault(x=>x.Key == key));

    Task<IEnumerable<IUser>> GetAsync(IEnumerable<Guid> keys) => Task.FromResult(GetAll(0, int.MaxValue, out _).Where(x => keys.Contains(x.Key)));

    /// <summary>
    ///     Gets a user by Id
    /// </summary>
    /// <param name="id">Id of the user to retrieve</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    IUser? GetUserById(int id);

    /// <summary>
    ///     Gets a users by Id
    /// </summary>
    /// <param name="ids">Ids of the users to retrieve</param>
    /// <returns>
    ///     <see cref="IUser" />
    /// </returns>
    IEnumerable<IUser> GetUsersById(params int[]? ids);

    /// <summary>
    ///     Removes a specific section from all user groups
    /// </summary>
    /// <remarks>This is useful when an entire section is removed from config</remarks>
    /// <param name="sectionAlias">Alias of the section to remove</param>
    void DeleteSectionFromAllUserGroups(string sectionAlias);

    /// <summary>
    /// Get explicitly assigned permissions for a user and node keys.
    /// </summary>
    /// <param name="userKey">Key of user to retrieve permissions for. </param>
    /// <param name="nodeKeys">The keys of the nodes to get permissions for.</param>
    /// <returns>An enumerable list of <see cref="NodePermissions"/>.</returns>
    Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetPermissionsAsync(Guid userKey, params Guid[] nodeKeys);

    /// <summary>
    /// Get explicitly assigned content permissions for a user and node keys.
    /// </summary>
    /// <param name="userKey">Key of user to retrieve permissions for. </param>
    /// <param name="mediaKeys">The keys of the media to get permissions for.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserOperationStatus"/>, and an enumerable of permissions.</returns>
    Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetMediaPermissionsAsync(Guid userKey, IEnumerable<Guid> mediaKeys);

    /// <summary>
    /// Get explicitly assigned media permissions for a user and node keys.
    /// </summary>
    /// <param name="userKey">Key of user to retrieve permissions for. </param>
    /// <param name="contentKeys">The keys of the content to get permissions for.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserOperationStatus"/>, and an enumerable of permissions.</returns>
    Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetDocumentPermissionsAsync(Guid userKey, IEnumerable<Guid> contentKeys);

    /// <summary>
    ///     Get explicitly assigned permissions for a user and optional node ids
    /// </summary>
    /// <remarks>If no permissions are found for a particular entity then the user's default permissions will be applied</remarks>
    /// <param name="user">User to retrieve permissions for</param>
    /// <param name="nodeIds">
    ///     Specifying nothing will return all user permissions for all nodes that have explicit permissions
    ///     defined
    /// </param>
    /// <returns>An enumerable list of <see cref="EntityPermission" /></returns>
    /// <remarks>
    ///     This will return the default permissions for the user's groups for node ids that don't have explicitly defined
    ///     permissions
    /// </remarks>
    EntityPermissionCollection GetPermissions(IUser? user, params int[] nodeIds);

    /// <summary>
    ///     Get explicitly assigned permissions for groups and optional node Ids
    /// </summary>
    /// <param name="groups"></param>
    /// <param name="fallbackToDefaultPermissions">
    ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit
    ///     permissions set
    /// </param>
    /// <param name="nodeIds">Specifying nothing will return all permissions for all nodes</param>
    /// <returns>An enumerable list of <see cref="EntityPermission" /></returns>
    EntityPermissionCollection GetPermissions(IUserGroup?[] groups, bool fallbackToDefaultPermissions, params int[] nodeIds);

    /// <summary>
    ///     Gets the implicit/inherited permissions for the user for the given path
    /// </summary>
    /// <param name="user">User to check permissions for</param>
    /// <param name="path">Path to check permissions for</param>
    EntityPermissionSet GetPermissionsForPath(IUser? user, string? path);

    /// <summary>
    ///     Gets the permissions for the provided groups and path
    /// </summary>
    /// <param name="groups"></param>
    /// <param name="path">Path to check permissions for</param>
    /// <param name="fallbackToDefaultPermissions">
    ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit
    ///     permissions set
    /// </param>
    EntityPermissionSet GetPermissionsForPath(IUserGroup[] groups, string path, bool fallbackToDefaultPermissions = false);

    /// <summary>
    ///     Replaces the same permission set for a single group to any number of entities
    /// </summary>
    /// <param name="groupId">Id of the group</param>
    /// <param name="permissions">
    ///     Permissions as enumerable list of <see cref="char" />,
    ///     if no permissions are specified then all permissions for this node are removed for this group
    /// </param>
    /// <param name="entityIds">
    ///     Specify the nodes to replace permissions for. If nothing is specified all permissions are
    ///     removed.
    /// </param>
    /// <remarks>If no 'entityIds' are specified all permissions will be removed for the specified group.</remarks>
    void ReplaceUserGroupPermissions(int groupId, ISet<string> permissions, params int[] entityIds);

    /// <summary>
    ///     Assigns the same permission set for a single user group to any number of entities
    /// </summary>
    /// <param name="groupId">Id of the group</param>
    /// <param name="permission"></param>
    /// <param name="entityIds">Specify the nodes to replace permissions for</param>
    void AssignUserGroupPermission(int groupId, string permission, params int[] entityIds);

    /// <summary>
    ///     Gets a list of <see cref="IUser" /> objects associated with a given group
    /// </summary>
    /// <param name="groupId">Id of group</param>
    /// <returns>
    ///     <see cref="IEnumerable{IUser}" />
    /// </returns>
    IEnumerable<IUser> GetAllInGroup(int? groupId);

    /// <summary>
    ///     Gets a list of <see cref="IUser" /> objects not associated with a given group
    /// </summary>
    /// <param name="groupId">Id of group</param>
    /// <returns>
    ///     <see cref="IEnumerable{IUser}" />
    /// </returns>
    IEnumerable<IUser> GetAllNotInGroup(int groupId);

    #region User groups

    /// <summary>
    ///     Gets all UserGroups or those specified as parameters
    /// </summary>
    /// <param name="ids">Optional Ids of UserGroups to retrieve</param>
    /// <returns>An enumerable list of <see cref="IUserGroup" /></returns>
    [Obsolete("Use IUserGroupService.GetAsync instead, scheduled for removal in V15.")]
    IEnumerable<IUserGroup> GetAllUserGroups(params int[] ids);

    /// <summary>
    ///     Gets a UserGroup by its Alias
    /// </summary>
    /// <param name="alias">Alias of the UserGroup to retrieve</param>
    /// <returns>
    ///     <see cref="IUserGroup" />
    /// </returns>
    [Obsolete("Use IUserGroupService.GetAsync instead, scheduled for removal in V15.")]
    IEnumerable<IUserGroup> GetUserGroupsByAlias(params string[] alias);

    /// <summary>
    ///     Gets a UserGroup by its Alias
    /// </summary>
    /// <param name="name">Name of the UserGroup to retrieve</param>
    /// <returns>
    ///     <see cref="IUserGroup" />
    /// </returns>
    [Obsolete("Use IUserGroupService.GetAsync instead, scheduled for removal in V15.")]
    IUserGroup? GetUserGroupByAlias(string name);

    /// <summary>
    ///     Gets a UserGroup by its Id
    /// </summary>
    /// <param name="id">Id of the UserGroup to retrieve</param>
    /// <returns>
    ///     <see cref="IUserGroup" />
    /// </returns>
    [Obsolete("Use IUserGroupService.GetAsync instead, scheduled for removal in V15.")]
    IUserGroup? GetUserGroupById(int id);

    /// <summary>
    ///     Saves a UserGroup
    /// </summary>
    /// <param name="userGroup">UserGroup to save</param>
    /// <param name="userIds">
    ///     If null than no changes are made to the users who are assigned to this group, however if a value is passed in
    ///     than all users will be removed from this group and only these users will be added
    /// </param>
    [Obsolete("Use IUserGroupService.CreateAsync and IUserGroupService.UpdateAsync instead, scheduled for removal in V15.")]
    void Save(IUserGroup userGroup, int[]? userIds = null);

    /// <summary>
    ///     Deletes a UserGroup
    /// </summary>
    /// <param name="userGroup">UserGroup to delete</param>
    [Obsolete("Use IUserGroupService.DeleteAsync instead, scheduled for removal in V15.")]
    void DeleteUserGroup(IUserGroup userGroup);

    #endregion

    /// <summary>
    ///     Verifies the reset code sent from the reset password mail for a given user.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <param name="token">The reset password token.</param>
    Task<Attempt<UserOperationStatus>> VerifyPasswordResetAsync(Guid userKey, string token);

    /// <summary>
    ///     Changes the user's password.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <param name="token">The reset password token.</param>
    /// <param name="password">The new password of the user.</param>
    Task<Attempt<PasswordChangedModel, UserOperationStatus>> ResetPasswordAsync(Guid userKey, string token, string password);

    /// <summary>
    ///     Sends an email with a link to reset user's password.
    /// </summary>
    /// <param name="userEmail">The email address of the user.</param>
    Task<Attempt<UserOperationStatus>> SendResetPasswordEmailAsync(string userEmail);

    Task<Attempt<UserInvitationResult, UserOperationStatus>> ResendInvitationAsync(Guid performingUserKey, UserResendInviteModel model);

    Task<Attempt<PasswordChangedModel, UserOperationStatus>> ResetPasswordAsync(Guid performingUserKey, Guid userKey);

    Task<UserClientCredentialsOperationStatus> AddClientIdAsync(Guid userKey, string clientId);

    Task<bool> RemoveClientIdAsync(Guid userKey, string clientId);

    Task<IUser?> FindByClientIdAsync(string clientId);

    Task<IEnumerable<string>> GetClientIdsAsync(Guid userKey);
}
