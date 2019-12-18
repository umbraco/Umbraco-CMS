using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMemberRepository : IContentRepository<int, IMember>
    {
        /// <summary>
        /// Finds members in a given role
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="usernameToMatch"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        IEnumerable<IMember> FindMembersInRole(string roleName, string usernameToMatch, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

        /// <summary>
        /// Get all members in a specific group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetByMemberGroup(string groupName);

        /// <summary>
        /// Checks if a member with the username exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        bool Exists(string username);

        /// <summary>
        /// Gets the count of items based on a complex query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetCountByQuery(IQuery<IMember> query);
    }
}
