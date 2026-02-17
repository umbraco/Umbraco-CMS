using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Defines a repository cache policy for managing cached entities.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <remarks>
///     Repository cache policies control how repositories interact with caches,
///     determining when to read from cache, when to populate cache, and how to
///     keep caches in sync with the underlying data store.
/// </remarks>
public interface IAsyncRepositoryCachePolicy<TEntity, TId>
    where TEntity : class, IEntity
{
    /// <summary>
    ///     Gets an entity from the cache, else from the repository.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="performGet">The repository PerformGet method.</param>
    /// <returns>The entity with the specified identifier, if it exists, else null.</returns>
    /// <remarks>First considers the cache then the repository.</remarks>
    Task<TEntity?> GetAsync(TId? id, Func<TId?, Task<TEntity?>> performGet);

    /// <summary>
    ///     Gets an entity from the cache.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The entity with the specified identifier, if it is in the cache already, else null.</returns>
    /// <remarks>Does not consider the repository at all.</remarks>
    Task<TEntity?> GetCachedAsync(TId id);

    /// <summary>
    ///     Gets a value indicating whether an entity with a specified identifier exists.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="performExists">The repository PerformExists method.</param>
    /// <returns>A value indicating whether an entity with the specified identifier exists.</returns>
    /// <remarks>First considers the cache then the repository.</remarks>
    Task<bool> ExistsAsync(TId id, Func<TId, Task<bool>> performExists);

    /// <summary>
    ///     Creates an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="persistNew">The repository PersistNewItem method.</param>
    /// <remarks>Creates the entity in the repository, and updates the cache accordingly.</remarks>
    Task CreateAsync(TEntity entity, Func<TEntity, Task> persistNew);

    /// <summary>
    ///     Updates an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="persistUpdated">The repository PersistUpdatedItem method.</param>
    /// <remarks>Updates the entity in the repository, and updates the cache accordingly.</remarks>
    Task UpdateAsync(TEntity entity, Func<TEntity, Task> persistUpdated);

    /// <summary>
    ///     Removes an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="persistDeleted">The repository PersistDeletedItem method.</param>
    /// <remarks>Removes the entity from the repository and clears the cache.</remarks>
    Task DeleteAsync(TEntity entity, Func<TEntity, Task> persistDeleted);

    /// <summary>
    ///     Gets entities.
    /// </summary>
    /// <param name="ids">The identifiers.</param>
    /// <param name="performGetAll">The repository PerformGetAll method.</param>
    /// <returns>If <paramref name="ids" /> is empty, all entities, else the entities with the specified identifiers.</returns>
    /// <remarks>Get all the entities. Either from the cache or the repository depending on the implementation.</remarks>
    Task<TEntity[]> GetAllAsync(TId[]? ids, Func<TId[]?, Task<IEnumerable<TEntity>?>> performGetAll);

    /// <summary>
    ///     Clears the entire cache.
    /// </summary>
    Task ClearAllAsync();
}
