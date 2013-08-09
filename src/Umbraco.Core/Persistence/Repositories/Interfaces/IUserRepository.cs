using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IUserRepository : IRepositoryQueryable<int, IUser>
    {
        //IProfile GetProfileById(int id);
        IProfile GetProfileByUserName(string username);
        IUser GetUserByUserName(string username);
        
        /// <summary>
        /// This is useful when an entire section is removed from config
        /// </summary>
        /// <param name="sectionAlias"></param>
        IEnumerable<IUser> GetUsersAssignedToSection(string sectionAlias);

        /// <summary>
        /// Gets the user permissions for the specified entities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        IEnumerable<EntityPermission> GetUserPermissionsForEntities(object userId, params int[] entityIds);

    }
}