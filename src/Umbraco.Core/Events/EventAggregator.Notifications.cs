using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Core.Events
{
    /// <content>
    /// Contains types and methods that allow publishing general notifications.
    /// </content>
    public partial class EventAggregator : IEventAggregator
    {
        private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> NotificationHandlers
            = new ConcurrentDictionary<Type, NotificationHandlerWrapper>();

        private Task PublishNotificationAsync(INotification notification, CancellationToken cancellationToken = default)
        {
            var notificationType = notification.GetType();
            var handler = NotificationHandlers.GetOrAdd(
                notificationType,
                t => (NotificationHandlerWrapper)Activator.CreateInstance(typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(notificationType)));

            return handler.HandleAsync(notification, cancellationToken, _serviceFactory, PublishCoreAsync);
        }

        private async Task PublishCoreAsync(
            IEnumerable<Func<INotification, CancellationToken, Task>> allHandlers,
            INotification notification, CancellationToken cancellationToken)
        {
            foreach (var handler in allHandlers)
            {
                await handler(notification, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    internal abstract class NotificationHandlerWrapper
    {
        public abstract Task HandleAsync(
            INotification notification,
            CancellationToken cancellationToken,
            ServiceFactory serviceFactory,
            Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish);
    }

    internal class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        public override Task HandleAsync(
            INotification notification,
            CancellationToken cancellationToken,
            ServiceFactory serviceFactory,
            Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish)
        {
            IEnumerable<Func<INotification, CancellationToken, Task>> handlers = serviceFactory
                .GetInstances<INotificationHandler<TNotification>>()
                .Select(x => new Func<INotification, CancellationToken, Task>(
                    (theNotification, theToken) =>
                    x.HandleAsync((TNotification)theNotification, theToken)));

            return publish(handlers, notification, cancellationToken);
        }
    }
}
