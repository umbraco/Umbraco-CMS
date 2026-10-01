using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
internal class RepositoryCacheVersionService : IRepositoryCacheVersionService
{
    private const string PendingCacheKeysKey = "Umbraco.Cms.Core.Cache.RepositoryCacheVersionService.PendingCacheKeys";

    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IRepositoryCacheVersionRepository _repositoryCacheVersionRepository;
    private readonly ILogger<RepositoryCacheVersionService> _logger;
    private readonly IRepositoryCacheVersionAccessor _repositoryCacheVersionAccessor;
    private readonly IRequestCache _requestCache;
    private readonly ConcurrentDictionary<string, Guid> _cacheVersions = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentHashSet<string>> _writtenKeysByScope = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="RepositoryCacheVersionService" /> class.
    /// </summary>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="repositoryCacheVersionRepository">The repository cache version repository.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="repositoryCacheVersionAccessor">The repository cache version accessor.</param>
    /// <param name="requestCache">The request cache, which holds the cache updates deferred until the request ends.</param>
    public RepositoryCacheVersionService(
        ICoreScopeProvider scopeProvider,
        IRepositoryCacheVersionRepository repositoryCacheVersionRepository,
        ILogger<RepositoryCacheVersionService> logger,
        IRepositoryCacheVersionAccessor repositoryCacheVersionAccessor,
        IRequestCache requestCache)
    {
        _scopeProvider = scopeProvider;
        _repositoryCacheVersionRepository = repositoryCacheVersionRepository;
        _logger = logger;
        _repositoryCacheVersionAccessor = repositoryCacheVersionAccessor;
        _requestCache = requestCache;
    }

    /// <inheritdoc />
    public async Task<bool> IsCacheSyncedAsync<TEntity>()
        where TEntity : class
    {
        _logger.LogDebug("Checking if cache for {EntityType} is synced", typeof(TEntity).Name);

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);

        var cacheKey = GetCacheKey<TEntity>();

        RepositoryCacheVersion? databaseVersion = await _repositoryCacheVersionAccessor.GetAsync(cacheKey);

        if (databaseVersion?.Version is null)
        {
            _logger.LogDebug("Cache for {EntityType} has no version in the database, considering it synced", typeof(TEntity).Name);

            // If the database version is null, it means the cache has never been initialized, so we consider it synced.
            return true;
        }

        if (_cacheVersions.TryGetValue(cacheKey, out Guid localVersion) is false)
        {
            _logger.LogDebug("Cache for {EntityType} is not initialized, considering it synced", typeof(TEntity).Name);

            // We're not initialized yet, so cache is empty, which means cache is synced.
            // Since the cache is most likely no longer empty, we should set the cache version to the database version.
            _cacheVersions[cacheKey] = Guid.Parse(databaseVersion.Version);
            return true;
        }

        // We could've parsed this in the repository layer; however, the fact that we are using a Guid is an implementation detail.
        if (localVersion != Guid.Parse(databaseVersion.Version))
        {
            _logger.LogDebug(
                "Cache for {EntityType} is not synced: local version {LocalVersion} does not match database version {DatabaseVersion}",
                typeof(TEntity).Name,
                localVersion,
                databaseVersion.Version);
            return false;
        }

        _logger.LogDebug("Cache for {EntityType} is synced", typeof(TEntity).Name);
        return true;
    }

    /// <inheritdoc />
    public async Task SetCacheUpdatedAsync<TEntity>()
        where TEntity : class
    {
        var cacheKey = GetCacheKey<TEntity>();

        ConcurrentHashSet<string>? writtenKeys = GetOrRegisterScopeWrittenKeys();
        if (writtenKeys?.TryAdd(cacheKey) is false)
        {
            _logger.LogDebug("Cache version for {EntityType} already written in this scope, skipping", typeof(TEntity).Name);
            return;
        }

        if (_requestCache.IsAvailable)
        {
            // Published at the end of the request, after the cache instructions. The local version and the accessor
            // are left as they are: this server's isolated cache was updated in place, so its local version must keep
            // matching the published one until the new version is published.
            if (writtenKeys is null)
            {
                AddPendingCacheKey(cacheKey);
            }

            return;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.CacheVersion);
        await WriteVersionAsync(cacheKey);
        scope.Complete();
    }

    /// <inheritdoc />
    public async Task FlushCacheUpdatesAsync()
    {
        if (_requestCache.IsAvailable is false)
        {
            return;
        }

        var pendingCacheKeys = _requestCache.Get(PendingCacheKeysKey) as ConcurrentHashSet<string>;
        _requestCache.Remove(PendingCacheKeysKey);
        if (pendingCacheKeys is null || pendingCacheKeys.Count == 0)
        {
            return;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.WriteLock(Constants.Locks.CacheVersion);

        foreach (var cacheKey in pendingCacheKeys)
        {
            await WriteVersionAsync(cacheKey);
        }

        scope.Complete();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<RepositoryCacheVersion>> GetCacheVersionsAsync()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return (await _repositoryCacheVersionRepository.GetAllAsync()).ToList();
    }

    /// <inheritdoc />
    public async Task SetCachesSyncedAsync()
        => await SetCachesSyncedAsync(await GetCacheVersionsAsync());

    /// <inheritdoc />
    public Task SetCachesSyncedAsync(IEnumerable<RepositoryCacheVersion> cacheVersions)
    {
        foreach (RepositoryCacheVersion version in cacheVersions)
        {
            if (version.Version is null)
            {
                continue;
            }

            _cacheVersions[version.Identifier] = Guid.Parse(version.Version);
        }

        _repositoryCacheVersionAccessor.CachesSynced();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Gets the cache key for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The cache key for the entity type.</returns>
    internal string GetCacheKey<TEntity>()
        where TEntity : class =>
        typeof(TEntity).FullName ?? typeof(TEntity).Name;

    private async Task WriteVersionAsync(string cacheKey)
    {
        var newVersion = Guid.NewGuid();
        _logger.LogDebug("Setting cache for {CacheKey} to version {Version}", cacheKey, newVersion);
        await _repositoryCacheVersionRepository.SaveAsync(new RepositoryCacheVersion { Identifier = cacheKey, Version = newVersion.ToString() });
        _cacheVersions[cacheKey] = newVersion;
        _repositoryCacheVersionAccessor.VersionChanged(cacheKey, newVersion);
    }

    private void AddPendingCacheKey(string cacheKey)
        => (_requestCache.Get(PendingCacheKeysKey, () => new ConcurrentHashSet<string>()) as ConcurrentHashSet<string>)?.TryAdd(cacheKey);

    private ConcurrentHashSet<string>? GetOrRegisterScopeWrittenKeys()
    {
        IScopeContext? context = _scopeProvider.Context;
        if (context is null)
        {
            return null;
        }

        Guid contextId = context.InstanceId;
        ConcurrentHashSet<string> writtenKeys = _writtenKeysByScope.GetOrAdd(contextId, _ => new ConcurrentHashSet<string>());

        context.Enlist(
            $"RepositoryCacheVersionService_{contextId}",
            completed => OnScopeExit(contextId, completed));

        return writtenKeys;
    }

    // Runs after the scope's transaction has been committed or rolled back, so only committed changes are published.
    private void OnScopeExit(Guid contextId, bool completed)
    {
        if (_writtenKeysByScope.TryRemove(contextId, out ConcurrentHashSet<string>? writtenKeys) is false
            || completed is false
            || _requestCache.IsAvailable is false)
        {
            return;
        }

        foreach (var cacheKey in writtenKeys)
        {
            AddPendingCacheKey(cacheKey);
        }
    }
}
