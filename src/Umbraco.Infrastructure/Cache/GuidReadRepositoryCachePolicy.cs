// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A cache policy for GUID-keyed read repositories that share an isolated cache
///     with their parent int-keyed repository.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <remarks>
///     <para>
///         GUID-keyed read repositories and their parent int-keyed repositories both resolve
///         to the same <see cref="IAppPolicyCache" /> via <c>IsolatedCaches.GetOrCreate&lt;TEntity&gt;()</c>.
///         If both used <see cref="DefaultRepositoryCachePolicy{TEntity, TId}" />, they would share
///         the same cache key prefix (<c>"uRepo_{TypeName}_"</c>), causing the int-keyed repository's
///         prefix-based count validation to always fail (finding 2× the expected entries).
///     </para>
///     <para>
///         This policy uses a separate prefix (<c>"uRepoGuid_{TypeName}_"</c>) so that GUID-keyed
///         entries don't interfere with the int-keyed repository's cache operations.
///     </para>
///     <para>
///         For <see cref="GetAll" /> without specific IDs, this policy always delegates to the
///         repository rather than caching the full set, since the parent int-keyed repository
///         already handles full-set caching with proper count validation.
///     </para>
/// </remarks>
internal sealed class GuidReadRepositoryCachePolicy<TEntity> : RepositoryCachePolicyBase<TEntity, Guid>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Gets the cache key prefix used for storing GUIDs associated with the entity type.
    /// </summary>
    internal static string GuidCacheKeyPrefix { get; } = RepositoryCacheKeys.GetGuidKey<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="GuidReadRepositoryCachePolicy{TEntity}"/> class.
    /// </summary>
    public GuidReadRepositoryCachePolicy(
        IAppPolicyCache cache,
        IScopeAccessor scopeAccessor,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(cache, scopeAccessor, repositoryCacheVersionService, cacheSyncService)
    {
    }

    /// <inheritdoc />
    public override TEntity? Get(Guid id, Func<Guid, TEntity?> performGet, Func<Guid[]?, IEnumerable<TEntity>?> performGetAll)
    {
        EnsureCacheIsSynced();

        var cacheKey = GuidCacheKeyPrefix + id;
        TEntity? fromCache = Cache.GetCacheItem<TEntity>(cacheKey);

        if (fromCache is not null)
        {
            return fromCache;
        }

        TEntity? entity = performGet(id);

        if (entity is { HasIdentity: true })
        {
            Cache.Insert(cacheKey, () => entity, RepositoryCacheConstants.DefaultCacheDuration, true);
        }

        return entity;
    }

    /// <inheritdoc />
    public override TEntity? GetCached(Guid id)
    {
        EnsureCacheIsSynced();
        return Cache.GetCacheItem<TEntity>(GuidCacheKeyPrefix + id);
    }

    /// <inheritdoc />
    public override bool Exists(Guid id, Func<Guid, bool> performExists, Func<Guid[], IEnumerable<TEntity>?> performGetAll)
    {
        EnsureCacheIsSynced();

        TEntity? fromCache = Cache.GetCacheItem<TEntity>(GuidCacheKeyPrefix + id);
        return fromCache is not null || performExists(id);
    }

    /// <inheritdoc />
    public override TEntity[] GetAll(Guid[]? ids, Func<Guid[]?, IEnumerable<TEntity>?> performGetAll)
    {
        EnsureCacheIsSynced();

        // For specific IDs, try cache first.
        if (ids?.Length > 0)
        {
            TEntity[] cached = ids
                .Select(id => Cache.GetCacheItem<TEntity>(GuidCacheKeyPrefix + id))
                .WhereNotNull()
                .ToArray();

            if (cached.Length == ids.Length)
            {
                return cached;
            }
        }

        // Cache miss (partial or full set) — delegate to the repository.
        TEntity[] entities = performGetAll(ids)?.WhereNotNull().ToArray() ?? [];

        // For specific IDs, populate the GUID cache so subsequent lookups hit the cache.
        // For the full set (no IDs), skip, as the parent int-keyed repository handles full-set caching.
        if (ids?.Length > 0)
        {
            foreach (TEntity entity in entities)
            {
                if (entity.HasIdentity)
                {
                    var cacheKey = GuidCacheKeyPrefix + entity.Key;
                    Cache.Insert(cacheKey, () => entity, RepositoryCacheConstants.DefaultCacheDuration, true);
                }
            }
        }

        return entities;
    }

    /// <inheritdoc />
    public override void Create(TEntity entity, Action<TEntity> persistNew)
        => throw new InvalidOperationException("This method won't be implemented.");

    /// <inheritdoc />
    public override void Update(TEntity entity, Action<TEntity> persistUpdated)
        => throw new InvalidOperationException("This method won't be implemented.");

    /// <inheritdoc />
    public override void Delete(TEntity entity, Action<TEntity> persistDeleted)
        => throw new InvalidOperationException("This method won't be implemented.");

    /// <inheritdoc />
    public override void ClearAll() => Cache.ClearByKey(GuidCacheKeyPrefix);

    /// <summary>
    ///     Gets the GUID-prefixed cache key for the given entity key.
    /// </summary>
    internal static string GetCacheKey(Guid key) => GuidCacheKeyPrefix + key;
}
