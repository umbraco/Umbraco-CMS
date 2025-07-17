using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
/// Defines methods for accessing and persisting <see cref="RepositoryCacheVersion"/> entities.
/// </summary>
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
}
