using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

public interface IMembershipRoleService<out T>
    where T : class, IMembershipUser
{
    void AddRole(string roleName);

    IEnumerable<IMemberGroup> GetAllRoles();

    IEnumerable<string> GetAllRoles(int memberId);

    IEnumerable<string> GetAllRoles(string username);

    IEnumerable<int> GetAllRolesIds();

    IEnumerable<int> GetAllRolesIds(int memberId);

    IEnumerable<int> GetAllRolesIds(string username);

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

    void ReplaceRoles(string[] usernames, string[] roleNames);

    void ReplaceRoles(int[] memberIds, string[] roleNames);
}
