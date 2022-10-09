using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IMemberRepository : IContentRepository<int, IMember>
{
    int[] GetMemberIds(string[] names);

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
    ///     Sets a members last login date based on their username
    /// </summary>
    /// <param name="username"></param>
    /// <param name="date"></param>
    /// <remarks>
    ///     This is a specialized method because whenever a member logs in, the membership provider requires us to set the
    ///     'online' which requires
    ///     updating their login date. This operation must be fast and cannot use database locks which is fine if we are only
    ///     executing a single query
    ///     for this data since there won't be any other data contention issues.
    /// </remarks>
    [Obsolete(
        "This is now a NoOp since last login date is no longer an umbraco property, set the date on the IMember directly and Save it instead, scheduled for removal in V11.")]
    void SetLastLogin(string username, DateTime date);
}
