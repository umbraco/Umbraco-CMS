using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for domain caches.
/// </summary>
public sealed class DomainCacheRefresher : PayloadCacheRefresherBase<DomainCacheRefresherNotification, DomainCacheRefresher.JsonPayload>
{
    private readonly IDomainCacheService _domainCacheService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DomainCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The cache refresher notification factory.</param>
    /// <param name="domainCacheService">The domain cache service.</param>
    public DomainCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IDomainCacheService domainCacheService)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _domainCacheService = domainCacheService;
    }

    #region Json

    /// <summary>
    ///     Represents the JSON payload for domain cache refresh operations.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonPayload" /> class.
        /// </summary>
        /// <param name="id">The identifier of the domain.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        public JsonPayload(int id, DomainChangeTypes changeType)
        {
            Id = id;
            ChangeType = changeType;
        }

        /// <summary>
        ///     Gets the identifier of the domain.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets the type of change that occurred.
        /// </summary>
        public DomainChangeTypes ChangeType { get; }
    }

    #endregion

    #region Define

    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("11290A79-4B57-4C99-AD72-7748A3CF38AF");

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Domain Cache Refresher";

    #endregion

    #region Refresher

    /// <inheritdoc />
    public override void RefreshInternal(JsonPayload[] payloads)
    {
        ClearAllIsolatedCacheByEntityType<IDomain>();

        // note: must do what's above FIRST else the repositories still have the old cached
        // content and when the PublishedCachesService is notified of changes it does not see
        // the new content...

        // notify
        _domainCacheService.Refresh(payloads);

        base.RefreshInternal(payloads);
    }

    // these events should never trigger
    // everything should be PAYLOAD/JSON

    /// <inheritdoc />
    public override void RefreshAll() => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Refresh(int id) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Refresh(Guid id) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
