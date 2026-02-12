using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Cache;

public class RepositoryCacheVersionAccessor : IRepositoryCacheVersionAccessor
{
    private readonly IRequestCache _requestCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepositoryCacheVersionRepository _repositoryCacheVersionRepository;
    private readonly ICoreScopeProvider _coreScopeProvider;
    private readonly ILogger<RepositoryCacheVersionAccessor> _logger;

    public RepositoryCacheVersionAccessor(
        IRequestCache requestCache,
        IHttpContextAccessor httpContextAccessor,
        IRepositoryCacheVersionRepository repositoryCacheVersionRepository,
        ICoreScopeProvider coreScopeProvider,
        ILogger<RepositoryCacheVersionAccessor> logger)
    {
        _requestCache = requestCache;
        _httpContextAccessor = httpContextAccessor;
        _repositoryCacheVersionRepository = repositoryCacheVersionRepository;
        _coreScopeProvider = coreScopeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the cache version for the specified cache key.
    /// </summary>
    /// <param name="cacheKey">The unique identifier for the cache entry.</param>
    /// <returns>
    /// The cache version if found, or <see langword="null"/> if the version doesn't exist or the request is a client-side request.
    /// </returns>
    /// <remarks>
    /// This method implements a two-tier caching strategy:
    /// <list type="number">
    /// <item>First checks the request cache to avoid database queries within the same request.</item>
    /// <item>If not found in request cache, queries the database and caches the result for subsequent calls.</item>
    /// </list>
    /// Client-side requests always return <see langword="null"/>
    /// to avoid unnecessary cache version lookups.
    /// </remarks>
    public async Task<RepositoryCacheVersion?> GetAsync(string cacheKey)
    {
        HttpContext? httpcontext = _httpContextAccessor.HttpContext;
        if (httpcontext?.RequestServices is not null && httpcontext.Request.IsBackOfficeRequest() is false)
        {
            _logger.LogDebug("Client side request detected, skipping cache version retrieval for key {CacheKey}", cacheKey);
            // We don't want to try and fetch version for client side requests, always assume we're in sync.
            return null;
        }

        RepositoryCacheVersion? requestCachedVersion = _requestCache.GetCacheItem<RepositoryCacheVersion>(cacheKey);
        if (requestCachedVersion is not null)
        {
            _logger.LogDebug("Cache version for key {CacheKey} found in request cache", cacheKey);
            return requestCachedVersion;
        }

        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Core.Constants.Locks.CacheVersion);

        RepositoryCacheVersion? databaseVersion = await _repositoryCacheVersionRepository.GetAsync(cacheKey);

        if (databaseVersion is null)
        {
            return databaseVersion;
        }

        _requestCache.Set(cacheKey, databaseVersion);
        return databaseVersion;
    }


    /// <inheritdoc />
    public void VersionChanged(string cacheKey)
    {
        var removed = _requestCache.Remove(cacheKey);
        if (removed is false)
        {
            _logger.LogDebug("Cache version for key {CacheKey} wasn't removed from request cache, possibly missing HTTP context", cacheKey);
        }
    }

    /// <inheritdoc />
    public void CachesSynced() => _requestCache.ClearOfType<RepositoryCacheVersion>();
}
