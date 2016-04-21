using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMemberRepository : IRepositoryVersionable<int, IMember>, IDeleteMediaFilesRepository
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

        /// <summary>
        /// Gets paged member results
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <param name="orderBy">The order by column</param>
        /// <param name="orderDirection">The order direction.</param>
        /// <param name="orderBySystemField">Flag to indicate when ordering by system field</param>
        /// <param name="filter">Search query</param>
        /// <returns></returns>
        IEnumerable<IMember> GetPagedResultsByQuery(IQuery<IMember> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, string filter = "");

        //IEnumerable<IMember> GetPagedResultsByQuery<TDto>(
        //    Sql sql, int pageIndex, int pageSize, out int totalRecords,
        //    Func<IEnumerable<TDto>, int[]> resolveIds);

        /// <summary>
        /// Used to add/update published xml for the media item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        void AddOrUpdateContentXml(IMember content, Func<IMember, XElement> xml);

        /// <summary>
        /// Used to add/update preview xml for the content item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="xml"></param>
        void AddOrUpdatePreviewXml(IMember content, Func<IMember, XElement> xml);

    }
}