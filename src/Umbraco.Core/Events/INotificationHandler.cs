﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events
{
    /// <summary>
    /// Defines a handler for a notification.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        /// <summary>
        /// Handles a notification
        /// </summary>
        /// <param name="notification">The notification</param>
        void Handle(TNotification notification);
    }

    /// <summary>
    /// Defines a handler for a async notification.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being handled.</typeparam>
    public interface INotificationAsyncHandler<in TNotification>
        where TNotification : INotification
    {
        /// <summary>
        /// Handles a notification
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
    }
}
