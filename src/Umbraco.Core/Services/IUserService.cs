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
    /// <param name="userId">The integer id of the user.</param>
    /// <param name="requestingIpAddress">The IP address from which the login request originated.</param>
    /// <returns>A <see cref="Guid"/> representing the session id.</returns>
    Guid CreateLoginSession(int userId, string requestingIpAddress);

    /// <summary>
    ///     Validates that a user login session is valid/current and hasn't been closed
    /// </summary>
    /// <param name="userId">The integer id of the user.</param>
    /// <param name="sessionId">The session id to validate.</param>
    /// <returns><c>true</c> if the session is valid; otherwise, <c>false</c>.</returns>
    bool ValidateLoginSession(int userId, Guid sessionId);

    /// <summary>
    ///     Removes the session's validity
    /// </summary>
    /// <param name="sessionId">The session id to clear.</param>
    void ClearLoginSession(Guid sessionId);

    /// <summary>
    ///     Removes all valid sessions for the user
    /// </summary>
    /// <param name="userId">The integer id of the user.</param>
    /// <returns>The number of sessions that were cleared.</returns>
    int ClearLoginSessions(int userId);

    /// <summary>
    ///     This is basically facets of UserStates key = state, value = count
    /// </summary>
    /// <returns>A dictionary where the key is the <see cref="UserState"/> and the value is the count of users in that state.</returns>
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

    /// <summary>
    ///     Invites a new user to the system by sending an invitation email.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="model">The model containing the invitation details.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<UserInvitationResult, UserOperationStatus>> InviteAsync(Guid performingUserKey, UserInviteModel model);

    /// <summary>
    ///     Verifies an invitation token for a user.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <param name="token">The invitation token to verify.</param>
    /// <returns>An attempt indicating if the verification was successful as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<UserOperationStatus>> VerifyInviteAsync(Guid userKey, string token);

    /// <summary>
    ///     Creates an initial password for a user after invitation verification.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <param name="token">The invitation token.</param>
    /// <param name="password">The password to set for the user.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<PasswordChangedModel, UserOperationStatus>> CreateInitialPasswordAsync(Guid userKey, string token, string password);

    /// <summary>
    ///     Updates an existing user.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="model">The model containing the updated user details.</param>
    /// <returns>An attempt containing the updated <see cref="IUser"/> if successful, as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<IUser?, UserOperationStatus>> UpdateAsync(Guid performingUserKey, UserUpdateModel model);

    /// <summary>
    ///     Sets the avatar for a user from a temporary file.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <param name="temporaryFileKey">The key of the temporary file containing the avatar image.</param>
    /// <returns>A <see cref="UserOperationStatus"/> indicating the result of the operation.</returns>
    Task<UserOperationStatus> SetAvatarAsync(Guid userKey, Guid temporaryFileKey);

    /// <summary>
    ///     Deletes multiple users by their keys.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="keys">The set of user keys to delete.</param>
    /// <returns>A <see cref="UserOperationStatus"/> indicating the result of the operation.</returns>
    Task<UserOperationStatus> DeleteAsync(Guid performingUserKey, ISet<Guid> keys);

    /// <summary>
    ///     Deletes a single user by their key.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="key">The unique key of the user to delete.</param>
    /// <returns>A <see cref="UserOperationStatus"/> indicating the result of the operation.</returns>
    Task<UserOperationStatus> DeleteAsync(Guid performingUserKey, Guid key) => DeleteAsync(performingUserKey, new HashSet<Guid> { key });

    /// <summary>
    ///     Disables multiple users by their keys.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="keys">The set of user keys to disable.</param>
    /// <returns>A <see cref="UserOperationStatus"/> indicating the result of the operation.</returns>
    Task<UserOperationStatus> DisableAsync(Guid performingUserKey, ISet<Guid> keys);

    /// <summary>
    ///     Enables multiple users by their keys.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="keys">The set of user keys to enable.</param>
    /// <returns>A <see cref="UserOperationStatus"/> indicating the result of the operation.</returns>
    Task<UserOperationStatus> EnableAsync(Guid performingUserKey, ISet<Guid> keys);

    /// <summary>
    ///     Unlocks users that have been locked out.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="keys">The keys of the users to unlock.</param>
    /// <returns>An attempt containing the unlock result as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<UserUnlockResult, UserOperationStatus>> UnlockAsync(Guid performingUserKey, params Guid[] keys);

    /// <summary>
    ///     Changes a user's password.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="model">The model containing the password change details.</param>
    /// <returns>An attempt containing the password changed result as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<PasswordChangedModel, UserOperationStatus>> ChangePasswordAsync(Guid performingUserKey, ChangeUserPasswordModel model);

    /// <summary>
    ///     Clears the avatar for a user.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <returns>A <see cref="UserOperationStatus"/> indicating the result of the operation.</returns>
    Task<UserOperationStatus> ClearAvatarAsync(Guid userKey);

    /// <summary>
    ///     Gets all linked external logins for a user.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <returns>An attempt containing the collection of linked logins as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus>> GetLinkedLoginsAsync(Guid userKey);

    /// <summary>
    /// Gets all users that the requesting user is allowed to see.
    /// </summary>
    /// <param name="performingUserKey">The Key of the user requesting the users.</param>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns>All users that the user is allowed to see.</returns>
    Task<Attempt<PagedModel<IUser>?, UserOperationStatus>> GetAllAsync(Guid performingUserKey, int skip, int take) => throw new NotImplementedException();

    /// <summary>
    ///     Filters users based on the specified criteria.
    /// </summary>
    /// <param name="userKey">The key of the user performing the filter operation.</param>
    /// <param name="filter">The filter criteria to apply.</param>
    /// <param name="skip">The number of records to skip. Default is 0.</param>
    /// <param name="take">The number of records to take. Default is 100.</param>
    /// <param name="orderBy">The field to order results by. Default is <see cref="UserOrder.UserName"/>.</param>
    /// <param name="orderDirection">The direction to order results. Default is <see cref="Direction.Ascending"/>.</param>
    /// <returns>An attempt containing a paged model of filtered users as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
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
    /// <param name="pageIndex">The page index (zero-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalRecords">The total number of records found (out).</param>
    /// <param name="orderBy">The field to order by.</param>
    /// <param name="orderDirection">The direction to order by.</param>
    /// <param name="userState">Optional array of user states to filter by.</param>
    /// <param name="includeUserGroups">
    ///     A filter to only include user that belong to these user groups
    /// </param>
    /// <param name="excludeUserGroups">
    ///     A filter to only include users that do not belong to these user groups
    /// </param>
    /// <param name="filter">Optional query filter.</param>
    /// <returns>An enumerable collection of <see cref="IUser"/> objects.</returns>
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
    /// <param name="pageIndex">The page index (zero-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalRecords">The total number of records found (out).</param>
    /// <param name="orderBy">The field to order by.</param>
    /// <param name="orderDirection">The direction to order by.</param>
    /// <param name="userState">Optional array of user states to filter by.</param>
    /// <param name="userGroups">
    ///     A filter to only include user that belong to these user groups
    /// </param>
    /// <param name="filter">Optional string filter.</param>
    /// <returns>An enumerable collection of <see cref="IUser"/> objects.</returns>
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

    /// <summary>
    ///     Gets multiple users by their keys.
    /// </summary>
    /// <param name="keys">The keys of the users to retrieve.</param>
    /// <returns>An enumerable collection of <see cref="IUser"/> objects matching the specified keys.</returns>
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
    /// Get explicitly assigned document permissions for a user and node keys.
    /// </summary>
    /// <param name="userKey">Key of user to retrieve permissions for. </param>
    /// <param name="contentKeys">The keys of the content to get permissions for.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserOperationStatus"/>, and an enumerable of permissions.</returns>
    Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetDocumentPermissionsAsync(Guid userKey, IEnumerable<Guid> contentKeys);

    /// <summary>
    /// Get explicitly assigned element permissions for a user and node keys.
    /// </summary>
    /// <param name="userKey">Key of user to retrieve permissions for. </param>
    /// <param name="elementKeys">The keys of the elements to get permissions for.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserOperationStatus"/>, and an enumerable of permissions.</returns>
    Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetElementPermissionsAsync(
        Guid userKey,
        IEnumerable<Guid> elementKeys) => throw new NotImplementedException(); // TODO (V19): Remove default implementation.

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
    /// <param name="groups">The user groups to get permissions for.</param>
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
    /// <returns>An <see cref="EntityPermissionSet"/> containing the calculated permissions.</returns>
    EntityPermissionSet GetPermissionsForPath(IUser? user, string? path);

    /// <summary>
    ///     Gets the permissions for the provided groups and path
    /// </summary>
    /// <param name="groups">The user groups to get permissions for.</param>
    /// <param name="path">Path to check permissions for</param>
    /// <param name="fallbackToDefaultPermissions">
    ///     Flag indicating if we want to include the default group permissions for each result if there are not explicit
    ///     permissions set
    /// </param>
    /// <returns>An <see cref="EntityPermissionSet"/> containing the calculated permissions.</returns>
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
    /// <param name="permission">The permission to assign.</param>
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

    /// <summary>
    ///     Verifies the reset code sent from the reset password mail for a given user.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <param name="token">The reset password token.</param>
    /// <returns>An attempt indicating if the verification was successful as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<UserOperationStatus>> VerifyPasswordResetAsync(Guid userKey, string token);

    /// <summary>
    ///     Changes the user's password.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <param name="token">The reset password token.</param>
    /// <param name="password">The new password of the user.</param>
    /// <returns>An attempt containing the password changed result as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<PasswordChangedModel, UserOperationStatus>> ResetPasswordAsync(Guid userKey, string token, string password);

    /// <summary>
    ///     Sends an email with a link to reset user's password.
    /// </summary>
    /// <param name="userEmail">The email address of the user.</param>
    /// <returns>An attempt indicating if the operation was successful as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<UserOperationStatus>> SendResetPasswordEmailAsync(string userEmail);

    /// <summary>
    ///     Resends an invitation email to a user.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="model">The model containing the resend invitation details.</param>
    /// <returns>An attempt containing the invitation result as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<UserInvitationResult, UserOperationStatus>> ResendInvitationAsync(Guid performingUserKey, UserResendInviteModel model);

    /// <summary>
    ///     Resets a user's password without requiring a token.
    /// </summary>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <param name="userKey">The unique key of the user whose password will be reset.</param>
    /// <returns>An attempt containing the password changed result as well as a more detailed <see cref="UserOperationStatus"/>.</returns>
    Task<Attempt<PasswordChangedModel, UserOperationStatus>> ResetPasswordAsync(Guid performingUserKey, Guid userKey);

    /// <summary>
    ///     Adds a client ID to a user for OAuth client credentials authentication.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <param name="clientId">The client ID to add.</param>
    /// <returns>A <see cref="UserClientCredentialsOperationStatus"/> indicating the result of the operation.</returns>
    Task<UserClientCredentialsOperationStatus> AddClientIdAsync(Guid userKey, string clientId);

    /// <summary>
    ///     Removes a client ID from a user.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <param name="clientId">The client ID to remove.</param>
    /// <returns><c>true</c> if the client ID was successfully removed; otherwise, <c>false</c>.</returns>
    Task<bool> RemoveClientIdAsync(Guid userKey, string clientId);

    /// <summary>
    ///     Finds a user by their client ID.
    /// </summary>
    /// <param name="clientId">The client ID to search for.</param>
    /// <returns>The <see cref="IUser"/> associated with the client ID, or <c>null</c> if not found.</returns>
    Task<IUser?> FindByClientIdAsync(string clientId);

    /// <summary>
    ///     Gets all client IDs associated with a user.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <returns>An enumerable collection of client IDs.</returns>
    Task<IEnumerable<string>> GetClientIdsAsync(Guid userKey);
}
