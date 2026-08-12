using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the Element Container Cache Refresher.
/// </summary>
public class ElementContainerCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementContainerCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The refresher payload.</param>
    /// <param name="messageType">Type of the cache refresher message, <see cref="MessageType"/>.</param>
    public ElementContainerCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
