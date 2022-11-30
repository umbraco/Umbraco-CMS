// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models.Entities;
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
internal class FullDataSetRepositoryCachePolicy<TEntity, TId> : RepositoryCachePolicyBase<TEntity, TId>
    where TEntity : class, IEntity
{
    protected static readonly TId[] EmptyIds = new TId[0]; // const
    private readonly Func<TEntity, TId> _entityGetId;
    private readonly bool _expires;

    public FullDataSetRepositoryCachePolicy(IAppPolicyCache cache, IScopeAccessor scopeAccessor, Func<TEntity, TId> entityGetId, bool expires)
        : base(cache, scopeAccessor)
    {
        _entityGetId = entityGetId;
        _expires = expires;
    }

    /// <inheritdoc />
    public override void Create(TEntity entity, Action<TEntity> persistNew)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            persistNew(entity);
        }
        finally
        {
            ClearAll();
        }
    }

    protected string GetEntityTypeCacheKey() => $"uRepo_{typeof(TEntity).Name}_";

    protected void InsertEntities(TEntity[]? entities)
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
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            persistUpdated(entity);
        }
        finally
        {
            ClearAll();
        }
    }

    /// <inheritdoc />
    public override void Delete(TEntity entity, Action<TEntity> persistDeleted)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            persistDeleted(entity);
        }
        finally
        {
            ClearAll();
        }
    }

    /// <inheritdoc />
    public override TEntity? Get(TId? id, Func<TId?, TEntity?> performGet, Func<TId[]?, IEnumerable<TEntity>?> performGetAll)
    {
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
        // get all as one set, then look for the entity
        IEnumerable<TEntity> all = GetAllCached(performGetAll);
        return all.Any(x => _entityGetId(x)?.Equals(id) ?? false);
    }

    /// <inheritdoc />
    public override TEntity[] GetAll(TId[]? ids, Func<TId[], IEnumerable<TEntity>?> performGetAll)
    {
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

    // does NOT clone anything, so be nice with the returned values
    internal IEnumerable<TEntity> GetAllCached(Func<TId[], IEnumerable<TEntity>?> performGetAll)
    {
        // try the cache first
        DeepCloneableList<TEntity>? all = Cache.GetCacheItem<DeepCloneableList<TEntity>>(GetEntityTypeCacheKey());
        if (all != null)
        {
            return all.ToArray();
        }

        // else get from repo and cache
        TEntity[]? entities = performGetAll(EmptyIds)?.WhereNotNull().ToArray();
        InsertEntities(entities); // may be an empty array...
        return entities ?? Enumerable.Empty<TEntity>();
    }
}
