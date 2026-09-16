using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.Content;

/// <summary>
/// The distributed notification broadcast by <see cref="PublishedContentCacheRefresher"/>.
/// </summary>
internal sealed class PublishedContentCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedContentCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload carried by the notification.</param>
    /// <param name="messageType">The type of the message, determining how <paramref name="messageObject"/> is interpreted.</param>
    public PublishedContentCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
