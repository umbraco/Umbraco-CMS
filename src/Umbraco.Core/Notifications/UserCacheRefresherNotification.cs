using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the User Cache Refresher.
/// </summary>
public class UserCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="UserCacheRefresherNotification"/>
    /// </summary>
    /// <param name="messageObject">
    /// The refresher payload.
    /// </param>
    /// <param name="messageType">
    /// Type of the cache refresher message, <see cref="MessageType"/>
    /// </param>
    public UserCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
