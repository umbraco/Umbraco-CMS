using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    internal interface IRepositoryCachePolicy<TEntity, TId> : IDisposable
        where TEntity : class, IAggregateRoot
    {
        /// <summary>
        /// Gets an entity from the cache, else from the repository.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="repoGet">The repository method to get the entity.</param>
        /// <returns>The entity with the specified identifier, if it exits, else null.</returns>
        /// <remarks>First considers the cache then the repository.</remarks>
        TEntity Get(TId id, Func<TId, TEntity> repoGet);

        /// <summary>
        /// Gets an entity from the cache.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The entity with the specified identifier, if it is in the cache already, else null.</returns>
        /// <remarks>Does not consider the repository at all.</remarks>
        TEntity Get(TId id);

        /// <summary>
        /// Gets a value indicating whether an entity with a specified identifier exists.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="repoExists">The repository method to check for the existence of the entity.</param>
        /// <returns>A value indicating whether an entity with the specified identifier exists.</returns>
        /// <remarks>First considers the cache then the repository.</remarks>
        bool Exists(TId id, Func<TId, bool> repoExists);

        /// <summary>
        /// Creates or updates an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="repoCreateOrUpdate">The repository method to create or update the entity.</param>
        /// <remarks>Creates or updates the entity in the repository, and updates the cache accordingly.</remarks>
        void CreateOrUpdate(TEntity entity, Action<TEntity> repoCreateOrUpdate);

        /// <summary>
        /// Removes an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="repoRemove">The repository method to remove the entity.</param>
        /// <remarks>Removes the entity from the repository and clears the cache.</remarks>
        void Remove(TEntity entity, Action<TEntity> repoRemove);

        /// <summary>
        /// Gets entities.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <param name="repoGet">The repository method to get entities.</param>
        /// <returns>If <paramref name="ids"/> is empty, all entities, else the entities with the specified identifiers.</returns>
        /// <remarks>fixme explain what it should do!</remarks>
        TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> repoGet);
    }
}