// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that triggers the content cache refresher.
/// </summary>
/// <remarks>
///     This notification is used to synchronize content cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class ContentCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the content to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public ContentCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(
        messageObject,
        messageType)
    {
    }
}
