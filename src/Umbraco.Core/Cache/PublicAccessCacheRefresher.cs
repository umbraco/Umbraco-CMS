using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

public sealed class PublicAccessCacheRefresher : CacheRefresherBase<PublicAccessCacheRefresherNotification>
{
    #region Define

    public static readonly Guid UniqueId = Guid.Parse("1DB08769-B104-4F8B-850E-169CAC1DF2EC");

    public PublicAccessCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Public Access Cache Refresher";

    #endregion

    #region Refresher

    public override void Refresh(Guid id)
    {
        ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
        base.Refresh(id);
    }

    public override void Refresh(int id)
    {
        ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
        base.Refresh(id);
    }

    public override void RefreshAll()
    {
        ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
        base.RefreshAll();
    }

    public override void Remove(int id)
    {
        ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
        base.Remove(id);
    }

    #endregion
}
