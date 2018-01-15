using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Defines the base implementation of a repository for content items.
    /// </summary>
    public interface IContentRepository<in TId, TEntity> : IReadWriteQueryRepository<TId, TEntity>
        where TEntity : IUmbracoEntity
    {
        /// <summary>
        /// Gets versions.
        /// </summary>
        /// <remarks>Current version is first, and then versions are ordered with most recent first.</remarks>
        IEnumerable<TEntity> GetAllVersions(int nodeId);

        /// <summary>
        /// Gets version identifiers.
        /// </summary>
        /// <remarks>Current version is first, and then versions are ordered with most recent first.</remarks>
        IEnumerable<int> GetVersionIds(int id, int topRows);

        /// <summary>
        /// Gets a version.
        /// </summary>
        TEntity GetVersion(int versionId);

        /// <summary>
        /// Deletes a version.
        /// </summary>
        void DeleteVersion(int versionId);

        /// <summary>
        /// Deletes all versions older than a date.
        /// </summary>
        void DeleteVersions(int nodeId, DateTime versionDate);

        /// <summary>
        /// Gets the recycle bin identifier.
        /// </summary>
        int RecycleBinId { get; }

        /// <summary>
        /// Gets the recycle bin content.
        /// </summary>
        IEnumerable<TEntity> GetRecycleBin();

        /// <summary>
        /// Gets the count of content items of a given content type.
        /// </summary>
        int Count(string contentTypeAlias = null);

        /// <summary>
        /// Gets the count of child content items of a given parent content, of a given content type.
        /// </summary>
        int CountChildren(int parentId, string contentTypeAlias = null);

        /// <summary>
        /// Gets the count of descendant content items of a given parent content, of a given content type.
        /// </summary>
        int CountDescendants(int parentId, string contentTypeAlias = null);

        /// <summary>
        /// Gets paged content items.
        /// </summary>
        IEnumerable<TEntity> GetPage(IQuery<TEntity> query, long pageIndex, int pageSize, out long totalRecords,
            string orderBy, Direction orderDirection, bool orderBySystemField, IQuery<TEntity> filter = null);
    }
}
