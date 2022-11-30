using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

public sealed class ApplicationCacheRefresher : CacheRefresherBase<ApplicationCacheRefresherNotification>
{
    public static readonly Guid UniqueId = Guid.Parse("B15F34A1-BC1D-4F8B-8369-3222728AB4C8");

    public ApplicationCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Application Cache Refresher";

    public override void RefreshAll()
    {
        AppCaches.RuntimeCache.Clear(CacheKeys.ApplicationsCacheKey);
        base.RefreshAll();
    }

    public override void Refresh(int id)
    {
        Remove(id);
        base.Refresh(id);
    }

    public override void Remove(int id)
    {
        AppCaches.RuntimeCache.Clear(CacheKeys.ApplicationsCacheKey);
        base.Remove(id);
    }
}
