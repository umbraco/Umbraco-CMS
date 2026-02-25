using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Implements <see cref="IAsyncRepositoryCachePolicy{TEntity, TId}" /> with no caching behavior.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <remarks>
///     This policy bypasses all caching and directly delegates to the repository methods.
///     Used when caching is not desired for a particular repository.
/// </remarks>
public class AsyncNoCacheRepositoryCachePolicy<TEntity, TId> : IAsyncRepositoryCachePolicy<TEntity, TId>
    where TEntity : class, IEntity
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncNoCacheRepositoryCachePolicy{TEntity, TId}" /> class.
    /// </summary>
    private AsyncNoCacheRepositoryCachePolicy()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of the no-cache policy.
    /// </summary>
    public static AsyncNoCacheRepositoryCachePolicy<TEntity, TId> Instance { get; } = new();

    /// <inheritdoc />
    public async Task<TEntity?> GetAsync(TId? id, Func<TId?, Task<TEntity?>> performGet,  Func<Task<IEnumerable<TEntity>?>> performGetAll) =>
        await performGet(id);

    /// <inheritdoc />
    public Task<TEntity?> GetCachedAsync(TId id) => Task.FromResult<TEntity?>(null);

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(TId id, Func<TId, Task<bool>> performExists,  Func<Task<IEnumerable<TEntity>?>> performGetAll) =>
        await performExists(id);

    /// <inheritdoc />
    public async Task CreateAsync(TEntity entity, Func<TEntity, Task> persistNew) => await persistNew(entity);

    /// <inheritdoc />
    public async Task UpdateAsync(TEntity entity, Func<TEntity, Task> persistUpdated) => await persistUpdated(entity);

    /// <inheritdoc />
    public async Task DeleteAsync(TEntity entity, Func<TEntity, Task> persistDeleted) => await persistDeleted(entity);

    /// <inheritdoc />
    public async Task<TEntity[]> GetAllAsync(Func<Task<IEnumerable<TEntity>?>> performGetAll) =>
        (await performGetAll())?.ToArray() ?? Array.Empty<TEntity>();

    /// <inheritdoc />
    public async Task<TEntity[]> GetManyAsync(TId[] ids, Func<TId[], Task<IEnumerable<TEntity>?>> performGetMany,  Func<Task<IEnumerable<TEntity>?>> performGetAll) =>
        (await performGetMany(ids))?.ToArray() ?? Array.Empty<TEntity>();

    /// <inheritdoc />
    public Task ClearAllAsync() => Task.CompletedTask;
}
