using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.Content;

/// <summary>
/// The distributed notification broadcast by <see cref="DraftContentCacheRefresher"/>.
/// </summary>
internal sealed class DraftContentCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DraftContentCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload carried by the notification.</param>
    /// <param name="messageType">The type of the message, determining how <paramref name="messageObject"/> is interpreted.</param>
    public DraftContentCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
