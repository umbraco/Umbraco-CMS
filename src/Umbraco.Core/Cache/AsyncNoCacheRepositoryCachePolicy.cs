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
    public async Task<TEntity?> Get(TId? id, Func<TId?, Task<TEntity?>> performGet, Func<TId[]?, Task<IEnumerable<TEntity>?>> performGetAll) =>
        await performGet(id);

    /// <inheritdoc />
    public Task<TEntity?> GetCached(TId id) => Task.FromResult<TEntity?>(null);

    /// <inheritdoc />
    public async Task<bool> Exists(TId id, Func<TId, Task<bool>> performExists, Func<TId[], Task<IEnumerable<TEntity>?>> performGetAll) =>
        await performExists(id);

    /// <inheritdoc />
    public async Task Create(TEntity entity, Func<TEntity, Task> persistNew) => await persistNew(entity);

    /// <inheritdoc />
    public async Task Update(TEntity entity, Func<TEntity, Task> persistUpdated) => await persistUpdated(entity);

    /// <inheritdoc />
    public async Task Delete(TEntity entity, Func<TEntity, Task> persistDeleted) => await persistDeleted(entity);

    /// <inheritdoc />
    public async Task<TEntity[]> GetAll(TId[]? ids, Func<TId[]?, Task<IEnumerable<TEntity>?>> performGetAll) =>
        (await performGetAll(ids))?.ToArray() ?? Array.Empty<TEntity>();

    /// <inheritdoc />
    public Task ClearAll() => Task.CompletedTask;
}
