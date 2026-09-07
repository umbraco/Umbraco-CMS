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
    /// <param name="messageObject">The payload containing information about what to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public ContentTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
