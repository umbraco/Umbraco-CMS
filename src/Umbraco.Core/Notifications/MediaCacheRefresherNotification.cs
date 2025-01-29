using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the Media Cache Refresher.
/// </summary>
public class MediaCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="MediaCacheRefresherNotification"/>
    /// </summary>
    /// <param name="messageObject">
    /// The refresher payload.
    /// </param>
    /// <param name="messageType">
    /// Type of the cache refresher message, <see cref="MessageType"/>
    /// </param>
    public MediaCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
