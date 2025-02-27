using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

public sealed class UserCacheRefresher : PayloadCacheRefresherBase<UserCacheRefresherNotification, UserCacheRefresher.JsonPayload>
{
    public UserCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public static readonly Guid UniqueId = Guid.Parse("E057AF6D-2EE6-41F4-8045-3694010F0AA6");

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "User Cache Refresher";

    public record JsonPayload
    {
        public int Id { get; init; }

        public Guid Key { get; init; }
    }

    public override void RefreshAll()
    {
        ClearAllIsolatedCacheByEntityType<IUser>();
        base.RefreshAll();
    }

    public override void Refresh(JsonPayload[] payloads)
    {
        ClearCache(payloads);
        base.Refresh(payloads);
    }

    private void ClearCache(params JsonPayload[] payloads)
    {
        foreach (JsonPayload p in payloads)
        {
            Attempt<IAppPolicyCache?> userCache = AppCaches.IsolatedCaches.Get<IUser>();
            if (!userCache.Success)
            {
                continue;
            }

            userCache.Result?.Clear(RepositoryCacheKeys.GetKey<IUser, Guid>(p.Key));
            userCache.Result?.Clear(RepositoryCacheKeys.GetKey<IUser, int>(p.Id));
            userCache.Result?.ClearByKey(CacheKeys.UserContentStartNodePathsPrefix + p.Key);
            userCache.Result?.ClearByKey(CacheKeys.UserMediaStartNodePathsPrefix + p.Key);
            userCache.Result?.ClearByKey(CacheKeys.UserAllContentStartNodesPrefix + p.Key);
            userCache.Result?.ClearByKey(CacheKeys.UserAllMediaStartNodesPrefix + p.Key);
        }
    }
}
