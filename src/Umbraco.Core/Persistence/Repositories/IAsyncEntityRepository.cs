using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IAsyncEntityRepository : IEntityRepository
    {
        Task<IEntitySlim?> GetAsync(int id);
        Task<IEntitySlim?> GetAsync(Guid key);
        Task<IEntitySlim?> GetAsync(int id, Guid objectTypeId);
        Task<IEntitySlim?> GetAsync(Guid key, Guid objectTypeId);

        Task<IEnumerable<IEntitySlim>> GetAllAsync(Guid objectType, params int[] ids);
        Task<IEnumerable<IEntitySlim>> GetAllAsync(Guid objectType, params Guid[] keys);

        /// <summary>
        /// Gets entities for a query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<IEnumerable<IEntitySlim>> GetByQueryAsync(IQuery<IUmbracoEntity> query);

        /// <summary>
        /// Gets entities for a query and a specific object type allowing the query to be slightly more optimized
        /// </summary>
        /// <param name="query"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        Task<IEnumerable<IEntitySlim>> GetByQueryAsync(IQuery<IUmbracoEntity> query, Guid objectType);

        Task<int> ReserveIdAsync(Guid key);

        Task<IEnumerable<TreeEntityPath>> GetAllPathsAsync(Guid objectType, params int[]? ids);
        Task<IEnumerable<TreeEntityPath>> GetAllPathsAsync(Guid objectType, params Guid[] keys);

        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(Guid key);

        /// <summary>
        /// Gets paged entities for a query and a specific object type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="objectType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="filter"></param>
        /// <param name="ordering"></param>
        /// <returns></returns>
        Task<(IEnumerable<IEntitySlim> Entities, long TotalRecords)> GetPagedResultsByQueryAsync(IQuery<IUmbracoEntity> query, Guid objectType, long pageIndex, int pageSize,
            IQuery<IUmbracoEntity>? filter, Ordering? ordering);
    }
}
