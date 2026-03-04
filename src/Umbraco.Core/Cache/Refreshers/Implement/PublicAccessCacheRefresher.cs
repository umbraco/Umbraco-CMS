using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for public access entry caches.
/// </summary>
public sealed class PublicAccessCacheRefresher : CacheRefresherBase<PublicAccessCacheRefresherNotification>
{
    #region Define

    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("1DB08769-B104-4F8B-850E-169CAC1DF2EC");

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublicAccessCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public PublicAccessCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Public Access Cache Refresher";

    #endregion

    #region Refresher

    /// <inheritdoc />
    public override void Refresh(Guid id)
    {
        ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
        base.Refresh(id);
    }

    /// <inheritdoc />
    public override void Refresh(int id)
    {
        ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
        base.Refresh(id);
    }

    /// <inheritdoc />
    public override void RefreshAll()
    {
        ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
        base.RefreshAll();
    }

    /// <inheritdoc />
    public override void Remove(int id)
    {
        ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
        base.Remove(id);
    }

    #endregion
}
