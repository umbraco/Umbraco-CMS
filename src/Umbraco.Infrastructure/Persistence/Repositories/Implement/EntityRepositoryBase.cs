using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Provides a base class to all <see cref="IEntity" /> based repositories.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <typeparam name="TEntity">The type of the entity managed by this repository.</typeparam>
public abstract class EntityRepositoryBase<TId, TEntity> : RepositoryBase, IReadWriteQueryRepository<TId, TEntity>
    where TEntity : class, IEntity
{
    private static RepositoryCachePolicyOptions? _defaultOptions;
    private IRepositoryCachePolicy<TEntity, TId>? _cachePolicy;
    private IQuery<TEntity>? _hasIdQuery;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityRepositoryBase{TId, TEntity}" /> class.
    /// </summary>
    protected EntityRepositoryBase(IScopeAccessor scopeAccessor, AppCaches appCaches, ILogger<EntityRepositoryBase<TId, TEntity>> logger)
        : base(scopeAccessor, appCaches) =>
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    ///     Gets the logger
    /// </summary>
    protected ILogger<EntityRepositoryBase<TId, TEntity>> Logger { get; }

    /// <summary>
    ///     Gets the isolated cache for the <see cref="TEntity" />
    /// </summary>
    protected IAppPolicyCache GlobalIsolatedCache => AppCaches.IsolatedCaches.GetOrCreate<TEntity>();

    /// <summary>
    ///     Gets the isolated cache.
    /// </summary>
    /// <remarks>Depends on the ambient scope cache mode.</remarks>
    protected IAppPolicyCache IsolatedCache
    {
        get
        {
            switch (AmbientScope.RepositoryCacheMode)
            {
                case RepositoryCacheMode.Default:
                    return AppCaches.IsolatedCaches.GetOrCreate<TEntity>();
                case RepositoryCacheMode.Scoped:
                    return AmbientScope.IsolatedCaches.GetOrCreate<TEntity>();
                case RepositoryCacheMode.None:
                    return NoAppCache.Instance;
                default:
                    throw new Exception("oops: cache mode.");
            }
        }
    }

    /// <summary>
    ///     Gets the default <see cref="RepositoryCachePolicyOptions" />
    /// </summary>
    protected virtual RepositoryCachePolicyOptions DefaultOptions => _defaultOptions
        ??= new RepositoryCachePolicyOptions(() =>
        {
            // get count of all entities of current type (TEntity) to ensure cached result is correct
            // create query once if it is needed (no need for locking here) - query is static!
            IQuery<TEntity> query = _hasIdQuery ??= AmbientScope.SqlContext.Query<TEntity>().Where(x => x.Id != 0);
            return PerformCount(query);
        });

    /// <summary>
    ///     Gets the repository cache policy
    /// </summary>
    protected IRepositoryCachePolicy<TEntity, TId> CachePolicy
    {
        get
        {
            if (AppCaches == AppCaches.NoCache)
            {
                return NoCacheRepositoryCachePolicy<TEntity, TId>.Instance;
            }

            // create the cache policy using IsolatedCache which is either global
            // or scoped depending on the repository cache mode for the current scope
            switch (AmbientScope.RepositoryCacheMode)
            {
                case RepositoryCacheMode.Default:
                case RepositoryCacheMode.Scoped:
                    // return the same cache policy in both cases - the cache policy is
                    // supposed to pick either the global or scope cache depending on the
                    // scope cache mode
                    return _cachePolicy ??= CreateCachePolicy();
                case RepositoryCacheMode.None:
                    return NoCacheRepositoryCachePolicy<TEntity, TId>.Instance;
                default:
                    throw new Exception("oops: cache mode.");
            }
        }
    }

    /// <summary>
    ///     Adds or Updates an entity of type TEntity
    /// </summary>
    /// <remarks>This method is backed by an <see cref="IAppPolicyCache" /> cache</remarks>
    public virtual void Save(TEntity entity)
    {
        if (entity.HasIdentity == false)
        {
            CachePolicy.Create(entity, PersistNewItem);
        }
        else
        {
            CachePolicy.Update(entity, PersistUpdatedItem);
        }
    }

    /// <summary>
    ///     Deletes the passed in entity
    /// </summary>
    public virtual void Delete(TEntity entity)
        => CachePolicy.Delete(entity, PersistDeletedItem);

    /// <summary>
    ///     Gets an entity by the passed in Id utilizing the repository's cache policy
    /// </summary>
    public TEntity? Get(TId? id)
        => CachePolicy.Get(id, PerformGet, PerformGetAll);

    /// <summary>
    ///     Gets all entities of type TEntity or a list according to the passed in Ids
    /// </summary>
    public IEnumerable<TEntity> GetMany(params TId[]? ids)
    {
        // ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
        ids = ids?.Distinct()

            // don't query by anything that is a default of T (like a zero)
            // TODO: I think we should enabled this in case accidental calls are made to get all with invalid ids
            // .Where(x => Equals(x, default(TId)) == false)
            .ToArray();

        // can't query more than 2000 ids at a time... but if someone is really querying 2000+ entities,
        // the additional overhead of fetching them in groups is minimal compared to the lookup time of each group
        if (ids?.Length <= Constants.Sql.MaxParameterCount)
        {
            return CachePolicy.GetAll(ids, PerformGetAll);
        }

        var entities = new List<TEntity>();
        foreach (IEnumerable<TId> group in ids.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            TEntity[] groups = CachePolicy.GetAll(group.ToArray(), PerformGetAll);
            entities.AddRange(groups);
        }

        return entities;
    }

    /// <summary>
    ///     Gets a list of entities by the passed in query
    /// </summary>
    public IEnumerable<TEntity> Get(IQuery<TEntity> query) =>

        // ensure we don't include any null refs in the returned collection!
        PerformGetByQuery(query)
            .WhereNotNull();

    /// <summary>
    ///     Returns a boolean indicating whether an entity with the passed Id exists
    /// </summary>
    public bool Exists(TId id)
        => CachePolicy.Exists(id, PerformExists, PerformGetAll);

    /// <summary>
    ///     Returns an integer with the count of entities found with the passed in query
    /// </summary>
    public int Count(IQuery<TEntity> query)
        => PerformCount(query);

    /// <summary>
    ///     Get the entity id for the <see cref="TEntity" />
    /// </summary>
    protected virtual TId GetEntityId(TEntity entity)
        => (TId)(object)entity.Id;

    /// <summary>
    ///     Create the repository cache policy
    /// </summary>
    protected virtual IRepositoryCachePolicy<TEntity, TId> CreateCachePolicy()
        => new DefaultRepositoryCachePolicy<TEntity, TId>(GlobalIsolatedCache, ScopeAccessor, DefaultOptions);

    protected abstract TEntity? PerformGet(TId? id);

    protected abstract IEnumerable<TEntity> PerformGetAll(params TId[]? ids);

    protected abstract IEnumerable<TEntity> PerformGetByQuery(IQuery<TEntity> query);

    protected abstract void PersistNewItem(TEntity item);

    protected abstract void PersistUpdatedItem(TEntity item);

    // TODO: obsolete, use QueryType instead everywhere like GetBaseQuery(QueryType queryType);
    protected abstract Sql<ISqlContext> GetBaseQuery(bool isCount);

    protected abstract string GetBaseWhereClause();

    protected abstract IEnumerable<string> GetDeleteClauses();

    protected virtual bool PerformExists(TId id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(true);
        sql.Where(GetBaseWhereClause(), new { id });
        var count = Database.ExecuteScalar<int>(sql);
        return count == 1;
    }

    protected virtual int PerformCount(IQuery<TEntity> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(true);
        var translator = new SqlTranslator<TEntity>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        return Database.ExecuteScalar<int>(sql);
    }

    protected virtual void PersistDeletedItem(TEntity entity)
    {
        IEnumerable<string> deletes = GetDeleteClauses();
        foreach (var delete in deletes)
        {
            Database.Execute(delete, new { id = GetEntityId(entity) });
        }

        entity.DeleteDate = DateTime.Now;
    }
}
