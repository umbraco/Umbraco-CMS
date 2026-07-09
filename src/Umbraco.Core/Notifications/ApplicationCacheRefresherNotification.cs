// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when application-wide cache needs to be refreshed.
/// </summary>
/// <remarks>
///     This notification is used to synchronize application cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class ApplicationCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about what to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public ApplicationCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(
        messageObject,
        messageType)
    {
    }
}
