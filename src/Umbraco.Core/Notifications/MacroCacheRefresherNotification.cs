using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class MacroCacheRefresherNotification : CacheRefresherNotification
{
    public MacroCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
