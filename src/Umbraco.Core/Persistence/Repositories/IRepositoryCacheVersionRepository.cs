using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
/// Defines methods for accessing and persisting <see cref="RepositoryCacheVersion"/> entities.
/// </summary>
/// <remarks>
/// The synchronous members exist for callers that run while a distributed lock is held: as long as the lock wait and
/// the notification pipeline are synchronous, awaiting a database call there blocks a thread-pool thread and needs a
/// second one for the continuation, which under load stretches the time the lock is held. Implementations should
/// make the synchronous members genuinely synchronous rather than wrap the asynchronous ones.
/// </remarks>
public interface IRepositoryCacheVersionRepository : IRepository
{
    /// <summary>
    /// Gets a <see cref="RepositoryCacheVersion"/> by its identifier.
    /// </summary>
    /// <param name="identifier">The unique identifier of the cache version.</param>
    /// <returns>
    /// A <see cref="RepositoryCacheVersion"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<RepositoryCacheVersion?> GetAsync(string identifier);

    /// <summary>
    /// Gets all <see cref="RepositoryCacheVersion"/> entities.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{RepositoryCacheVersion}"/> containing all cache versions.
    /// </returns>
    Task<IEnumerable<RepositoryCacheVersion>> GetAllAsync();

    /// <summary>
    /// Saves the specified <see cref="RepositoryCacheVersion"/>.
    /// </summary>
    /// <param name="repositoryCacheVersion">The cache version entity to save.</param>
    Task SaveAsync(RepositoryCacheVersion repositoryCacheVersion);

    /// <summary>
    /// Gets a <see cref="RepositoryCacheVersion"/> by its identifier without leaving the calling thread.
    /// </summary>
    /// <param name="identifier">The unique identifier of the cache version.</param>
    /// <returns>A <see cref="RepositoryCacheVersion"/> if found; otherwise, <c>null</c>.</returns>
    // TODO (V20): Remove the default implementation once locks and notification publishing are asynchronous.
    RepositoryCacheVersion? Get(string identifier)
        => GetAsync(identifier).GetAwaiter().GetResult();

    /// <summary>
    /// Gets all <see cref="RepositoryCacheVersion"/> entities without leaving the calling thread.
    /// </summary>
    /// <returns>All cache versions.</returns>
    // TODO (V20): Remove the default implementation once locks and notification publishing are asynchronous.
    IEnumerable<RepositoryCacheVersion> GetAll()
        => GetAllAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Saves the specified <see cref="RepositoryCacheVersion"/> without leaving the calling thread.
    /// </summary>
    /// <param name="repositoryCacheVersion">The cache version entity to save.</param>
    // TODO (V20): Remove the default implementation once locks and notification publishing are asynchronous.
    void Save(RepositoryCacheVersion repositoryCacheVersion)
        => SaveAsync(repositoryCacheVersion).GetAwaiter().GetResult();
}
