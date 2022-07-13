// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Core.Events;

/// <content>
///     Contains types and methods that allow publishing general notifications.
/// </content>
public partial class EventAggregator : IEventAggregator
{
    private static readonly ConcurrentDictionary<Type, NotificationAsyncHandlerWrapper> NotificationAsyncHandlers
        = new();

    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> NotificationHandlers = new();

    private Task PublishNotificationAsync(INotification notification, CancellationToken cancellationToken = default)
    {
        Type notificationType = notification.GetType();
        NotificationAsyncHandlerWrapper asyncHandler = NotificationAsyncHandlers.GetOrAdd(
            notificationType,
            t =>
            {
                var value = Activator.CreateInstance(
                    typeof(NotificationAsyncHandlerWrapperImpl<>).MakeGenericType(notificationType));
                return value is not null
                    ? (NotificationAsyncHandlerWrapper)value
                    : throw new InvalidCastException("Activator could not create instance of NotificationHandler");
            });

        return asyncHandler.HandleAsync(notification, cancellationToken, _serviceFactory, PublishCoreAsync);
    }

    private void PublishNotification(INotification notification)
    {
        Type notificationType = notification.GetType();
        NotificationHandlerWrapper? asyncHandler = NotificationHandlers.GetOrAdd(
            notificationType,
            t =>
            {
                var value = Activator.CreateInstance(
                    typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(notificationType));
                return value is not null
                    ? (NotificationHandlerWrapper)value
                    : throw new InvalidCastException("Activator could not create instance of NotificationHandler");
            });

        asyncHandler?.Handle(notification, _serviceFactory, PublishCore);
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
        Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task>
            publish);
}

internal class NotificationAsyncHandlerWrapperImpl<TNotification> : NotificationAsyncHandlerWrapper
    where TNotification : INotification
{
    /// <remarks>
    ///     <para>
    ///         Background - During v9 build we wanted an in-process message bus to facilitate removal of the old static event
    ///         handlers. <br />
    ///         Instead of taking a dependency on MediatR we (the community) implemented our own using MediatR as inspiration.
    ///     </para>
    ///     <para>
    ///         Some things worth knowing about MediatR.
    ///         <list type="number">
    ///             <item>
    ///                 All handlers are by default registered with transient lifetime, but can easily depend on services
    ///                 with state.
    ///             </item>
    ///             <item>
    ///                 Both the Mediatr instance and its handler resolver are registered transient and as such it is always
    ///                 possible to depend on scoped services in a handler.
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         Our EventAggregator started out registered with a transient lifetime but later (before initial release) the
    ///         registration was changed to singleton, presumably
    ///         because there are a lot of singleton services in Umbraco which like to publish notifications and it's a pain to
    ///         use scoped services from a singleton.
    ///         <br />
    ///         The problem with a singleton EventAggregator is it forces handlers to create a service scope and service locate
    ///         any scoped services
    ///         they wish to make use of e.g. a unit of work (think entity framework DBContext).
    ///     </para>
    ///     <para>
    ///         Moving forwards it probably makes more sense to register EventAggregator transient but doing so now would mean
    ///         an awful lot of service location to avoid breaking changes.
    ///         <br />
    ///         For now we can do the next best thing which is to create a scope for each published notification, thus enabling
    ///         the transient handlers to take a dependency on a scoped service.
    ///     </para>
    ///     <para>
    ///         Did discuss using HttpContextAccessor/IScopedServiceProvider to enable sharing of scopes when publisher has
    ///         http context,
    ///         but decided against because it's inconsistent with what happens in background threads and will just cause
    ///         confusion.
    ///     </para>
    /// </remarks>
    public override Task HandleAsync(
        INotification notification,
        CancellationToken cancellationToken,
        ServiceFactory serviceFactory,
        Func<IEnumerable<Func<INotification, CancellationToken, Task>>, INotification, CancellationToken, Task> publish)
    {
        // Create a new service scope from which to resolve handlers and ensure it's disposed when it goes out of scope.
        // TODO: go back to using ServiceFactory to resolve
        IServiceScopeFactory scopeFactory = serviceFactory.GetInstance<IServiceScopeFactory>();
        using IServiceScope scope = scopeFactory.CreateScope();
        IServiceProvider container = scope.ServiceProvider;

        IEnumerable<Func<INotification, CancellationToken, Task>> handlers = container
            .GetServices<INotificationAsyncHandler<TNotification>>()
            .Select(x => new Func<INotification, CancellationToken, Task>(
                (theNotification, theToken) =>
                    x.HandleAsync((TNotification)theNotification, theToken)));

        return publish(handlers, notification, cancellationToken);
    }
}

internal class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    /// <remarks>
    ///     See remarks on <see cref="NotificationAsyncHandlerWrapperImpl{T}.HandleAsync" /> for explanation on
    ///     what's going on with the IServiceProvider stuff here.
    /// </remarks>
    public override void Handle(
        INotification notification,
        ServiceFactory serviceFactory,
        Action<IEnumerable<Action<INotification>>, INotification> publish)
    {
        // Create a new service scope from which to resolve handlers and ensure it's disposed when it goes out of scope.
        // TODO: go back to using ServiceFactory to resolve
        IServiceScopeFactory scopeFactory = serviceFactory.GetInstance<IServiceScopeFactory>();
        using IServiceScope scope = scopeFactory.CreateScope();
        IServiceProvider container = scope.ServiceProvider;

        IEnumerable<Action<INotification>> handlers = container
            .GetServices<INotificationHandler<TNotification>>()
            .Select(x => new Action<INotification>(
                theNotification =>
                    x.Handle((TNotification)theNotification)));

        publish(handlers, notification);
    }
}
