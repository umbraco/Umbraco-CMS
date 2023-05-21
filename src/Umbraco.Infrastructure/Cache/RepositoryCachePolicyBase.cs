// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A base class for repository cache policies.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public abstract class RepositoryCachePolicyBase<TEntity, TId> : IRepositoryCachePolicy<TEntity, TId>
    where TEntity : class, IEntity
{
    private readonly IAppPolicyCache _globalCache;
    private readonly IScopeAccessor _scopeAccessor;

    protected RepositoryCachePolicyBase(IAppPolicyCache globalCache, IScopeAccessor scopeAccessor)
    {
        _globalCache = globalCache ?? throw new ArgumentNullException(nameof(globalCache));
        _scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));
    }

    protected IAppPolicyCache Cache
    {
        get
        {
            IScope? ambientScope = _scopeAccessor.AmbientScope;
            switch (ambientScope?.RepositoryCacheMode)
            {
                case RepositoryCacheMode.Default:
                    return _globalCache;
                case RepositoryCacheMode.Scoped:
                    return ambientScope.IsolatedCaches.GetOrCreate<TEntity>();
                case RepositoryCacheMode.None:
                    return NoAppCache.Instance;
                default:
                    throw new NotSupportedException(
                        $"Repository cache mode {ambientScope?.RepositoryCacheMode} is not supported.");
            }
        }
    }

    /// <inheritdoc />
    public abstract TEntity? Get(TId? id, Func<TId?, TEntity?> performGet, Func<TId[]?, IEnumerable<TEntity>?> performGetAll);

    /// <inheritdoc />
    public abstract TEntity? GetCached(TId id);

    /// <inheritdoc />
    public abstract bool Exists(TId id, Func<TId, bool> performExists, Func<TId[], IEnumerable<TEntity>?> performGetAll);

    /// <inheritdoc />
    public abstract void Create(TEntity entity, Action<TEntity> persistNew);

    /// <inheritdoc />
    public abstract void Update(TEntity entity, Action<TEntity> persistUpdated);

    /// <inheritdoc />
    public abstract void Delete(TEntity entity, Action<TEntity> persistDeleted);

    /// <inheritdoc />
    public abstract TEntity[] GetAll(TId[]? ids, Func<TId[]?, IEnumerable<TEntity>?> performGetAll);

    /// <inheritdoc />
    public abstract void ClearAll();
}
