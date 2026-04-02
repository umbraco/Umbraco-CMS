// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when content type cache needs to be refreshed.
/// </summary>
/// <remarks>
///     This notification is used to synchronize content type cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class ContentTypeCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the content type to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public ContentTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(
        messageObject,
        messageType)
    {
    }
}
