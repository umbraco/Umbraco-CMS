using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
/// A notification that is used to trigger the Entity Type Moved Cache Refresher.
/// </summary>
public class EntityTypeMovedCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityTypeMovedCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The refresher payload.</param>
    /// <param name="messageType">Type of the cache refresher message, <see cref="MessageType"/>.</param>
    public EntityTypeMovedCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
