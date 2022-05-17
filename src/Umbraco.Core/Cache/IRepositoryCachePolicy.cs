using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Cache;

public interface IRepositoryCachePolicy<TEntity, TId>
    where TEntity : class, IEntity
{
    /// <summary>
    ///     Gets an entity from the cache, else from the repository.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="performGet">The repository PerformGet method.</param>
    /// <param name="performGetAll">The repository PerformGetAll method.</param>
    /// <returns>The entity with the specified identifier, if it exits, else null.</returns>
    /// <remarks>First considers the cache then the repository.</remarks>
    TEntity? Get(TId? id, Func<TId?, TEntity?> performGet, Func<TId[]?, IEnumerable<TEntity>?> performGetAll);

    /// <summary>
    ///     Gets an entity from the cache.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The entity with the specified identifier, if it is in the cache already, else null.</returns>
    /// <remarks>Does not consider the repository at all.</remarks>
    TEntity? GetCached(TId id);

    /// <summary>
    ///     Gets a value indicating whether an entity with a specified identifier exists.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="performExists">The repository PerformExists method.</param>
    /// <param name="performGetAll">The repository PerformGetAll method.</param>
    /// <returns>A value indicating whether an entity with the specified identifier exists.</returns>
    /// <remarks>First considers the cache then the repository.</remarks>
    bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>?> performGetAll);

    /// <summary>
    ///     Creates an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="persistNew">The repository PersistNewItem method.</param>
    /// <remarks>Creates the entity in the repository, and updates the cache accordingly.</remarks>
    void Create(TEntity entity, Action<TEntity> persistNew);

    /// <summary>
    ///     Updates an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="persistUpdated">The repository PersistUpdatedItem method.</param>
    /// <remarks>Updates the entity in the repository, and updates the cache accordingly.</remarks>
    void Update(TEntity entity, Action<TEntity> persistUpdated);

    /// <summary>
    ///     Removes an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="persistDeleted">The repository PersistDeletedItem method.</param>
    /// <remarks>Removes the entity from the repository and clears the cache.</remarks>
    void Delete(TEntity entity, Action<TEntity> persistDeleted);

    /// <summary>
    ///     Gets entities.
    /// </summary>
    /// <param name="ids">The identifiers.</param>
    /// <param name="performGetAll">The repository PerformGetAll method.</param>
    /// <returns>If <paramref name="ids" /> is empty, all entities, else the entities with the specified identifiers.</returns>
    /// <remarks>Get all the entities. Either from the cache or the repository depending on the implementation.</remarks>
    TEntity[] GetAll(TId[]? ids, Func<TId[]?, IEnumerable<TEntity>> performGetAll);

    /// <summary>
    ///     Clears the entire cache.
    /// </summary>
    void ClearAll();
}
