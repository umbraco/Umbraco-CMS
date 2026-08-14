// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Represents a special policy that does not cache the result of GetAll.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the identifier.</typeparam>
/// <remarks>
///     <para>
///         Overrides the default repository cache policy and does not writes the result of GetAll
///         to cache, but only the result of individual Gets. It does read the cache for GetAll, though.
///     </para>
///     <para>Used by DictionaryRepository.</para>
/// </remarks>
internal sealed class SingleItemsOnlyRepositoryCachePolicy<TEntity, TId> : DefaultRepositoryCachePolicy<TEntity, TId>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleItemsOnlyRepositoryCachePolicy{TEntity, TId}"/> class, which manages caching for repository items where only single items are cached at a time.
    /// </summary>
    /// <param name="cache">The application-level policy cache used for storing cached items.</param>
    /// <param name="scopeAccessor">Provides access to the current scope for cache operations.</param>
    /// <param name="options">Configuration options for the repository cache policy.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning within the repository.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
    public SingleItemsOnlyRepositoryCachePolicy(
        IAppPolicyCache cache,
        IScopeAccessor scopeAccessor,
        RepositoryCachePolicyOptions options,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            cache,
            scopeAccessor,
            options,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    protected override void InsertEntities(TId[]? ids, TEntity[]? entities)
    {
        // nop
    }
}
