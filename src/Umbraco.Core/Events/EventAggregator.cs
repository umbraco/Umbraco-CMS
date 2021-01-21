// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// A factory method used to resolve all services.
    /// For multiple instances, it will resolve against <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <param name="serviceType">Type of service to resolve.</param>
    /// <returns>An instance of type <paramref name="serviceType" />.</returns>
    public delegate object ServiceFactory(Type serviceType);

    /// <inheritdoc/>
    public partial class EventAggregator : IEventAggregator
    {
        private readonly ServiceFactory _serviceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventAggregator"/> class.
        /// </summary>
        /// <param name="serviceFactory">The service instance factory.</param>
        public EventAggregator(ServiceFactory serviceFactory)
            => _serviceFactory = serviceFactory;

        /// <inheritdoc/>
        public Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            // TODO: Introduce codegen efficient Guard classes to reduce noise.
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            PublishNotification(notification);
            return PublishNotificationAsync(notification, cancellationToken);
        }

        /// <inheritdoc/>
        public void Publish<TNotification>(TNotification notification)
            where TNotification : INotification
        {
            // TODO: Introduce codegen efficient Guard classes to reduce noise.
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            PublishNotification(notification);
            Task.WaitAll(PublishNotificationAsync(notification));
        }
    }

    /// <summary>
    /// Extensions for <see cref="ServiceFactory"/>.
    /// </summary>
    public static class ServiceFactoryExtensions
    {
        /// <summary>
        /// Gets an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="factory">The service factory.</param>
        /// <returns>The new instance.</returns>
        public static T GetInstance<T>(this ServiceFactory factory)
            => (T)factory(typeof(T));

        /// <summary>
        /// Gets a collection of instances of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The collection item type to return.</typeparam>
        /// <param name="factory">The service factory.</param>
        /// <returns>The new instance collection.</returns>
        public static IEnumerable<T> GetInstances<T>(this ServiceFactory factory)
            => (IEnumerable<T>)factory(typeof(IEnumerable<T>));
    }
}
