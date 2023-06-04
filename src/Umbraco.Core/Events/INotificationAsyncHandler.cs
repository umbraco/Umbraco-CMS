// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events;

/// <summary>
/// Defines a handler for a async notification.
/// </summary>
/// <typeparam name="TNotification">The type of notification being handled.</typeparam>
public interface INotificationAsyncHandler<in TNotification> : INotificationHandler
    where TNotification : INotification
{
    /// <summary>
    /// Handles a notification.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken);

    /// <summary>
    /// Handles the notifications.
    /// </summary>
    /// <param name="notifications">The notifications.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    Task HandleAsync(IEnumerable<TNotification> notifications, CancellationToken cancellationToken)
        => Task.WhenAll(notifications.Select(x => HandleAsync(x, cancellationToken)));
}
