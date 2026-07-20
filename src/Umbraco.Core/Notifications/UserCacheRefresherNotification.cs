// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that triggers the user cache refresher.
/// </summary>
/// <remarks>
///     This notification is used to synchronize user cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class UserCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the user to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public UserCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
