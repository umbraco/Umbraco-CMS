// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when user group cache needs to be refreshed.
/// </summary>
/// <remarks>
///     This notification is used to synchronize user group cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class UserGroupCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the user groups to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public UserGroupCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
