// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using IScopeAccessor = Umbraco.Cms.Core.Scoping.EFCore.IScopeAccessor;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     A cache policy for the async (EF Core) repositories that key entities by <see cref="Guid" />.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <remarks>
///     The synchronous NPoco repositories cache their Guid-keyed reads under the
///     <c>"uRepoGuid_{TypeName}_"</c> prefix (see <see cref="GuidReadRepositoryCachePolicy{TEntity}" />), and
///     <c>ContentCacheRefresher</c> already clears that exact prefix on every save/refresh. The base
///     <see cref="AsyncDefaultRepositoryCachePolicy{TEntity, TKey}" /> instead defaults to the int-style
///     <c>"uRepo_{TypeName}_"</c> prefix even when keyed by <see cref="Guid" />, so its entries were never
///     invalidated by the existing NPoco-side clearing logic. This override reuses the exact same
///     <c>"uRepoGuid_"</c> prefix so async reads share the already-correct invalidation wiring.
/// </remarks>
internal sealed class AsyncGuidReadRepositoryCachePolicy<TEntity> : AsyncDefaultRepositoryCachePolicy<TEntity, Guid>
    where TEntity : class, IEntity
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AsyncGuidReadRepositoryCachePolicy{TEntity}"/> class.
    /// </summary>
    public AsyncGuidReadRepositoryCachePolicy(
        IAppPolicyCache cache,
        IScopeAccessor scopeAccessor,
        AsyncRepositoryCachePolicyOptions options,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(cache, scopeAccessor, options, repositoryCacheVersionService, cacheSyncService)
    {
    }

    /// <inheritdoc />
    protected override string EntityTypeCacheKey { get; } = RepositoryCacheKeys.GetGuidKey<TEntity>();
}
