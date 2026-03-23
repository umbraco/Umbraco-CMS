// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that triggers the data type cache refresher.
/// </summary>
/// <remarks>
///     This notification is used to synchronize data type cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class DataTypeCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the data type to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public DataTypeCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
