using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache.PartialViewCacheInvalidators;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Cache;

public sealed class MemberCacheRefresher : PayloadCacheRefresherBase<MemberCacheRefresherNotification, MemberCacheRefresher.JsonPayload>
{
    public static readonly Guid UniqueId = Guid.Parse("E285DF34-ACDC-4226-AE32-C0CB5CF388DA");

    private readonly IIdKeyMap _idKeyMap;
    private readonly IMemberPartialViewCacheInvalidator _memberPartialViewCacheInvalidator;

    [Obsolete("Use the non obsoleted contructor instead. Planned for removal in V18")]
    public MemberCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IIdKeyMap idKeyMap, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : this(
            appCaches,
            serializer,
            idKeyMap,
            eventAggregator,
            factory,
            StaticServiceProvider.Instance.GetRequiredService<IMemberPartialViewCacheInvalidator>())
    {
    }

    public MemberCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IMemberPartialViewCacheInvalidator memberPartialViewCacheInvalidator)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _memberPartialViewCacheInvalidator = memberPartialViewCacheInvalidator;
    }

    #region Indirect

    public static void RefreshMemberTypes(AppCaches appCaches) => appCaches.IsolatedCaches.ClearCache<IMember>();

    #endregion

    public class JsonPayload
    {
        // [JsonConstructor]
        public JsonPayload(int id, string? username, bool removed)
        {
            Id = id;
            Username = username;
            Removed = removed;
        }

        public int Id { get; }

        public string? Username { get; }

        public bool Removed { get; }
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Member Cache Refresher";

    public override void Refresh(JsonPayload[] payloads)
    {
        ClearCache(payloads);
        base.Refresh(payloads);
    }

    public override void Refresh(int id)
    {
        ClearCache(new JsonPayload(id, null, false));
        base.Refresh(id);
    }

    public override void Remove(int id)
    {
        ClearCache(new JsonPayload(id, null, false));
        base.Remove(id);
    }

    private void ClearCache(params JsonPayload[] payloads)
    {
        _memberPartialViewCacheInvalidator.ClearPartialViewCacheItems(payloads.Select(p => p.Id));

        Attempt<IAppPolicyCache?> memberCache = AppCaches.IsolatedCaches.Get<IMember>();

        foreach (JsonPayload p in payloads)
        {
            _idKeyMap.ClearCache(p.Id);
            if (memberCache.Success is false)
            {
                continue;
            }

            memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, int>(p.Id));
            memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, string>(p.Username));

            // This specific cache key was introduced to fix an issue where the member username could not be the same as the member id, because the cache keys collided.
            // This is done in a bit of a hacky way, because the cache key is created internally in the repository, but we need to clear it here.
            // Ideally, we want to use a shared way of generating the key between this and the repository.
            // Additionally, the RepositoryCacheKeys actually caches the string to avoid re-allocating memory; we would like to also use this in the repository
            // See:
            // https://github.com/umbraco/Umbraco-CMS/pull/17350
            // https://github.com/umbraco/Umbraco-CMS/pull/17815
            memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, string>(CacheKeys.MemberUserNameCachePrefix + p.Username));
        }
    }
}
