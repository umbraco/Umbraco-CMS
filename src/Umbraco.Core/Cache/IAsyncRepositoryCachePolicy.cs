using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Defines a repository cache policy for managing cached entities.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity key.</typeparam>
/// <remarks>
///     Repository cache policies control how repositories interact with caches,
///     determining when to read from cache, when to populate cache, and how to
///     keep caches in sync with the underlying data store.
/// </remarks>
public interface IAsyncRepositoryCachePolicy<TEntity, TKey>
    where TEntity : class, IEntity
{
    /// <summary>
    ///     Gets an entity from the cache, else from the repository.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="performGet">The repository PerformGet method.</param>
    /// <param name="performGetAll">The repository PerformGetAll method.</param>
    /// <returns>The entity with the specified key, if it exists, else null.</returns>
    /// <remarks>First considers the cache then the repository.</remarks>
    Task<TEntity?> GetAsync(TKey? key, Func<TKey?, Task<TEntity?>> performGet, Func<Task<IEnumerable<TEntity>?>> performGetAll);

    /// <summary>
    ///     Gets an entity from the cache.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The entity with the specified key, if it is in the cache already, else null.</returns>
    /// <remarks>Does not consider the repository at all.</remarks>
    Task<TEntity?> GetCachedAsync(TKey key);

    /// <summary>
    ///     Gets a value indicating whether an entity with a specified key exists.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="performExists">The repository PerformExists method.</param>
    /// <param name="performGetAll">The repository PerformGetAll method.</param>
    /// <returns>A value indicating whether an entity with the specified key exists.</returns>
    /// <remarks>First considers the cache then the repository.</remarks>
    Task<bool> ExistsAsync(TKey key, Func<TKey, Task<bool>> performExists, Func<Task<IEnumerable<TEntity>?>> performGetAll);

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
    /// <param name="performGetAll">The repository PerformGetAll method.</param>
    /// <returns> Returns all entities.</returns>
    /// <remarks>Get all the entities. Either from the cache or the repository depending on the implementation.</remarks>
    Task<TEntity[]> GetAllAsync(Func<Task<IEnumerable<TEntity>?>> performGetAll);

    /// <summary>
    ///     Gets many entities.
    /// </summary>
    /// <param name="keys">The keys of the entities to retrieve.</param>
    /// <param name="performGetMany">The repository PerformGetMany method.</param>
    /// <param name="performGetAll">The repository PerformGetAll method.</param>
    /// <returns>The entities matching the specified keys.</returns>
    /// <remarks>Get the entities. Either from the cache or the repository depending on the implementation.</remarks>
    Task<TEntity[]> GetManyAsync(TKey[] keys, Func<TKey[], Task<IEnumerable<TEntity>?>> performGetMany, Func<Task<IEnumerable<TEntity>?>> performGetAll);

    /// <summary>
    ///     Clears the entire cache.
    /// </summary>
    Task ClearAllAsync();
}
