// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Repositories;
using IScopeAccessor = Umbraco.Cms.Core.Scoping.EFCore.IScopeAccessor;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Represents a special policy that does not cache the result of GetAll.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TKey">The type of the identifier.</typeparam>
/// <remarks>
///     <para>
///         Overrides the default repository cache policy and does not write the result of GetAll
///         to cache, but only the result of individual Gets. It does read the cache for GetAll, though.
///     </para>
///     <para>Used by DictionaryRepository.</para>
/// </remarks>
internal sealed class AsyncSingleItemsOnlyRepositoryCachePolicy<TEntity, TKey> : AsyncDefaultRepositoryCachePolicy<TEntity, TKey>
    where TEntity : class, IEntity
{
    public AsyncSingleItemsOnlyRepositoryCachePolicy(
        IAppPolicyCache cache,
        IScopeAccessor scopeAccessor,
        AsyncRepositoryCachePolicyOptions options,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(cache, scopeAccessor, options, repositoryCacheVersionService, cacheSyncService)
    {
    }

    protected override void InsertEntities(TEntity[]? entities)
    {
        // nop
    }
}
