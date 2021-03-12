using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// Event args for cache refresher updates
    /// </summary>
    public abstract class CacheRefresherNotificationBase : INotification
    {
        public CacheRefresherNotificationBase Init(object msgObject, MessageType type)
        {
            MessageType = type;
            MessageObject = msgObject;

            return this;
        }
        public object MessageObject { get; private set; }
        public MessageType MessageType { get; private set;}
    }
    public class DataTypeCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class UserCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }


    public class ContentCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class TemplateCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class RelationTypeCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class PublicAccessCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class MemberGroupCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class MemberCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class MediaCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class UserGroupCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class LanguageCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }
    public class MacroCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class DomainCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class ContentTypeCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }

    public class ApplicationCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }
    public class DictionaryCacheRefresherNotification : CacheRefresherNotificationBase
    {
    }
}
