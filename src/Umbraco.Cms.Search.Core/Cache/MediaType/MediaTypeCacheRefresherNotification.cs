using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.MediaType;

/// <summary>
/// The distributed notification broadcast by <see cref="MediaTypeCacheRefresher"/>.
/// </summary>
internal sealed class MediaTypeCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about what to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public MediaTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
