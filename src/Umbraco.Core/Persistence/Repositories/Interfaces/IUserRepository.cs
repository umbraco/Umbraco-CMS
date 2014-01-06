using System.Collections.Generic;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IUserRepository : IRepositoryQueryable<int, IUser>
    {
        //IProfile GetProfileById(int id);
        //IProfile GetProfileByUserName(string username);
        //IUser GetUserByUserName(string username);

        /// <summary>
        /// Gets the count of items based on a complex query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int GetCountByQuery(IQuery<IUser> query);

        /// <summary>
        /// Checks if a user with the username exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        bool Exists(string username);

        /// <summary>
        /// This is useful when an entire section is removed from config
        /// </summary>
        /// <param name="sectionAlias"></param>
        IEnumerable<IUser> GetUsersAssignedToSection(string sectionAlias);
    }
}