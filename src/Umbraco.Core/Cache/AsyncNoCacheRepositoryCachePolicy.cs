using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Implements <see cref="IAsyncRepositoryCachePolicy{TEntity, TKey}" /> with no caching behavior.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the entity key.</typeparam>
/// <remarks>
///     This policy bypasses all caching and directly delegates to the repository methods.
///     Used when caching is not desired for a particular repository.
/// </remarks>
public class AsyncNoCacheRepositoryCachePolicy<TEntity, TKey> : IAsyncRepositoryCachePolicy<TEntity, TKey>
    where TEntity : class, IEntity
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncNoCacheRepositoryCachePolicy{TEntity, TKey}" /> class.
    /// </summary>
    private AsyncNoCacheRepositoryCachePolicy()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of the no-cache policy.
    /// </summary>
    public static AsyncNoCacheRepositoryCachePolicy<TEntity, TKey> Instance { get; } = new();

    /// <inheritdoc />
    public async Task<TEntity?> GetAsync(TKey? key, Func<TKey?, Task<TEntity?>> performGet, Func<Task<IEnumerable<TEntity>?>> performGetAll) =>
        await performGet(key);

    /// <inheritdoc />
    public Task<TEntity?> GetCachedAsync(TKey key) => Task.FromResult<TEntity?>(null);

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(TKey key, Func<TKey, Task<bool>> performExists, Func<Task<IEnumerable<TEntity>?>> performGetAll) =>
        await performExists(key);

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
    public async Task<TEntity[]> GetManyAsync(TKey[] keys, Func<TKey[], Task<IEnumerable<TEntity>?>> performGetMany, Func<Task<IEnumerable<TEntity>?>> performGetAll) =>
        (await performGetMany(keys))?.ToArray() ?? Array.Empty<TEntity>();

    /// <inheritdoc />
    public Task ClearAllAsync() => Task.CompletedTask;
}
