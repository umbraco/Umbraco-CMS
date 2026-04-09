using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IMember" /> entities.
/// </summary>
public interface IMemberRepository : IContentRepository<int, IMember>
{
    /// <summary>
    ///     Gets member identifiers by their names.
    /// </summary>
    /// <param name="names">The names of the members.</param>
    /// <returns>An array of member identifiers.</returns>
    int[] GetMemberIds(string[] names);

    /// <summary>
    ///     Gets a member by their username.
    /// </summary>
    /// <param name="username">The username of the member.</param>
    /// <returns>The member if found; otherwise, <c>null</c>.</returns>
    IMember? GetByUsername(string? username);

    /// <summary>
    ///     Finds members in a given role
    /// </summary>
    /// <param name="roleName"></param>
    /// <param name="usernameToMatch"></param>
    /// <param name="matchType"></param>
    /// <returns></returns>
    IEnumerable<IMember> FindMembersInRole(string roleName, string usernameToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

    /// <summary>
    ///     Get all members in a specific group
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    IEnumerable<IMember> GetByMemberGroup(string groupName);

    /// <summary>
    ///     Checks if a member with the username exists
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    bool Exists(string username);

    /// <summary>
    ///     Gets the count of items based on a complex query
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    int GetCountByQuery(IQuery<IMember>? query);

    /// <summary>
    ///     Gets paged members matching the specified filter.
    /// </summary>
    /// <param name="memberFilter">The filter to apply.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <param name="ordering">The ordering to apply, or <c>null</c> for default ordering.</param>
    /// <returns>A paged model of members.</returns>
    Task<PagedModel<IMember>> GetPagedByFilterAsync(MemberFilter memberFilter,int skip, int take, Ordering? ordering = null);

    /// <summary>
    /// Saves only the properties related to login for the member, using an optimized, non-locking update.
    /// </summary>
    /// <param name="member">The member to update.</param>
    /// <returns>Used to avoid the full save of the member object after a login operation.</returns>
    Task UpdateLoginPropertiesAsync(IMember member) => Task.CompletedTask;
}
