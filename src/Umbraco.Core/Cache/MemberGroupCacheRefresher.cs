using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

public sealed class MemberGroupCacheRefresher : PayloadCacheRefresherBase<MemberGroupCacheRefresherNotification, MemberGroupCacheRefresher.JsonPayload>
{
    public MemberGroupCacheRefresher(AppCaches appCaches, IJsonSerializer jsonSerializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, jsonSerializer, eventAggregator, factory)
    {
    }

    #region Json

    public class JsonPayload
    {
        public JsonPayload(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; }

        public int Id { get; }
    }

    #endregion

    #region Define

    public static readonly Guid UniqueId = Guid.Parse("187F236B-BD21-4C85-8A7C-29FBA3D6C00C");

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Member Group Cache Refresher";

    #endregion

    #region Refresher

    public override void Refresh(string json)
    {
        ClearCache();
        base.Refresh(json);
    }

    public override void Refresh(int id)
    {
        ClearCache();
        base.Refresh(id);
    }

    public override void Remove(int id)
    {
        ClearCache();
        base.Remove(id);
    }

    private void ClearCache() =>

        // Since we cache by group name, it could be problematic when renaming to
        // previously existing names - see http://issues.umbraco.org/issue/U4-10846.
        // To work around this, just clear all the cache items
        AppCaches.IsolatedCaches.ClearCache<IMemberGroup>();

    #endregion
}
