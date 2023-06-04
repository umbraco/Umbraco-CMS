// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events;

/// <summary>
/// Marker interface for notification handlers.
/// </summary>
public interface INotificationHandler
{ }

/// <summary>
/// Defines a handler for a notification.
/// </summary>
/// <typeparam name="TNotification">The type of notification being handled.</typeparam>
public interface INotificationHandler<in TNotification> : INotificationHandler
    where TNotification : INotification
{
    /// <summary>
    /// Handles a notification.
    /// </summary>
    /// <param name="notification">The notification.</param>
    void Handle(TNotification notification);

    /// <summary>
    /// Handles the notifications.
    /// </summary>
    /// <param name="notifications">The notifications.</param>
    void Handle(IEnumerable<TNotification> notifications)
    {
        foreach (TNotification notification in notifications)
        {
            Handle(notification);
        }
    }
}
