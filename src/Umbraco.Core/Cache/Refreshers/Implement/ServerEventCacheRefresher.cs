using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Distributes server events that must be delivered per connection to every server, so that each
///     server can deliver them to the backoffice connections it holds.
/// </summary>
/// <remarks>
///     This refresher clears no caches. Each server handles the published
///     <see cref="ServerEventCacheRefresherNotification" /> by delivering the event to its own connections.
/// </remarks>
public sealed class ServerEventCacheRefresher : PayloadCacheRefresherBase<ServerEventCacheRefresherNotification, ServerEventCacheRefresher.JsonPayload>
{
    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("E5E6D2DA-D1AA-4268-8156-13903B55EBDA");

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerEventCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public ServerEventCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Server Event Cache Refresher";

    /// <summary>
    ///     Represents a server event to deliver on every server.
    /// </summary>
    public record JsonPayload
    {
        /// <summary>
        ///     Gets the source of the event.
        /// </summary>
        public required string EventSource { get; init; }

        /// <summary>
        ///     Gets the type of the event.
        /// </summary>
        public required string EventType { get; init; }

        /// <summary>
        ///     Gets the unique key of the entity the event concerns.
        /// </summary>
        public Guid Key { get; init; }

        /// <summary>
        ///     Gets the tree path of the entity the event concerns, used to decide which users may receive it.
        /// </summary>
        public string? EntityPath { get; init; }

        /// <summary>
        ///     Gets the key of the user the event is addressed to, or <c>null</c> when the event is routed
        ///     to every user permitted to receive it.
        /// </summary>
        public Guid? UserKey { get; init; }
    }
}
