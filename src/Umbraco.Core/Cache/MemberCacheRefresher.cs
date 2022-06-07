// using Newtonsoft.Json;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

public sealed class MemberCacheRefresher : PayloadCacheRefresherBase<MemberCacheRefresherNotification, MemberCacheRefresher.JsonPayload>
{
    public static readonly Guid UniqueId = Guid.Parse("E285DF34-ACDC-4226-AE32-C0CB5CF388DA");

    private readonly IIdKeyMap _idKeyMap;

    public MemberCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IIdKeyMap idKeyMap, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory) =>
        _idKeyMap = idKeyMap;

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
        AppCaches.ClearPartialViewCache();
        Attempt<IAppPolicyCache?> memberCache = AppCaches.IsolatedCaches.Get<IMember>();

        foreach (JsonPayload p in payloads)
        {
            _idKeyMap.ClearCache(p.Id);
            if (memberCache.Success)
            {
                memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, int>(p.Id));
                memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, string>(p.Username));
            }
        }
    }
}
