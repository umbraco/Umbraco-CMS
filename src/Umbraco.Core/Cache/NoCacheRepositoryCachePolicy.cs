using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Implements <see cref="IRepositoryCachePolicy{TEntity, TId}" /> with no caching behavior.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
/// <remarks>
///     This policy bypasses all caching and directly delegates to the repository methods.
///     Used when caching is not desired for a particular repository.
/// </remarks>
public class NoCacheRepositoryCachePolicy<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId>
    where TEntity : class, IEntity
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NoCacheRepositoryCachePolicy{TEntity, TId}" /> class.
    /// </summary>
    private NoCacheRepositoryCachePolicy()
    {
    }

    /// <summary>
    ///     Gets the singleton instance of the no-cache policy.
    /// </summary>
    public static NoCacheRepositoryCachePolicy<TEntity, TId> Instance { get; } = new();

    /// <inheritdoc />
    public TEntity? Get(TId? id, Func<TId?, TEntity?> performGet, Func<TId[]?, IEnumerable<TEntity>?> performGetAll) =>
        performGet(id);

    /// <inheritdoc />
    public TEntity? GetCached(TId id) => null;

    /// <inheritdoc />
    public bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>?> performGetAll) =>
        performExists(id);

    /// <inheritdoc />
    public void Create(TEntity entity, Action<TEntity> persistNew) => persistNew(entity);

    /// <inheritdoc />
    public void Update(TEntity entity, Action<TEntity> persistUpdated) => persistUpdated(entity);

    /// <inheritdoc />
    public void Delete(TEntity entity, Action<TEntity> persistDeleted) => persistDeleted(entity);

    /// <inheritdoc />
    public TEntity[] GetAll(TId[]? ids, Func<TId[]?, IEnumerable<TEntity>?> performGetAll) =>
        performGetAll(ids)?.ToArray() ?? Array.Empty<TEntity>();

    /// <inheritdoc />
    public void ClearAll()
    {
    }
}
