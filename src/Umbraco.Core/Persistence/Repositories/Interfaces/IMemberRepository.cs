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

        IEnumerable<IMember> GetMembersByEmails(params string[] emails);
    }
}