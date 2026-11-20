using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published on every server when a server event is distributed through the
///     <see cref="Cache.ServerEventCacheRefresher" />.
/// </summary>
public class ServerEventCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerEventCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payloads describing the server events to deliver.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public ServerEventCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
