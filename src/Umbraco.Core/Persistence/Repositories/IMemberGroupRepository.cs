using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IMemberGroupRepository : IReadWriteQueryRepository<int, IMemberGroup>
{
    /// <summary>
    ///     Gets a member group by it's uniqueId
    /// </summary>
    /// <param name="uniqueId"></param>
    /// <returns></returns>
    IMemberGroup? Get(Guid uniqueId);

    /// <summary>
    ///     Gets a member group by it's name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IMemberGroup? GetByName(string? name);

    /// <summary>
    ///     Creates the new member group if it doesn't already exist
    /// </summary>
    /// <param name="roleName"></param>
    IMemberGroup? CreateIfNotExists(string roleName);

    /// <summary>
    ///     Returns the member groups for a given member
    /// </summary>
    /// <param name="memberId"></param>
    /// <returns></returns>
    IEnumerable<IMemberGroup> GetMemberGroupsForMember(int memberId);

    /// <summary>
    ///     Returns the member groups for a given member
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    IEnumerable<IMemberGroup> GetMemberGroupsForMember(string? username);

    void ReplaceRoles(int[] memberIds, string[] roleNames);

    void AssignRoles(int[] memberIds, string[] roleNames);

    void DissociateRoles(int[] memberIds, string[] roleNames);
}
