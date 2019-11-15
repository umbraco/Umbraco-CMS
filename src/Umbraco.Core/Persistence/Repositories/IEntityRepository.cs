using NPoco;
using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IEntityRepository : IRepository
    {
        IEntitySlim Get(int id);
        IEntitySlim Get(Guid key);
        IEntitySlim Get(int id, Guid objectTypeId);
        IEntitySlim Get(Guid key, Guid objectTypeId);

        IEnumerable<IEntitySlim> GetAll(Guid objectType, params int[] ids);        
        IEnumerable<IEntitySlim> GetAll(Guid objectType, params Guid[] keys);

        /// <summary>
        /// Gets entities for a query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query);

        /// <summary>
        /// Gets entities for a query and a specific object type allowing the query to be slightly more optimized
        /// </summary>
        /// <param name="query"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        IEnumerable<IEntitySlim> GetByQuery(IQuery<IUmbracoEntity> query, Guid objectType);

        UmbracoObjectTypes GetObjectType(int id);
        UmbracoObjectTypes GetObjectType(Guid key);

        IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params int[] ids);
        IEnumerable<TreeEntityPath> GetAllPaths(Guid objectType, params Guid[] keys);

        bool Exists(int id);
        bool Exists(Guid key);

        /// <summary>
        /// Gets paged entities for a query and a subset of object types
        /// </summary>
        /// <param name="query"></param>
        /// <param name="objectTypes"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="filter"></param>
        /// <param name="ordering"></param>
        /// <param name="sqlCustomization">
        /// A callback providing the ability to customize the generated SQL used to retrieve entities
        /// </param>
        /// <returns>
        /// A collection of mixed entity types which would be of type <see cref="IEntitySlim"/>, <see cref="IDocumentEntitySlim"/>, <see cref="IMediaEntitySlim"/>,
        /// <see cref="IMemberEntitySlim"/>
        /// </returns>
        IEnumerable<IEntitySlim> GetPagedResultsByQuery(
            IQuery<IUmbracoEntity> query, Guid[] objectTypes, long pageIndex, int pageSize, out long totalRecords,
            IQuery<IUmbracoEntity> filter, Ordering ordering, Action<Sql<ISqlContext>> sqlCustomization = null);

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
        IEnumerable<IEntitySlim> GetPagedResultsByQuery(IQuery<IUmbracoEntity> query, Guid objectType, long pageIndex, int pageSize, out long totalRecords,
            IQuery<IUmbracoEntity> filter, Ordering ordering);

        
    }
}
