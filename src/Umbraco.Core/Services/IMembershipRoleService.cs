using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Services
{
    public interface IMembershipRoleService<out T>
        where T : class, IMembershipUser
    {
        void AddRole(string roleName);
        IEnumerable<string> GetAllRoles();
        IEnumerable<string> GetAllRoles(int memberId);
        IEnumerable<string> GetAllRoles(string username);
        IEnumerable<T> GetMembersInRole(string roleName);
        IEnumerable<T> FindMembersInRole(string roleName, string usernameToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);
        bool DeleteRole(string roleName, bool throwIfBeingUsed);
        void AssignRole(string username, string roleName); 
        void AssignRoles(string[] usernames, string[] roleNames);
        void DissociateRole(string username, string roleName); 
        void DissociateRoles(string[] usernames, string[] roleNames);
        void AssignRole(int memberId, string roleName); 
        void AssignRoles(int[] memberIds, string[] roleNames);
        void DissociateRole(int memberId, string roleName);
        void DissociateRoles(int[] memberIds, string[] roleNames);
    }
}