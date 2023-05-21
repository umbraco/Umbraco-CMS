using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Cache;

public sealed class RelationTypeCacheRefresher : CacheRefresherBase<RelationTypeCacheRefresherNotification>
{
    public RelationTypeCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory)
    {
    }

    public static readonly Guid UniqueId = Guid.Parse("D8375ABA-4FB3-4F86-B505-92FBA1B6F7C9");

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Relation Type Cache Refresher";

    public override void RefreshAll()
    {
        ClearAllIsolatedCacheByEntityType<IRelationType>();
        base.RefreshAll();
    }

    public override void Refresh(int id)
    {
        Attempt<IAppPolicyCache?> cache = AppCaches.IsolatedCaches.Get<IRelationType>();
        if (cache.Success)
        {
            cache.Result?.Clear(RepositoryCacheKeys.GetKey<IRelationType, int>(id));
        }

        base.Refresh(id);
    }

    public override void Refresh(Guid id) => throw new NotSupportedException();

    // base.Refresh(id);
    public override void Remove(int id)
    {
        Attempt<IAppPolicyCache?> cache = AppCaches.IsolatedCaches.Get<IRelationType>();
        if (cache.Success)
        {
            cache.Result?.Clear(RepositoryCacheKeys.GetKey<IRelationType, int>(id));
        }

        base.Remove(id);
    }
}
