using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for dictionary item caches.
/// </summary>
public sealed class DictionaryCacheRefresher : CacheRefresherBase<DictionaryCacheRefresherNotification>
{
    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("D1D7E227-F817-4816-BFE9-6C39B6152884");

    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public DictionaryCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Dictionary Cache Refresher";

    /// <inheritdoc />
    public override void Refresh(int id)
    {
        ClearAllIsolatedCacheByEntityType<IDictionaryItem>();
        base.Refresh(id);
    }

    /// <inheritdoc />
    public override void Remove(int id)
    {
        ClearAllIsolatedCacheByEntityType<IDictionaryItem>();
        base.Remove(id);
    }
}
