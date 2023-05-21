// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Defines an object that channels events from multiple objects into a single object
///     to simplify registration for clients.
/// </summary>
public interface IEventAggregator
{
    /// <summary>
    ///     Asynchronously send a notification to multiple handlers of both sync and async
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <param name="notification">The notification object.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>A task that represents the publish operation.</returns>
    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    ///     Synchronously send a notification to multiple handlers of both sync and async
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <param name="notification">The notification object.</param>
    void Publish<TNotification>(TNotification notification)
        where TNotification : INotification;

    /// <summary>
    ///     Publishes a cancelable notification to the notification subscribers
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <param name="notification"></param>
    /// <returns>True if the notification was cancelled by a subscriber, false otherwise</returns>
    bool PublishCancelable<TCancelableNotification>(TCancelableNotification notification)
        where TCancelableNotification : ICancelableNotification;

    /// <summary>
    ///     Publishes a cancelable notification async to the notification subscribers
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <param name="notification"></param>
    /// <returns>True if the notification was cancelled by a subscriber, false otherwise</returns>
    Task<bool> PublishCancelableAsync<TCancelableNotification>(TCancelableNotification notification)
        where TCancelableNotification : ICancelableNotification;
}
