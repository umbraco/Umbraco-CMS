// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

/// <summary>
/// A factory method used to resolve all services.
/// For multiple instances, it will resolve against <see cref="IEnumerable{T}" />.
/// </summary>
/// <param name="serviceType">Type of service to resolve.</param>
/// <returns>
/// An instance of type <paramref name="serviceType" />.
/// </returns>
public delegate object ServiceFactory(Type serviceType);

/// <summary>
/// Extensions for <see cref="ServiceFactory" />.
/// </summary>
public static class ServiceFactoryExtensions
{
    /// <summary>
    /// Gets an instance of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="factory">The service factory.</param>
    /// <returns>
    /// The new instance.
    /// </returns>
    public static T GetInstance<T>(this ServiceFactory factory)
        => (T)factory(typeof(T));

    /// <summary>
    /// Gets a collection of instances of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The collection item type to return.</typeparam>
    /// <param name="factory">The service factory.</param>
    /// <returns>
    /// The new instance collection.
    /// </returns>
    public static IEnumerable<T> GetInstances<T>(this ServiceFactory factory)
        => (IEnumerable<T>)factory(typeof(IEnumerable<T>));
}

public partial class EventAggregator : IEventAggregator
{
    private static readonly ConcurrentDictionary<Type, NotificationAsyncHandlerWrapper> _notificationAsyncHandlers = new();
    private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> _notificationHandlers = new();
    private readonly ServiceFactory _serviceFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventAggregator" /> class.
    /// </summary>
    /// <param name="serviceFactory">The service instance factory.</param>
    public EventAggregator(ServiceFactory serviceFactory)
        => _serviceFactory = serviceFactory;

    private void PublishNotifications<TNotification, TNotificationHandler>(IEnumerable<TNotification> notifications)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler
    {
        foreach (var notificationsByType in ChunkByType(notifications))
        {
            var notificationHandler = _notificationHandlers.GetOrAdd(notificationsByType.Key, x =>
            {
                var instance = Activator.CreateInstance(typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(x));

                return instance is not null
                    ? (NotificationHandlerWrapper)instance
                    : throw new InvalidCastException("Activator could not create instance of NotificationHandler");
            });

            notificationHandler.Handle<TNotification, TNotificationHandler>(notificationsByType, _serviceFactory, PublishCore);
        }
    }

    private async Task PublishNotificationsAsync<TNotification, TNotificationHandler>(IEnumerable<TNotification> notifications, CancellationToken cancellationToken = default)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler
    {
        foreach (var notificationsByType in ChunkByType(notifications))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var notificationAsyncHandler = _notificationAsyncHandlers.GetOrAdd(notificationsByType.Key, x =>
            {
                var instance = Activator.CreateInstance(typeof(NotificationAsyncHandlerWrapperImpl<>).MakeGenericType(x));

                return instance is not null
                    ? (NotificationAsyncHandlerWrapper)instance
                    : throw new InvalidCastException("Activator could not create instance of NotificationAsyncHandler.");
            });

            await notificationAsyncHandler.HandleAsync<TNotification, TNotificationHandler>(notificationsByType, cancellationToken, _serviceFactory, PublishCoreAsync);
        }
    }

    private void PublishCore<TNotification>(IEnumerable<Action<IEnumerable<TNotification>>> allHandlers, IEnumerable<TNotification> notifications)
    {
        foreach (Action<IEnumerable<TNotification>> handler in allHandlers)
        {
            handler(notifications);
        }
    }

    private async Task PublishCoreAsync<TNotification>(IEnumerable<Func<IEnumerable<TNotification>, CancellationToken, Task>> allHandlers, IEnumerable<TNotification> notifications, CancellationToken cancellationToken)
    {
        foreach (Func<IEnumerable<TNotification>, CancellationToken, Task> handler in allHandlers)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await handler(notifications, cancellationToken).ConfigureAwait(false);
        }
    }

    private static IEnumerable<IGrouping<Type, T>> ChunkByType<T>(IEnumerable<T> source)
        where T : notnull
    {
        IEnumerator<T> enumerator = source.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            // Skip empty source
            yield break;
        }

        // Create first grouping
        Type previousType = enumerator.Current.GetType();
        var grouping = new ChunkGrouping<Type, T>(previousType)
        {
            enumerator.Current
        };

        // Return chunks when type changes
        while (enumerator.MoveNext())
        {
            // Check against previous type
            Type currentType = enumerator.Current.GetType();
            if (previousType != currentType)
            {
                yield return grouping;

                // Reinitialize to ensure we're always adding to groupings of the same type
                previousType = currentType;
                grouping = new ChunkGrouping<Type, T>(previousType);
            }

            grouping.Add(enumerator.Current);
        }

        // Return final grouping
        yield return grouping;
    }

    private sealed class ChunkGrouping<TKey, TElement> : List<TElement>, IGrouping<TKey, TElement>
    {
        public TKey Key { get; }

        public ChunkGrouping(TKey key)
            => Key = key;
    }
}

