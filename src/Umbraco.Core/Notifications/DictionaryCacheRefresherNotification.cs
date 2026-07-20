// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when dictionary cache needs to be refreshed.
/// </summary>
/// <remarks>
///     This notification is used to synchronize dictionary cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public class DictionaryCacheRefresherNotification : CacheRefresherNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DictionaryCacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about the dictionary items to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    public DictionaryCacheRefresherNotification(object messageObject, MessageType messageType)
        : base(messageObject, messageType)
    {
    }
}
