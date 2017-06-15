using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
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
        /// Gets a list of <see cref="IUser"/> objects associated with a given group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        IEnumerable<IUser> GetAllInGroup(int groupId);

        /// <summary>
        /// Gets a list of <see cref="IUser"/> objects not associated with a given group
        /// </summary>
        /// <param name="groupId">Id of group</param>
        IEnumerable<IUser> GetAllNotInGroup(int groupId);

        [Obsolete("Use the overload with long operators instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerable<IUser> GetPagedResultsByQuery(IQuery<IUser> query, int pageIndex, int pageSize, out int totalRecords, Expression<Func<IUser, string>> orderBy);

        /// <summary>
        /// Gets paged user results
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <param name="orderDirection"></param>
        /// <param name="userGroups">Optional parameter to filter by specified user groups</param>
        /// <param name="userState">Optional parameter to filter by specfied user state</param>
        /// <returns></returns>
        IEnumerable<IUser> GetPagedResultsByQuery(IQuery<IUser> query, long pageIndex, int pageSize, out long totalRecords, Expression<Func<IUser, object>> orderBy, Direction orderDirection, string[] userGroups = null, UserState? userState = null, IQuery<IUser> filter = null);

        IProfile GetProfile(string username);
        IProfile GetProfile(int id);
        IDictionary<UserState, int> GetUserStates();
    }
}