using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    public class DomainCacheRefresherNotification : CacheRefresherNotification
    {
        public DomainCacheRefresherNotification(object messageObject, MessageType messageType) : base(messageObject, messageType)
        {
        }
    }
}
