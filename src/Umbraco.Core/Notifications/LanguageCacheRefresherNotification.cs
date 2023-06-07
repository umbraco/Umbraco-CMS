using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

public class LanguageCacheRefresherNotification : CacheRefresherNotification
{
    public LanguageCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
