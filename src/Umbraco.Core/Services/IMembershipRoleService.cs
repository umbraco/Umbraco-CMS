using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the MembershipRoleService, which provides role management operations for membership users.
/// </summary>
/// <typeparam name="T">The type of membership user, must implement <see cref="IMembershipUser"/>.</typeparam>
public interface IMembershipRoleService<out T>
    where T : class, IMembershipUser
{
    /// <summary>
    ///     Adds a new role with the specified name.
    /// </summary>
    /// <param name="roleName">The name of the role to add.</param>
    void AddRole(string roleName);

    /// <summary>
    ///     Gets all roles as <see cref="IMemberGroup"/> objects.
    /// </summary>
    /// <returns>An enumerable collection of all <see cref="IMemberGroup"/> objects.</returns>
    IEnumerable<IMemberGroup> GetAllRoles();

    /// <summary>
    ///     Gets all role names for a member by their integer id.
    /// </summary>
    /// <param name="memberId">The integer id of the member.</param>
    /// <returns>An enumerable collection of role names assigned to the member.</returns>
    IEnumerable<string> GetAllRoles(int memberId);

    /// <summary>
    ///     Gets all role names for a member by their username.
    /// </summary>
    /// <param name="username">The username of the member.</param>
    /// <returns>An enumerable collection of role names assigned to the member.</returns>
    IEnumerable<string> GetAllRoles(string username);

    /// <summary>
    ///     Gets the integer ids of all roles.
    /// </summary>
    /// <returns>An enumerable collection of all role ids.</returns>
    IEnumerable<int> GetAllRolesIds();

    /// <summary>
    ///     Gets the integer ids of all roles for a member by their integer id.
    /// </summary>
    /// <param name="memberId">The integer id of the member.</param>
    /// <returns>An enumerable collection of role ids assigned to the member.</returns>
    IEnumerable<int> GetAllRolesIds(int memberId);

    /// <summary>
    ///     Gets the integer ids of all roles for a member by their username.
    /// </summary>
    /// <param name="username">The username of the member.</param>
    /// <returns>An enumerable collection of role ids assigned to the member.</returns>
    IEnumerable<int> GetAllRolesIds(string username);

    /// <summary>
    ///     Gets all members that belong to a specific role.
    /// </summary>
    /// <param name="roleName">The name of the role.</param>
    /// <returns>An enumerable collection of members in the specified role.</returns>
    IEnumerable<T> GetMembersInRole(string roleName);

    /// <summary>
    ///     Finds members in a role whose username matches the specified pattern.
    /// </summary>
    /// <param name="roleName">The name of the role to search within.</param>
    /// <param name="usernameToMatch">The username pattern to match.</param>
    /// <param name="matchType">The type of string matching to use. Default is <see cref="StringPropertyMatchType.StartsWith"/>.</param>
    /// <returns>An enumerable collection of members matching the criteria.</returns>
    IEnumerable<T> FindMembersInRole(string roleName, string usernameToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

    /// <summary>
    ///     Deletes a role by its name.
    /// </summary>
    /// <param name="roleName">The name of the role to delete.</param>
    /// <param name="throwIfBeingUsed">If <c>true</c>, throws an exception if the role is currently assigned to any members.</param>
    /// <returns><c>true</c> if the role was successfully deleted; otherwise, <c>false</c>.</returns>
    bool DeleteRole(string roleName, bool throwIfBeingUsed);

    /// <summary>
    ///     Assigns a single role to a member by username.
    /// </summary>
    /// <param name="username">The username of the member.</param>
    /// <param name="roleName">The name of the role to assign.</param>
    void AssignRole(string username, string roleName);

    /// <summary>
    ///     Assigns multiple roles to multiple members by their usernames.
    /// </summary>
    /// <param name="usernames">An array of usernames to assign roles to.</param>
    /// <param name="roleNames">An array of role names to assign.</param>
    void AssignRoles(string[] usernames, string[] roleNames);

    /// <summary>
    ///     Removes a single role from a member by username.
    /// </summary>
    /// <param name="username">The username of the member.</param>
    /// <param name="roleName">The name of the role to remove.</param>
    void DissociateRole(string username, string roleName);

    /// <summary>
    ///     Removes multiple roles from multiple members by their usernames.
    /// </summary>
    /// <param name="usernames">An array of usernames to remove roles from.</param>
    /// <param name="roleNames">An array of role names to remove.</param>
    void DissociateRoles(string[] usernames, string[] roleNames);

    /// <summary>
    ///     Assigns a single role to a member by their integer id.
    /// </summary>
    /// <param name="memberId">The integer id of the member.</param>
    /// <param name="roleName">The name of the role to assign.</param>
    void AssignRole(int memberId, string roleName);

    /// <summary>
    ///     Assigns multiple roles to multiple members by their integer ids.
    /// </summary>
    /// <param name="memberIds">An array of member ids to assign roles to.</param>
    /// <param name="roleNames">An array of role names to assign.</param>
    void AssignRoles(int[] memberIds, string[] roleNames);

    /// <summary>
    ///     Removes a single role from a member by their integer id.
    /// </summary>
    /// <param name="memberId">The integer id of the member.</param>
    /// <param name="roleName">The name of the role to remove.</param>
    void DissociateRole(int memberId, string roleName);

    /// <summary>
    ///     Removes multiple roles from multiple members by their integer ids.
    /// </summary>
    /// <param name="memberIds">An array of member ids to remove roles from.</param>
    /// <param name="roleNames">An array of role names to remove.</param>
    void DissociateRoles(int[] memberIds, string[] roleNames);

    /// <summary>
    ///     Replaces all existing roles for the specified members with the given roles, identified by usernames.
    /// </summary>
    /// <param name="usernames">An array of usernames whose roles will be replaced.</param>
    /// <param name="roleNames">An array of role names to assign as the new roles.</param>
    void ReplaceRoles(string[] usernames, string[] roleNames);

    /// <summary>
    ///     Replaces all existing roles for the specified members with the given roles, identified by member ids.
    /// </summary>
    /// <param name="memberIds">An array of member ids whose roles will be replaced.</param>
    /// <param name="roleNames">An array of role names to assign as the new roles.</param>
    void ReplaceRoles(int[] memberIds, string[] roleNames);
}
