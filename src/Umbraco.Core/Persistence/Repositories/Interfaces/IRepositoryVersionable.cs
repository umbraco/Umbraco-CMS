using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Defines the implementation of a Repository, which allows getting versions of an <see cref="TEntity"/>
    /// </summary>
    /// <typeparam name="TEntity">Type of <see cref="IAggregateRoot"/> entity for which the repository is used</typeparam>
    /// <typeparam name="TId">Type of the Id used for this entity</typeparam>
    public interface IRepositoryVersionable<TId, TEntity> : IQueryRepository<TId, TEntity>
        where TEntity : IAggregateRoot
    {
        /// <summary>
        /// Get the total count of entities
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <returns></returns>
        int Count(string contentTypeAlias = null);

        int CountChildren(int parentId, string contentTypeAlias = null);

        int CountDescendants(int parentId, string contentTypeAlias = null);

        /// <summary>
        /// Gets a list of all versions for an <see cref="TEntity"/> ordered so latest is first
        /// </summary>
        /// <param name="nodeId">Id of the <see cref="TEntity"/> to retrieve versions from</param>
        /// <returns>An enumerable list of the same <see cref="TEntity"/> object with different versions</returns>
        IEnumerable<TEntity> GetAllVersions(int nodeId);

        /// <summary>
        /// Gets a list of all version idenfitifers of an entity.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="topRows">The maximum number of rows to return</param>
        IEnumerable<Guid> GetVersionIds(int id, int topRows);

        /// <summary>
        /// Gets a specific version of an entity.
        /// </summary>
        /// <param name="versionId">The identifier of the version.</param>
        TEntity GetVersion(Guid versionId);

        /// <summary>
        /// Deletes a specific version of an entity.
        /// </summary>
        /// <param name="versionId">The identifier of the version to delete.</param>
        void DeleteVersion(Guid versionId);

        /// <summary>
        /// Deletes all versions of an entity, older than a date.
        /// </summary>
        /// <param name="nodeId">The identifier of the entity.</param>
        /// <param name="versionDate">The date.</param>
        void DeleteVersions(int nodeId, DateTime versionDate);

        /// <summary>
        /// Gets paged content results.
        /// </summary>
        IEnumerable<TEntity> GetPagedResultsByQuery(IQuery<TEntity> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<TEntity> filter = null);
    }
}