internal abstract class NotificationHandlerWrapper
{
    public abstract void Handle<TNotification, TNotificationHandler>(
        IEnumerable<TNotification> notifications,
        ServiceFactory serviceFactory,
        Action<IEnumerable<Action<IEnumerable<TNotification>>>, IEnumerable<TNotification>> publish)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler;
}

internal abstract class NotificationAsyncHandlerWrapper
{
    public abstract Task HandleAsync<TNotification, TNotificationHandler>(
        IEnumerable<TNotification> notifications,
        CancellationToken cancellationToken,
        ServiceFactory serviceFactory,
        Func<IEnumerable<Func<IEnumerable<TNotification>, CancellationToken, Task>>, IEnumerable<TNotification>, CancellationToken, Task> publish)
        where TNotification : INotification
        where TNotificationHandler : INotificationHandler;
}

internal class NotificationAsyncHandlerWrapperImpl<TNotificationType> : NotificationAsyncHandlerWrapper
    where TNotificationType : INotification
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
    public override async Task HandleAsync<TNotification, TNotificationHandler>(
        IEnumerable<TNotification> notifications,
        CancellationToken cancellationToken,
        ServiceFactory serviceFactory,
        Func<IEnumerable<Func<IEnumerable<TNotification>, CancellationToken, Task>>, IEnumerable<TNotification>, CancellationToken, Task> publish)
    {
        // Create a new service scope from which to resolve handlers and ensure it's disposed when it goes out of scope.
        // TODO: go back to using ServiceFactory to resolve
        IServiceScopeFactory scopeFactory = serviceFactory.GetInstance<IServiceScopeFactory>();
        using IServiceScope scope = scopeFactory.CreateScope();
        IServiceProvider container = scope.ServiceProvider;

        IEnumerable<Func<IEnumerable<TNotification>, CancellationToken, Task>> handlers = container
            .GetServices<INotificationAsyncHandler<TNotificationType>>()
            .Where(x => x is TNotificationHandler)
            .Select(x => new Func<IEnumerable<TNotification>, CancellationToken, Task>(
                (handlerNotifications, handlerCancellationToken) => x.HandleAsync(handlerNotifications.Cast<TNotificationType>(), handlerCancellationToken)));

        await publish(handlers, notifications, cancellationToken);
    }
}

internal class NotificationHandlerWrapperImpl<TNotificationType> : NotificationHandlerWrapper
    where TNotificationType : INotification
{
    /// <remarks>
    ///     See remarks on <see cref="NotificationAsyncHandlerWrapperImpl{T}.HandleAsync" /> for explanation on
    ///     what's going on with the IServiceProvider stuff here.
    /// </remarks>
    public override void Handle<TNotification, TNotificationHandler>(
        IEnumerable<TNotification> notifications,
        ServiceFactory serviceFactory,
        Action<IEnumerable<Action<IEnumerable<TNotification>>>, IEnumerable<TNotification>> publish)
    {
        // Create a new service scope from which to resolve handlers and ensure it's disposed when it goes out of scope.
        // TODO: go back to using ServiceFactory to resolve
        IServiceScopeFactory scopeFactory = serviceFactory.GetInstance<IServiceScopeFactory>();
        using IServiceScope scope = scopeFactory.CreateScope();
        IServiceProvider container = scope.ServiceProvider;

        IEnumerable<Action<IEnumerable<TNotification>>> handlers = container
            .GetServices<INotificationHandler<TNotificationType>>()
            .Where(x => x is TNotificationHandler)
            .Select(x => new Action<IEnumerable<TNotification>>(handlerNotifications => x.Handle(handlerNotifications.Cast<TNotificationType>())));

        publish(handlers, notifications);
    }
}
