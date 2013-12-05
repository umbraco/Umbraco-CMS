using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IMemberRepository : IRepositoryVersionable<int, IMember>
    {
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

    }
}