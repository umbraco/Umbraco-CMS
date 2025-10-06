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
    public Task<RepositoryCacheVersion?> GetAsync(string cacheKey);
}
