using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMemberGroupRepository : IRepositoryQueryable<int, IMemberGroup>
    {
        /// <summary>
        /// Gets a member group by it's name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IMemberGroup GetByName(string name);

        /// <summary>
        /// Creates the new member group if it doesn't already exist
        /// </summary>
        /// <param name="roleName"></param>
        IMemberGroup CreateIfNotExists(string roleName);

        /// <summary>
        /// Returns the member groups for a given member
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        IEnumerable<IMemberGroup> GetMemberGroupsForMember(int memberId);

        /// <summary>
        /// Returns the member groups for a given member
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        IEnumerable<IMemberGroup> GetMemberGroupsForMember(string username);

        void AssignRoles(string[] usernames, string[] roleNames);

        void DissociateRoles(string[] usernames, string[] roleNames);

        void AssignRoles(int[] memberIds, string[] roleNames);

        void DissociateRoles(int[] memberIds, string[] roleNames);
    }
}