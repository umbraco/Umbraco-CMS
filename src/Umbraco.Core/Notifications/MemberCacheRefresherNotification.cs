using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;
/// <summary>
/// A notification that is used to trigger the Member Cache Refresher.
/// </summary>
public class MemberCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///  Initializes a new instance of the  <see cref="MemberCacheRefresherNotification"/>
    /// </summary>
    /// <param name="messageObject">
    /// The refresher payload.
    /// </param>
    /// <param name="messageType">
    /// Type of the cache refresher message, <see cref="MessageType"/>
    /// </param>
    public MemberCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
