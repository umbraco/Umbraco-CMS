using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

public class MemberRepositoryUsernameCachePolicy : DefaultRepositoryCachePolicy<IMember, string>
{
    public MemberRepositoryUsernameCachePolicy(
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
    public MemberRepositoryUsernameCachePolicy(
        IAppPolicyCache cache,
        IScopeAccessor scopeAccessor,
        RepositoryCachePolicyOptions options)
        : this(
            cache,
            scopeAccessor,
            options,
            StaticServiceProvider.Instance.GetRequiredService<IRepositoryCacheVersionService>(),
            StaticServiceProvider.Instance.GetRequiredService<ICacheSyncService>())
    {
    }

    public IMember? GetByUserName(string key, string? username, Func<string?, IMember?> performGetByUsername, Func<string[]?, IEnumerable<IMember>?> performGetAll)
    {
        EnsureCacheIsSynced();

        var cacheKey = GetEntityCacheKey(key + username);
        IMember? fromCache = Cache.GetCacheItem<IMember>(cacheKey);

        // if found in cache then return else fetch and cache
        if (fromCache != null)
        {
            return fromCache;
        }

        IMember? entity = performGetByUsername(username);

        if (entity != null && entity.HasIdentity)
        {
            InsertEntity(cacheKey, entity);
        }

        return entity;
    }

    public void DeleteByUserName(string key, string? username)
    {
        // We've removed an entity, register cache change for other servers.
        RegisterCacheChange();

        var cacheKey = GetEntityCacheKey(key + username);
        Cache.ClearByKey(cacheKey);
    }
}
