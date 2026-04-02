using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Represents a cache policy that determines how usernames are cached in the member repository.
/// This policy controls the caching behavior for member username lookups to improve performance and consistency.
/// </summary>
public class MemberRepositoryUsernameCachePolicy : DefaultRepositoryCachePolicy<IMember, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberRepositoryUsernameCachePolicy"/> class, which defines the caching policy for member repository lookups by username.
    /// </summary>
    /// <param name="cache">The application-level policy cache used for storing cached member data.</param>
    /// <param name="scopeAccessor">Provides access to the current scope for cache operations.</param>
    /// <param name="options">Configuration options for the repository cache policy.</param>
    /// <param name="repositoryCacheVersionService">Service for managing cache versioning within the repository.</param>
    /// <param name="cacheSyncService">Service responsible for synchronizing cache across distributed environments.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberRepositoryUsernameCachePolicy"/> class.
    /// </summary>
    /// <param name="cache">The <see cref="IAppPolicyCache"/> instance used for caching member data by username.</param>
    /// <param name="scopeAccessor">The <see cref="IScopeAccessor"/> used to manage the current scope for cache operations.</param>
    /// <param name="options">The <see cref="RepositoryCachePolicyOptions"/> that configure cache policy behavior for the repository.</param>
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

    /// <summary>
    /// Retrieves a member by username, first attempting to get it from the cache. If not present, fetches the member using the provided delegate and caches the result.
    /// </summary>
    /// <param name="key">A prefix used to construct the cache key for the member.</param>
    /// <param name="username">The username of the member to retrieve.</param>
    /// <param name="performGetByUsername">A delegate to fetch the member by username if it is not found in the cache.</param>
    /// <param name="performGetAll">A delegate to fetch all members. This parameter is required by the interface but is not used in this method.</param>
    /// <returns>The <see cref="IMember"/> matching the specified username if found; otherwise, <c>null</c>.</returns>
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

    /// <summary>
    /// Removes the cache entry for a member associated with the specified user name and key.
    /// </summary>
    /// <param name="key">A string used as part of the cache key to identify the cache entry.</param>
    /// <param name="username">The user name of the member whose cache entry should be removed. Can be <c>null</c>.</param>
    /// <remarks>
    /// This method also registers a cache change notification for other servers in a distributed environment.
    /// </remarks>
    public void DeleteByUserName(string key, string? username)
    {
        // We've removed an entity, register cache change for other servers.
        RegisterCacheChange();

        var cacheKey = GetEntityCacheKey(key + username);
        Cache.ClearByKey(cacheKey);
    }
}
