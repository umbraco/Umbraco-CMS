using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Provides access to repository cache version information with request-level caching.
/// </summary>
/// <remarks>
/// This accessor retrieves cache version information from the database and caches it at the request level
/// to minimize database queries. Cache versions are used to determine if cached repository data is still valid
/// in distributed environments.
/// </remarks>
public interface IRepositoryCacheVersionAccessor
{

    /// <summary>
    /// Retrieves the cache version for the specified cache key.
    /// </summary>
    /// <param name="cacheKey">The unique identifier for the cache entry.</param>
    /// <returns>
    /// The cache version if found, or <see langword="null"/> if the version doesn't exist or the request is a client-side request.
    /// </returns>
    Task<RepositoryCacheVersion?> GetAsync(string cacheKey);

    /// <summary>
    /// Notifies of a version change on a given cache key, providing the new version so internal caches
    /// can be updated in-place without a database round-trip.
    /// </summary>
    /// <param name="cacheKey">Key of the changed version.</param>
    /// <param name="newVersion">The new version GUID that was just written to the database.</param>
    void VersionChanged(string cacheKey, Guid newVersion);

    /// <summary>
    /// Notifies the accessor that caches have been synchronized and which versions were adopted.
    /// </summary>
    /// <param name="adoptedVersions">The versions the local caches now hold.</param>
    /// <remarks>
    /// The scope-level cache is moved to the adopted versions instead of being cleared: a scope holds its
    /// distributed locks for the rest of its transaction, so the data it reads cannot change under it and
    /// checking the database version again would only trigger another sync for a version published after
    /// its locks were taken. The request-level cache is cleared, so a later root scope in the same request
    /// reads the current versions again. Clearing the scope-level cache instead would let a version published
    /// mid-scope change that scope's view.
    /// </remarks>
    void CachesSynced(IEnumerable<RepositoryCacheVersion> adoptedVersions);
}
