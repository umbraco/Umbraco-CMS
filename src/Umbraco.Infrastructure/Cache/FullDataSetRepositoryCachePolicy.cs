// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Represents a caching policy that caches the entire entities set as a single collection.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the identifier.</typeparam>
/// <remarks>
///     <para>Caches the entire set of entities as a single collection.</para>
///     <para>
///         Used by Content-, Media- and MemberTypeRepository, DataTypeRepository, DomainRepository,
///         LanguageRepository, PublicAccessRepository, TemplateRepository... things that make sense to
///         keep as a whole in memory.
///     </para>
/// </remarks>
internal sealed class FullDataSetRepositoryCachePolicy<TEntity, TId> : RepositoryCachePolicyBase<TEntity, TId>
    where TEntity : class, IEntity
{
    private static readonly TId[] _emptyIds = []; // const
    private readonly Func<TEntity, TId> _entityGetId;
    private readonly bool _expires;
    private readonly Lock _getAllLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FullDataSetRepositoryCachePolicy{TEntity, TId}"/> class.
    /// </summary>
    /// <param name="cache">The cache to use for storing entities.</param>
    /// <param name="scopeAccessor">The scope accessor for accessing the current scope.</param>
    /// <param name="repositoryCacheVersionService">The service for managing cache version synchronization.</param>
    /// <param name="cacheSyncService">The service for synchronizing cache changes across servers.</param>
    /// <param name="entityGetId">A function to extract the identifier from an entity.</param>
    /// <param name="expires">Whether cached items should expire after a timeout.</param>
    public FullDataSetRepositoryCachePolicy(IAppPolicyCache cache, IScopeAccessor scopeAccessor, IRepositoryCacheVersionService repositoryCacheVersionService, ICacheSyncService cacheSyncService, Func<TEntity, TId> entityGetId, bool expires)
        : base(cache, scopeAccessor, repositoryCacheVersionService, cacheSyncService)
    {
        _entityGetId = entityGetId;
        _expires = expires;
    }

    /// <inheritdoc />
    public override void Create(TEntity entity, Action<TEntity> persistNew)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            persistNew(entity);
        }
        finally
        {
            ClearAll();
        }
    }

    private static string GetEntityTypeCacheKey() => RepositoryCacheKeys.GetKey<TEntity>();

    private void InsertEntities(TEntity[]? entities)
    {
        if (entities is null)
        {
            return;
        }

        // cache is expected to be a deep-cloning cache ie it deep-clones whatever is
        // IDeepCloneable when it goes in, and out. it also resets dirty properties,
        // making sure that no 'dirty' entity is cached.
        //
        // this policy is caching the entire list of entities. to ensure that entities
        // are properly deep-clones when cached, it uses a DeepCloneableList. however,
        // we don't want to deep-clone *each* entity in the list when fetching it from
        // cache as that would not be efficient for Get(id). so the DeepCloneableList is
        // set to ListCloneBehavior.CloneOnce ie it will clone *once* when inserting,
        // and then will *not* clone when retrieving.
        var key = GetEntityTypeCacheKey();

        if (_expires)
        {
            Cache.Insert(key, () => new DeepCloneableList<TEntity>(entities), TimeSpan.FromMinutes(5), true);
        }
        else
        {
            Cache.Insert(key, () => new DeepCloneableList<TEntity>(entities));
        }
    }

    /// <inheritdoc />
    public override void Update(TEntity entity, Action<TEntity> persistUpdated)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            persistUpdated(entity);
        }
        finally
        {
            ClearAll();
        }

        // We've changed the entity, register cache change for other servers.
        // We assume that if something goes wrong, we'll roll back, so don't need to register the change.
        RegisterCacheChange();
    }

    /// <inheritdoc />
    public override void Delete(TEntity entity, Action<TEntity> persistDeleted)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            persistDeleted(entity);
        }
        finally
        {
            ClearAll();
        }

        // We've changed the entity, register cache change for other servers.
        // We assume that if something goes wrong, we'll roll back, so don't need to register the change.
        RegisterCacheChange();
    }

    /// <inheritdoc />
    public override TEntity? Get(TId? id, Func<TId?, TEntity?> performGet, Func<TId[]?, IEnumerable<TEntity>?> performGetAll)
    {
        EnsureCacheIsSynced();

        // get all from the cache, then look for the entity
        IEnumerable<TEntity> all = GetAllCached(performGetAll);
        TEntity? entity = all.FirstOrDefault(x => _entityGetId(x)?.Equals(id) ?? false);

        // see note in InsertEntities - what we get here is the original
        // cached entity, not a clone, so we need to manually ensure it is deep-cloned.
        return (TEntity?)entity?.DeepClone();
    }

    /// <inheritdoc />
    public override TEntity? GetCached(TId id)
    {
        EnsureCacheIsSynced();

        // get all from the cache -- and only the cache, then look for the entity
        DeepCloneableList<TEntity>? all = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetEntityTypeCacheKey());
        TEntity? entity = all?.FirstOrDefault(x => _entityGetId(x)?.Equals(id) ?? false);

        // see note in InsertEntities - what we get here is the original
        // cached entity, not a clone, so we need to manually ensure it is deep-cloned.
        return (TEntity?)entity?.DeepClone();
    }

    /// <inheritdoc />
    public override bool Exists(TId id, Func<TId, bool> performExits, Func<TId[], IEnumerable<TEntity>?> performGetAll)
    {
        EnsureCacheIsSynced();

        // get all as one set, then look for the entity
        IEnumerable<TEntity> all = GetAllCached(performGetAll);
        return all.Any(x => _entityGetId(x)?.Equals(id) ?? false);
    }

    /// <inheritdoc />
    public override TEntity[] GetAll(TId[]? ids, Func<TId[], IEnumerable<TEntity>?> performGetAll)
    {
        EnsureCacheIsSynced();

        // get all as one set, from cache if possible, else repo
        IEnumerable<TEntity> all = GetAllCached(performGetAll);

        // if ids have been specified, filter
        if (ids?.Length > 0)
        {
            all = all.Where(x => ids.Contains(_entityGetId(x)));
        }

        // and return
        // see note in SetCacheActionToInsertEntities - what we get here is the original
        // cached entities, not clones, so we need to manually ensure they are deep-cloned.
        return all.Select(x => (TEntity)x.DeepClone()).ToArray();
    }

    /// <inheritdoc />
    public override void ClearAll() => Cache.Clear(GetEntityTypeCacheKey());

    /// <summary>
    /// Gets all cached entities, or retrieves them from the repository if not cached.
    /// </summary>
    /// <remarks>
    /// Uses double-check locking to prevent the "thundering herd" problem where multiple
    /// threads detecting a cache miss would all query the database simultaneously.
    /// Does NOT clone anything, so be nice with the returned values.
    /// </remarks>
    internal IEnumerable<TEntity> GetAllCached(Func<TId[], IEnumerable<TEntity>?> performGetAll)
    {
        // Fast path - check cache without lock.
        DeepCloneableList<TEntity>? all = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetEntityTypeCacheKey());
        if (all != null)
        {
            return all.ToArray();
        }

        // Slow path - lock to prevent thundering herd on cache miss.
        lock (_getAllLock)
        {
            // Double-check inside lock - another thread may have populated the cache.
            all = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetEntityTypeCacheKey());
            if (all != null)
            {
                return all.ToArray();
            }

            // Only one thread queries the database.
            TEntity[]? entities = performGetAll(_emptyIds)?.WhereNotNull().ToArray();
            InsertEntities(entities); // may be an empty array...
            return entities ?? Enumerable.Empty<TEntity>();
        }
    }
}
