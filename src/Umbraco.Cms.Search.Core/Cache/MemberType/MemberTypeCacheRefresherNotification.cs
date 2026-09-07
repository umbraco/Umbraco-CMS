using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Search.Core.Cache.MemberType;

/// <summary>
/// The distributed notification broadcast by <see cref="MemberTypeCacheRefresher"/>.
/// </summary>
internal sealed class MemberTypeCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about what to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public MemberTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
