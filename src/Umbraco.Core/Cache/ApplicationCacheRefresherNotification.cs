using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    public class ApplicationCacheRefresherNotification : CacheRefresherNotification
    {
        public ApplicationCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
