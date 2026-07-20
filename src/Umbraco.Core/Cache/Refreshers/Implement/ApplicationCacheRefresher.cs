using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for application section (tree) caches.
/// </summary>
public sealed class ApplicationCacheRefresher : CacheRefresherBase<ApplicationCacheRefresherNotification>
{
    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("B15F34A1-BC1D-4F8B-8369-3222728AB4C8");

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public ApplicationCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Application Cache Refresher";

    /// <inheritdoc />
    public override void RefreshAll()
    {
        AppCaches.RuntimeCache.Clear(CacheKeys.ApplicationsCacheKey);
        base.RefreshAll();
    }

    /// <inheritdoc />
    public override void Refresh(int id)
    {
        Remove(id);
        base.Refresh(id);
    }

    /// <inheritdoc />
    public override void Remove(int id)
    {
        AppCaches.RuntimeCache.Clear(CacheKeys.ApplicationsCacheKey);
        base.Remove(id);
    }
}
