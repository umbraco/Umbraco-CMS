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

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public SingleItemsOnlyRepositoryCachePolicy(IAppPolicyCache cache, IScopeAccessor scopeAccessor, RepositoryCachePolicyOptions options)
        : this(
            cache,
            scopeAccessor,
            options,
            StaticServiceProvider.Instance.GetRequiredService<IRepositoryCacheVersionService>(),
            StaticServiceProvider.Instance.GetRequiredService<ICacheSyncService>())
    {
    }

    protected override void InsertEntities(TId[]? ids, TEntity[]? entities)
    {
        // nop
    }
}
