using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IUserRepository : IRepositoryQueryable<int, IUser>
    {
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

        /// <summary>
        /// Gets paged member results
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        IEnumerable<IUser> GetPagedResultsByQuery(IQuery<IUser> query, int pageIndex, int pageSize, out int totalRecords, Expression<Func<IUser, string>> orderBy);
        
        
        /// <summary>
        /// Gets the user permissions for the specified entities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        IEnumerable<EntityPermission> GetUserPermissionsForEntities(int userId, params int[] entityIds);

        /// <summary>
        /// Replaces the same permission set for a single user to any number of entities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissions"></param>
        /// <param name="entityIds"></param>
        void ReplaceUserPermissions(int userId, IEnumerable<char> permissions, params int[] entityIds);

        /// <summary>
        /// Assigns the same permission set for a single user to any number of entities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permission"></param>
        /// <param name="entityIds"></param>
        void AssignUserPermission(int userId, char permission, params int[] entityIds);
    }
}