using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Manages user groups.
/// </summary>
public interface IUserGroupService
{
    /// <summary>
    /// Gets all user groups.
    /// </summary>
    /// <param name="skip">The amount of user groups to skip.</param>
    /// <param name="take">The amount of user groups to take.</param>
    /// <returns>All user groups as an enumerable list of <see cref="IUserGroup"/>.</returns>
    Task<PagedModel<IUserGroup>> GetAllAsync(int skip, int take);

    /// <summary>
    ///     Gets all UserGroups matching an ID in the parameter list.
    /// </summary>
    /// <param name="ids">Optional Ids of UserGroups to retrieve.</param>
    /// <returns>An enumerable list of <see cref="IUserGroup"/>.</returns>
    Task<IEnumerable<IUserGroup>> GetAsync(params int[] ids);

    /// <summary>
    ///     Gets all UserGroups matching an alias in the parameter list.
    /// </summary>
    /// <param name="aliases">Alias of the UserGroup to retrieve.</param>
    /// <returns>
    ///     <returns>An enumerable list of <see cref="IUserGroup"/>.</returns>
    /// </returns>
    Task<IEnumerable<IUserGroup>> GetAsync(params string[] aliases);

    /// <summary>
    ///     Gets a UserGroup by its Alias
    /// </summary>
    /// <param name="alias">Name of the UserGroup to retrieve.</param>
    /// <returns>
    ///     <see cref="IUserGroup" />
    /// </returns>
    Task<IUserGroup?> GetAsync(string alias);

    /// <summary>
    ///     Gets a UserGroup by its Id
    /// </summary>
    /// <param name="id">Id of the UserGroup to retrieve.</param>
    /// <returns>
    ///     <see cref="IUserGroup" />
    /// </returns>
    Task<IUserGroup?> GetAsync(int id);

    /// <summary>
    /// Gets a UserGroup by its key
    /// </summary>
    /// <param name="key">Key of the UserGroup to retrieve.</param>
    /// <returns>
    ///     <see cref="IUserGroup" />
    /// </returns>
    Task<IUserGroup?> GetAsync(Guid key);

    Task<IEnumerable<IUserGroup>> GetAsync(IEnumerable<Guid> keys);

    /// <summary>
    /// Performs filtering for user groups
    /// </summary>
    /// <param name="userKey">The key of the performing (current) user.</param>
    /// <param name="filter">The filter to apply.</param>
    /// <param name="skip">The amount of user groups to skip.</param>
    /// <param name="take">The amount of user groups to take.</param>
    /// <returns>All matching user groups as an enumerable list of <see cref="IUserGroup"/>.</returns>
    /// <remarks>
    /// If the performing user is not an administrator, this method only returns groups that the performing user is a member of.
    /// </remarks>
    Task<Attempt<PagedModel<IUserGroup>, UserGroupOperationStatus>> FilterAsync(Guid userKey, string? filter, int skip, int take);

    /// <summary>
    /// Persists a new user group.
    /// </summary>
    /// <param name="userGroup">The user group to create.</param>
    /// <param name="userKey">The key of the user responsible for creating the group.</param>
    /// <param name="groupMembersKeys">The keys of the users that should be part of the group when created.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<IUserGroup, UserGroupOperationStatus>> CreateAsync(IUserGroup userGroup, Guid userKey, Guid[]? groupMembersKeys = null);

    /// <summary>
    /// Updates an existing user group.
    /// </summary>
    /// <param name="userGroup">The user group to update.</param>
    /// <param name="userKey">The ID of the user responsible for updating the group.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<IUserGroup, UserGroupOperationStatus>> UpdateAsync(IUserGroup userGroup, Guid userKey);

    /// <summary>
    ///     Deletes a UserGroup
    /// </summary>
    /// <param name="userGroupKeys">The keys of the user groups to delete.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<UserGroupOperationStatus>> DeleteAsync(ISet<Guid> userGroupKeys);

    Task<Attempt<UserGroupOperationStatus>> DeleteAsync(Guid userGroupKey) => DeleteAsync(new HashSet<Guid> { userGroupKey });

    /// <summary>
    /// Updates the users to have the groups specified.
    /// </summary>
    /// <param name="userGroupKeys">The user groups the users should be part of.</param>
    /// <param name="userKeys">The user whose groups we want to alter.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<UserGroupOperationStatus>> UpdateUserGroupsOnUsersAsync(ISet<Guid> userGroupKeys, ISet<Guid> userKeys);

    Task<Attempt<UserGroupOperationStatus>> AddUsersToUserGroupAsync(UsersToUserGroupManipulationModel addUsersModel, Guid performingUserKey);
    Task<Attempt<UserGroupOperationStatus>> RemoveUsersFromUserGroupAsync(UsersToUserGroupManipulationModel removeUsersModel, Guid performingUserKey);
}
