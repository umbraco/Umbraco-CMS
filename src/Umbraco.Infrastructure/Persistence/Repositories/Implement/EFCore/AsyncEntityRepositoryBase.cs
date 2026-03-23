using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     Provides a base class to all <see cref="IEntity" /> based repositories.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
/// <typeparam name="TEntity">The type of the entity managed by this repository.</typeparam>
public abstract class AsyncEntityRepositoryBase<TId, TEntity> : AsyncRepositoryBase, IAsyncReadWriteRepository<TId, TEntity>
    where TEntity : class, IEntity
{
    private static AsyncRepositoryCachePolicyOptions? _defaultOptions;


    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncEntityRepositoryBase{TId, TEntity}"/> class.
    /// </summary>
    /// <param name="scopeAccessor">The EF Core scope accessor.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="repositoryCacheVersionService">The repository cache version service.</param>
    /// <param name="cacheSyncService">The cache synchronization service.</param>
    public AsyncEntityRepositoryBase(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches appCaches,
        ILogger<AsyncEntityRepositoryBase<TId, TEntity>> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(scopeAccessor, appCaches)
    {
        Logger = logger;
        RepositoryCacheVersionService = repositoryCacheVersionService;
        CacheSyncService = cacheSyncService;
    }

    /// <summary>
    ///     Gets the logger
    /// </summary>
    protected ILogger<AsyncEntityRepositoryBase<TId, TEntity>> Logger { get; }

    /// <summary>
    ///    Gets the repository cache version service.
    /// </summary>
    protected IRepositoryCacheVersionService RepositoryCacheVersionService { get; }

    /// <summary>
    /// Gets the cache synchronization service.
    /// </summary>
    protected ICacheSyncService CacheSyncService { get; }

    /// <summary>
    ///     Gets the isolated cache for the <typeparamref name="TEntity"/>
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
    protected virtual AsyncRepositoryCachePolicyOptions DefaultOptions => _defaultOptions
        ??= new AsyncRepositoryCachePolicyOptions(() =>
            AmbientScope.ExecuteWithContextAsync(db => db.Set<TEntity>().CountAsync()));

    /// <summary>
    ///     Gets the repository cache policy
    /// </summary>
    [field: AllowNull, MaybeNull]
    protected IAsyncRepositoryCachePolicy<TEntity, TId> CachePolicy
    {
        get
        {
            if (AppCaches == AppCaches.NoCache)
            {
                return AsyncNoCacheRepositoryCachePolicy<TEntity, TId>.Instance;
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
                    return field ??= CreateCachePolicy();
                case RepositoryCacheMode.None:
                    return AsyncNoCacheRepositoryCachePolicy<TEntity, TId>.Instance;
                default:
                    throw new Exception("oops: cache mode.");
            }
        }
    }

    /// <summary>
    ///     Adds or updates an entity, backed by the repository cache policy.
    /// </summary>
    /// <param name="entity">The entity to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SaveAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (entity.HasIdentity == false)
        {
            await CachePolicy.CreateAsync(entity, PersistNewItemAsync);
        }
        else
        {
            await CachePolicy.UpdateAsync(entity, PersistUpdatedItemAsync);
        }
    }

    /// <summary>
    ///     Deletes the passed in entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken) =>
        await CachePolicy.DeleteAsync(entity, PersistDeletedItemAsync);

    /// <summary>
    ///     Gets an entity by its identifier, utilizing the repository cache policy.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity, or <see langword="null"/> if not found.</returns>
    public async Task<TEntity?> GetAsync(TId? id, CancellationToken cancellationToken) =>
        await CachePolicy.GetAsync(id, PerformGetAsync, PerformGetAllAsync);

    /// <summary>
    ///     Gets all entities of type <typeparamref name="TEntity"/>, or a subset matching the passed identifiers.
    /// </summary>
    /// <param name="ids">The identifiers to retrieve, or <see langword="null"/> to get all.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching entities.</returns>
    public async Task<IEnumerable<TEntity>> GetManyAsync(TId[]? ids, CancellationToken cancellationToken)
    {
        // ensure they are de-duplicated, easy win if people don't do this as this can cause many excess queries
        ids = ids?.Distinct()

            // don't query by anything that is a default of T (like a zero)
            // TODO: I think we should enabled this in case accidental calls are made to get all with invalid ids
            // .Where(x => Equals(x, default(TId)) == false)
            .ToArray() ?? [];

        // can't query more than 2000 ids at a time... but if someone is really querying 2000+ entities,
        // the additional overhead of fetching them in groups is minimal compared to the lookup time of each group
        if (ids.Length <= Core.Constants.Sql.MaxParameterCount)
        {
            return await CachePolicy.GetManyAsync(ids, PerformGetManyAsync, PerformGetAllAsync);
        }

        var entities = new List<TEntity>();
        foreach (IEnumerable<TId> group in ids.InGroupsOf(Core.Constants.Sql.MaxParameterCount))
        {
            TEntity[] groups = await CachePolicy.GetManyAsync(group.ToArray(), PerformGetManyAsync, PerformGetAllAsync);
            entities.AddRange(groups);
        }

        return entities;
    }

    /// <summary>
    ///     Returns a value indicating whether an entity with the specified identifier exists.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if an entity with the identifier exists; otherwise <see langword="false"/>.</returns>
    public async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken)
        => await CachePolicy.ExistsAsync(id, PerformExistsAsync, PerformGetAllAsync);

    /// <summary>
    ///     Get the entity id for the <typeparamref name="TEntity"/>.
    /// </summary>
    protected virtual TId GetEntityId(TEntity entity) // TODO: change TId into Guid, as all entities should be using Guid keys
        => (TId)(object)entity.Key;

    /// <summary>
    ///     Creates the repository cache policy.
    /// </summary>
    /// <returns>The cache policy to use for this repository.</returns>
    protected virtual IAsyncRepositoryCachePolicy<TEntity, TId> CreateCachePolicy()
        => new AsyncDefaultRepositoryCachePolicy<TEntity, TId>(
            GlobalIsolatedCache,
            ScopeAccessor,
            DefaultOptions,
            RepositoryCacheVersionService,
            CacheSyncService);

    /// <summary>
    ///     Performs the actual get operation against the data store.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <returns>The entity, or <see langword="null"/> if not found.</returns>
    protected abstract Task<TEntity?> PerformGetAsync(TId? id);

    /// <summary>
    ///     Performs the actual get-all operation against the data store.
    /// </summary>
    /// <returns>All entities, or <see langword="null"/>.</returns>
    protected abstract Task<IEnumerable<TEntity>?> PerformGetAllAsync();

    /// <summary>
    ///     Performs the actual get-many operation against the data store.
    /// </summary>
    /// <param name="ids">The identifiers of the entities to retrieve.</param>
    /// <returns>The matching entities, or <see langword="null"/>.</returns>
    protected abstract Task<IEnumerable<TEntity>?> PerformGetManyAsync(TId[]? ids);

    /// <summary>
    ///     Persists a new entity to the data store.
    /// </summary>
    /// <param name="item">The entity to persist.</param>
    protected abstract Task PersistNewItemAsync(TEntity item);

    /// <summary>
    ///     Persists an updated entity to the data store.
    /// </summary>
    /// <param name="item">The entity to persist.</param>
    protected abstract Task PersistUpdatedItemAsync(TEntity item);

    /// <summary>
    ///     Performs the actual exists check against the data store.
    /// </summary>
    /// <param name="id">The identifier to check.</param>
    /// <returns><see langword="true"/> if an entity with the identifier exists; otherwise <see langword="false"/>.</returns>
    protected virtual Task<bool> PerformExistsAsync(TId id)
        => AmbientScope.ExecuteWithContextAsync(async db =>
        {
            if (id is not Guid key)
            {
                return false;
            }

            return await db.Set<TEntity>().AnyAsync(e => e.Key == key);
        });

    /// <summary>
    ///     Deletes an entity from the data store.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    protected virtual async Task PersistDeletedItemAsync(TEntity entity)
    {
        await AmbientScope.ExecuteWithContextAsync(async db =>
        {
            await db.Set<TEntity>().Where(x => x.Id == entity.Id).ExecuteDeleteAsync();
            return true;
        });

        entity.DeleteDate = DateTime.UtcNow;
    }
}
