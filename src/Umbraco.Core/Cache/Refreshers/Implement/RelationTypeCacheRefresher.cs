using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for relation type caches.
/// </summary>
public sealed class RelationTypeCacheRefresher : CacheRefresherBase<RelationTypeCacheRefresherNotification>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationTypeCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public RelationTypeCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory)
    {
    }

    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("D8375ABA-4FB3-4F86-B505-92FBA1B6F7C9");

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Relation Type Cache Refresher";

    /// <inheritdoc />
    public override void RefreshAll()
    {
        ClearAllIsolatedCacheByEntityType<IRelationType>();
        base.RefreshAll();
    }

    /// <inheritdoc />
    public override void Refresh(int id)
    {
        Attempt<IAppPolicyCache?> cache = AppCaches.IsolatedCaches.Get<IRelationType>();
        if (cache.Success)
        {
            cache.Result?.Clear(RepositoryCacheKeys.GetKey<IRelationType, int>(id));
        }

        base.Refresh(id);
    }

    /// <inheritdoc />
    public override void Refresh(Guid id) => throw new NotSupportedException();

    /// <inheritdoc />
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
