﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Defines the implementation of a Repository, which allows getting versions of an <see cref="TEntity"/>
    /// </summary>
    /// <typeparam name="TEntity">Type of <see cref="IAggregateRoot"/> entity for which the repository is used</typeparam>
    /// <typeparam name="TId">Type of the Id used for this entity</typeparam>
    public interface IRepositoryVersionable<TId, TEntity> : IRepositoryQueryable<TId, TEntity>
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
        /// Gets a list of all versions for an <see cref="TEntity"/>.
        /// </summary>
        /// <param name="id">Id of the <see cref="TEntity"/> to retrieve versions from</param>
        /// <returns>An enumerable list of the same <see cref="TEntity"/> object with different versions</returns>
        IEnumerable<TEntity> GetAllVersions(int id);

        /// <summary>
        /// Gets a specific version of an <see cref="TEntity"/>.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="TEntity"/> item</returns>
        TEntity GetByVersion(Guid versionId);

        /// <summary>
        /// Deletes a specific version from an <see cref="TEntity"/> object.
        /// </summary>
        /// <param name="versionId">Id of the version to delete</param>
        void DeleteVersion(Guid versionId);

        /// <summary>
        /// Deletes versions from an <see cref="TEntity"/> object prior to a specific date.
        /// </summary>
        /// <param name="id">Id of the <see cref="TEntity"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        void DeleteVersions(int id, DateTime versionDate);
    }
}