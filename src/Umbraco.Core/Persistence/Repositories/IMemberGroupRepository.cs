using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IMemberGroup" /> entities.
/// </summary>
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

    /// <summary>
    ///     Replaces the roles for the specified members.
    /// </summary>
    /// <param name="memberIds">The identifiers of the members.</param>
    /// <param name="roleNames">The names of the roles to assign.</param>
    void ReplaceRoles(int[] memberIds, string[] roleNames);

    /// <summary>
    ///     Assigns roles to the specified members.
    /// </summary>
    /// <param name="memberIds">The identifiers of the members.</param>
    /// <param name="roleNames">The names of the roles to assign.</param>
    void AssignRoles(int[] memberIds, string[] roleNames);

    /// <summary>
    ///     Dissociates roles from the specified members.
    /// </summary>
    /// <param name="memberIds">The identifiers of the members.</param>
    /// <param name="roleNames">The names of the roles to dissociate.</param>
    void DissociateRoles(int[] memberIds, string[] roleNames);
}
