using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.Element;

/// <summary>
/// The distributed notification broadcast by <see cref="PublishedElementCacheRefresher"/>.
/// </summary>
internal sealed class PublishedElementCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedElementCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload carried by the notification.</param>
    /// <param name="messageType">The type of the message, determining how <paramref name="messageObject"/> is interpreted.</param>
    public PublishedElementCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
