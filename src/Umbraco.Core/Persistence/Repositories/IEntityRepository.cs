using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IEntityRepository : IRepository
    {
        IUmbracoEntity GetByKey(Guid key);
        IUmbracoEntity GetByKey(Guid key, Guid objectTypeId);
        IUmbracoEntity Get(int id);
        IUmbracoEntity Get(int id, Guid objectTypeId);
        IEnumerable<IUmbracoEntity> GetAll(Guid objectType, params int[] ids);
        IEnumerable<IUmbracoEntity> GetAll(Guid objectType, params Guid[] keys);
        IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query);
        IEnumerable<IUmbracoEntity> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectType);

        UmbracoObjectTypes GetObjectType(int id);
        UmbracoObjectTypes GetObjectType(Guid key);

        IEnumerable<EntityPath> GetAllPaths(Guid objectType, params int[] ids);
        IEnumerable<EntityPath> GetAllPaths(Guid objectType, params Guid[] keys);

        /// <summary>
        /// Gets paged results
        /// </summary>
        /// <param name="query">Query to excute</param>
        /// <param name="objectType"></param>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records query would return without paging</param>
        /// <param name="orderBy">Field to order by</param>
        /// <param name="orderDirection">Direction to order by</param>
        /// <param name="filter"></param>
        /// <returns>An Enumerable list of <see cref="IUmbracoEntity"/> objects</returns>
        IEnumerable<IUmbracoEntity> GetPagedResultsByQuery(IQuery<IUmbracoEntity> query, Guid objectType, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, IQuery<IUmbracoEntity> filter = null);

        /// <summary>
        /// Returns true if the entity exists
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exists(Guid key);

        /// <summary>
        /// Returns true if the entity exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exists(int id);

    }
}
