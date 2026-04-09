// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that triggers the media cache refresher.
/// </summary>
/// <remarks>
///     This notification is used to synchronize media cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class MediaCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the media to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public MediaCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
