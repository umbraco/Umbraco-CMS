using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Entities;
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

    private readonly IRepositoryCacheVersionService _repositoryCacheVersionService;
    private readonly ICacheSyncService _cacheSyncService;

    public AsyncEntityRepositoryBase(
        IEFCoreScopeAccessor<UmbracoDbContext> scopeAccessor,
        AppCaches appCaches,
        ILogger<AsyncEntityRepositoryBase<TId, TEntity>> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(scopeAccessor, appCaches)
    {
        _logger = logger;
        _repositoryCacheVersionService = repositoryCacheVersionService;
        _cacheSyncService = cacheSyncService;
    }

    /// <summary>
    ///     Gets the logger
    /// </summary>
    protected ILogger<AsyncEntityRepositoryBase<TId, TEntity>> _logger { get;  }


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

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken) =>
        await CachePolicy.DeleteAsync(entity, PersistDeletedItemAsync);

    public async Task<TEntity?> GetAsync(TId? id, CancellationToken cancellationToken) =>
        await CachePolicy.GetAsync(id, PerformGetAsync);

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
            return await CachePolicy.GetAllAsync(PerformGetAllAsync);
        }

        var entities = new List<TEntity>();
        foreach (IEnumerable<TId> group in ids.InGroupsOf(Core.Constants.Sql.MaxParameterCount))
        {
            TEntity[] groups = await CachePolicy.GetManyAsync(group.ToArray(), PerformGetManyAsync);
            entities.AddRange(groups);
        }

        return entities;
    }

    public async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken)
        => await CachePolicy.ExistsAsync(id, PerformExistsAsync);

    protected virtual IAsyncRepositoryCachePolicy<TEntity, TId> CreateCachePolicy()
        => new AsyncDefaultRepositoryCachePolicy<TEntity, TId>(
            GlobalIsolatedCache,
            ScopeAccessor,
            DefaultOptions,
            _repositoryCacheVersionService,
            _cacheSyncService);

    protected abstract Task<TEntity?> PerformGetAsync(TId? id);

    protected abstract Task<IEnumerable<TEntity>?> PerformGetAllAsync();

    protected abstract Task<IEnumerable<TEntity>?> PerformGetManyAsync(TId[]? ids);

    protected abstract Task PersistNewItemAsync(TEntity item);

    protected abstract Task PersistUpdatedItemAsync(TEntity item);

    protected virtual Task<bool> PerformExistsAsync(TId id)
        => AmbientScope.ExecuteWithContextAsync(async db =>
        {
            if (id is not Guid key)
            {
                return false;
            }

            return await db.Set<TEntity>().AnyAsync(e => e.Key == key);
        });

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
