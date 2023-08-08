// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

/// <inheritdoc />
public partial class EventAggregator : IEventAggregator
{
    /// <inheritdoc />
    public void Publish<TNotification>(TNotification notification)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        Publish<TNotification, INotificationHandler>(notification.Yield());
    }

    /// <inheritdoc />
    public void Publish<TNotification, TNotificationHandler>(IEnumerable<TNotification> notifications)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler
    {
        PublishNotifications<TNotification, TNotificationHandler>(notifications);

        Task task = PublishNotificationsAsync<TNotification, TNotificationHandler>(notifications);
        if (task is not null)
        {
            Task.WaitAll(task);
        }
    }

    /// <inheritdoc />
    public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        await PublishAsync<TNotification, INotificationHandler>(notification.Yield(), cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishAsync<TNotification, TNotificationHandler>(IEnumerable<TNotification> notifications, CancellationToken cancellationToken = default)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler
    {
        PublishNotifications<TNotification, TNotificationHandler>(notifications);

        await PublishNotificationsAsync<TNotification, TNotificationHandler>(notifications, cancellationToken);
    }

    /// <inheritdoc />
    public bool PublishCancelable<TCancelableNotification>(TCancelableNotification notification)
        where TCancelableNotification : ICancelableNotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        Publish(notification);

        return notification.Cancel;
    }

    /// <inheritdoc />
    public async Task<bool> PublishCancelableAsync<TCancelableNotification>(TCancelableNotification notification)
        where TCancelableNotification : ICancelableNotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        Task? task = PublishAsync(notification);
        if (task is not null)
        {
            await task;
        }

        return notification.Cancel;
    }
}
