using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IUserGroupRepository : IRepositoryQueryable<int, IUserGroup>
    {
        /// <summary>
        /// This is useful when an entire section is removed from config
        /// </summary>
        /// <param name="sectionAlias"></param>
        IEnumerable<IUserGroup> GetGroupsAssignedToSection(string sectionAlias);

        /// <summary>
        /// Removes all users from a group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        void RemoveAllUsersFromGroup(int groupId);

        /// <summary>
        /// Adds a set of users to a group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <param name="userIds">Ids of users</param>
        void AddUsersToGroup(int groupId, int[] userIds);

        /// <summary>
        /// Gets the group permissions for the specified entities
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <param name="entityIds">Array of entity Ids</param>
        IEnumerable<EntityPermission> GetPermissionsForEntities(int groupId, params int[] entityIds);

        /// <summary>
        /// Replaces the same permission set for a single group to any number of entities
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <param name="permissions">Permissions as enumerable list of <see cref="char"/></param>
        /// <param name="entityIds">Specify the nodes to replace permissions for. If nothing is specified all permissions are removed.</param>
        void ReplaceGroupPermissions(int groupId, IEnumerable<char> permissions, params int[] entityIds);

        /// <summary>
        /// Assigns the same permission set for a single group to any number of entities
        /// </summary>
        /// <param name="groupId">Id of group</param>
        /// <param name="permission">Permissions as enumerable list of <see cref="char"/></param>
        /// <param name="entityIds">Specify the nodes to replace permissions for</param>
        void AssignGroupPermission(int groupId, char permission, params int[] entityIds);
    }
}