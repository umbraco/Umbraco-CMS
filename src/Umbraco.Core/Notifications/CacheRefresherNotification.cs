// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for cache refresher notifications.
/// </summary>
/// <remarks>
///     Cache refresher notifications are used to synchronize cache invalidation across
///     multiple servers in a load-balanced environment.
/// </remarks>
public abstract class CacheRefresherNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CacheRefresherNotification"/> class.
    /// </summary>
    /// <param name="messageObject">The payload containing information about what to refresh.</param>
    /// <param name="messageType">The type of cache refresh operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageObject"/> is null.</exception>
    public CacheRefresherNotification(object messageObject, MessageType messageType)
    {
        MessageObject = messageObject ?? throw new ArgumentNullException(nameof(messageObject));
        MessageType = messageType;
    }

    /// <summary>
    ///     Gets the payload containing information about what to refresh.
    /// </summary>
    public object MessageObject { get; }

    /// <summary>
    ///     Gets the type of cache refresh operation.
    /// </summary>
    public MessageType MessageType { get; }
}
