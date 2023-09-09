// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

/// <summary>
/// Defines an object that channels events from multiple objects into a single object to simplify registration for clients.
/// </summary>
public interface IEventAggregator
{
    /// <summary>
    /// Synchronously send a notification to multiple handlers of both sync and async.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <param name="notification">The notification object.</param>
    void Publish<TNotification>(TNotification notification) // TODO Convert to extension method
        where TNotification : INotification;

    /// <summary>
    /// Synchronously send a notifications to multiple handlers of both sync and async.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <typeparam name="TNotificationHandler">The type of the notification handler.</typeparam>
    /// <param name="notifications">The notification objects.</param>
    void Publish<TNotification, TNotificationHandler>(IEnumerable<TNotification> notifications)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler;

    /// <summary>
    /// Asynchronously send a notification to multiple handlers of both sync and async.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <param name="notification">The notification object.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// A task that represents the publish operation.
    /// </returns>
    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) // TODO Convert to extension method
        where TNotification : INotification;

    /// <summary>
    /// Asynchronously send a notifications to multiple handlers of both sync and async.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <typeparam name="TNotificationHandler">The type of the notification handler.</typeparam>
    /// <param name="notifications">The notification objects.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// A task that represents the publish operation.
    /// </returns>
    Task PublishAsync<TNotification, TNotificationHandler>(IEnumerable<TNotification> notifications, CancellationToken cancellationToken = default)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler;

    /// <summary>
    /// Publishes a cancelable notification to the notification subscribers.
    /// </summary>
    /// <typeparam name="TCancelableNotification">The type of notification being handled.</typeparam>
    /// <param name="notification">The notification.</param>
    /// <returns>
    ///   <c>true</c> if the notification was cancelled by a subscriber; otherwise, <c>false</c>.
    /// </returns>
    bool PublishCancelable<TCancelableNotification>(TCancelableNotification notification)
        where TCancelableNotification : ICancelableNotification;

    /// <summary>
    /// Publishes a cancelable notification async to the notification subscribers.
    /// </summary>
    /// <typeparam name="TCancelableNotification">The type of notification being handled.</typeparam>
    /// <param name="notification">The notification.</param>
    /// <returns>
    ///   <c>true</c> if the notification was cancelled by a subscriber; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> PublishCancelableAsync<TCancelableNotification>(TCancelableNotification notification)
        where TCancelableNotification : ICancelableNotification;
}

/// <summary>
/// Extension methods for <see cref="IEventAggregator" />.
/// </summary>
public static class EventAggregatorExtensions
{
    /// <summary>
    /// Synchronously send a notifications to multiple handlers of both sync and async.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="notifications">The notification objects.</param>
    public static void Publish<TNotification>(this IEventAggregator eventAggregator, IEnumerable<TNotification> notifications)
        where TNotification : INotification
        => eventAggregator.Publish<TNotification, INotificationHandler>(notifications);

    /// <summary>
    /// Synchronously send a notifications to multiple handlers of both sync and async.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <typeparam name="TNotificationHandler">The type of the notification handler.</typeparam>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="notification">The notification.</param>
    public static void Publish<TNotification, TNotificationHandler>(this IEventAggregator eventAggregator, TNotification notification)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler
        => eventAggregator.Publish<TNotification, TNotificationHandler>(notification.Yield());

    /// <summary>
    /// Asynchronously send a notification to multiple handlers of both sync and async.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="notifications">The notifications.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// A task that represents the publish operation.
    /// </returns>
    public static Task PublishAsync<TNotification>(this IEventAggregator eventAggregator, IEnumerable<TNotification> notifications, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => eventAggregator.PublishAsync<TNotification, INotificationHandler>(notifications, cancellationToken);

    /// <summary>
    /// Asynchronously send a notification to multiple handlers of both sync and async.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    /// <typeparam name="TNotificationHandler">The type of the notification handler.</typeparam>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="notification">The notification object.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>
    /// A task that represents the publish operation.
    /// </returns>
    public static Task PublishAsync<TNotification, TNotificationHandler>(this IEventAggregator eventAggregator, TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler
        => eventAggregator.PublishAsync<TNotification, TNotificationHandler>(notification.Yield(), cancellationToken);
}
