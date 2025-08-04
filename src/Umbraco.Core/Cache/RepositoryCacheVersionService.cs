﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
internal class RepositoryCacheVersionService : IRepositoryCacheVersionService
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IRepositoryCacheVersionRepository _repositoryCacheVersionRepository;
    private readonly ILogger<RepositoryCacheVersionService> _logger;
    private readonly ConcurrentDictionary<string, Guid> _cacheVersions = new();

    public RepositoryCacheVersionService(
        ICoreScopeProvider scopeProvider,
        IRepositoryCacheVersionRepository repositoryCacheVersionRepository,
        ILogger<RepositoryCacheVersionService> logger)
    {
        _scopeProvider = scopeProvider;
        _repositoryCacheVersionRepository = repositoryCacheVersionRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> IsCacheSyncedAsync<TEntity>()
        where TEntity : class
    {
        _logger.LogDebug("Checking if cache for {EntityType} is synced", typeof(TEntity).Name);

        // We have to take a read lock to ensure the cache is not being updated while we check the version.
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        scope.ReadLock(Constants.Locks.CacheVersion);

        var cacheKey = GetCacheKey<TEntity>();

        RepositoryCacheVersion? databaseVersion = await _repositoryCacheVersionRepository.GetAsync(cacheKey);

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
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        // We have to take a write lock to ensure the cache is not being read while we update the version.
        scope.WriteLock(Constants.Locks.CacheVersion);

        var cacheKey = GetCacheKey<TEntity>();
        var newVersion = Guid.NewGuid();

        _logger.LogDebug("Setting cache for {EntityType} to version {Version}", typeof(TEntity).Name, newVersion);
        await _repositoryCacheVersionRepository.SaveAsync(new RepositoryCacheVersion { Identifier = cacheKey, Version = newVersion.ToString() });
        _cacheVersions[cacheKey] = newVersion;

        scope.Complete();
    }

    /// <inheritdoc />
    public async Task SetCachesSyncedAsync()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        scope.ReadLock(Constants.Locks.CacheVersion);

        // We always sync all caches versions, so it's safe to assume all caches are synced at this point.
        IEnumerable<RepositoryCacheVersion> cacheVersions = await _repositoryCacheVersionRepository.GetAllAsync();

        foreach (RepositoryCacheVersion version in cacheVersions)
        {
            if (version.Version is null)
            {
                continue;
            }

            _cacheVersions[version.Identifier] = Guid.Parse(version.Version);
        }

        scope.Complete();
    }

    internal string GetCacheKey<TEntity>()
        where TEntity : class =>
        typeof(TEntity).FullName ?? typeof(TEntity).Name;
}
