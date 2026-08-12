using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.ContentType;

/// <summary>
/// The distributed notification broadcast by <see cref="ContentTypeCacheRefresher"/>.
/// </summary>
internal sealed class ContentTypeCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeCacheRefresherNotification"/> class.
    /// </summary>
    public ContentTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
