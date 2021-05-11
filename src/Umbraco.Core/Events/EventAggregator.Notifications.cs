// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events
{
    /// <content>
    /// Contains types and methods that allow publishing general notifications.
    /// </content>
    public partial class EventAggregator : IEventAggregator
    {
        private static readonly ConcurrentDictionary<Type, NotificationAsyncHandlerWrapper> s_notificationAsyncHandlers
            = new ConcurrentDictionary<Type, NotificationAsyncHandlerWrapper>();

        private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> s_notificationHandlers
            = new ConcurrentDictionary<Type, NotificationHandlerWrapper>();

        private Task PublishNotificationAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            Type notificationType = notification.GetType();
            NotificationAsyncHandlerWrapper asyncHandler = s_notificationAsyncHandlers.GetOrAdd(
                notificationType,
                t => (NotificationAsyncHandlerWrapper)Activator.CreateInstance(typeof(NotificationAsyncHandlerWrapperImpl<>).MakeGenericType(notificationType)));

            return asyncHandler.HandleAsync(notification, cancellationToken, _serviceFactory, PublishCoreAsync);
        }

        private void PublishNotification(INotification notification)
        {
            Type notificationType = notification.GetType();
            NotificationHandlerWrapper asyncHandler = s_notificationHandlers.GetOrAdd(
                notificationType,
                t => (NotificationHandlerWrapper)Activator.CreateInstance(typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(notificationType)));

            asyncHandler.Handle(notification, _serviceFactory, PublishCore);
        }

        private async Task PublishCoreAsync(
            IEnumerable<Func<INotification, CancellationToken, Task>> allHandlers,
            INotification notification,
            CancellationToken cancellationToken)
        {
            foreach (Func<INotification, CancellationToken, Task> handler in allHandlers)
            {
                await handler(notification, cancellationToken).ConfigureAwait(false);
            }
        }

        private void PublishCore(
            IEnumerable<Action<INotification>> allHandlers,
            INotification notification)
        {
            foreach (Action<INotification> handler in allHandlers)
            {
                handler(notification);
            }
        }
    }

    internal abstract class NotificationHandlerWrapper
    {
        public abstract void Handle(
            INotification notification,
            ServiceFactory serviceFactory,
            Action<IEnumerable<Action<INotification>>, INotification> publish);
    }

    internal abstract class NotificationAsyncHandlerWrapper
    {
        public abstract Task HandleAsync(
            INotification notification,
            CancellationToken cancellationToken,
            ServiceFactory serviceFactory,
            Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish);
    }

    internal class NotificationAsyncHandlerWrapperImpl<TNotification> : NotificationAsyncHandlerWrapper
        where TNotification : INotification
    {
        public override Task HandleAsync(
            INotification notification,
            CancellationToken cancellationToken,
            ServiceFactory serviceFactory,
            Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish)
        {
            IEnumerable<Func<INotification, CancellationToken, Task>> handlers = serviceFactory
                .GetInstances<INotificationAsyncHandler<TNotification>>()
                .Select(x => new Func<INotification, CancellationToken, Task>(
                    (theNotification, theToken) =>
                    x.HandleAsync((TNotification)theNotification, theToken)));

            return publish(handlers, notification, cancellationToken);
        }
    }

    internal class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        public override void Handle(
            INotification notification,
            ServiceFactory serviceFactory,
            Action<IEnumerable<Action<INotification>>, INotification> publish)
        {
            IEnumerable<Action<INotification>> handlers = serviceFactory
                .GetInstances<INotificationHandler<TNotification>>()
                .Select(x => new Action<INotification>(
                    (theNotification) =>
                        x.Handle((TNotification)theNotification)));

            publish(handlers, notification);
        }
    }
}
