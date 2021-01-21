// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Events;

namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Contains extensions methods for <see cref="IUmbracoBuilder"/> used for registering event handlers.
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Registers a notification handler against the Umbraco service collection.
        /// </summary>
        /// <typeparam name="TNotification">The type of notification.</typeparam>
        /// <typeparam name="TNotificationHandler">The type of notificiation handler.</typeparam>
        /// <param name="builder">The Umbraco builder.</param>
        /// <returns>The <see cref="IUmbracoBuilder"/>.</returns>
        public static IUmbracoBuilder AddNotificationHandler<TNotification, TNotificationHandler>(this IUmbracoBuilder builder)
            where TNotificationHandler : INotificationHandler<TNotification>
            where TNotification : INotification
        {
            // Register the handler as transient. This ensures that anything can be injected into it.
            var descriptor = new ServiceDescriptor(typeof(INotificationHandler<TNotification>), typeof(TNotificationHandler), ServiceLifetime.Transient);

            if (!builder.Services.Contains(descriptor))
            {
                builder.Services.Add(descriptor);
            }

            return builder;
        }

        /// <summary>
        /// Registers a notification async handler against the Umbraco service collection.
        /// </summary>
        /// <typeparam name="TNotification">The type of notification.</typeparam>
        /// <typeparam name="TNotificationAsyncHandler">The type of notification async handler.</typeparam>
        /// <param name="builder">The Umbraco builder.</param>
        /// <returns>The <see cref="IUmbracoBuilder"/>.</returns>
        public static IUmbracoBuilder AddNotificationAsyncHandler<TNotification, TNotificationAsyncHandler>(this IUmbracoBuilder builder)
            where TNotificationAsyncHandler : INotificationAsyncHandler<TNotification>
            where TNotification : INotification
        {
            // Register the handler as transient. This ensures that anything can be injected into it.
            var descriptor = new ServiceDescriptor(typeof(INotificationAsyncHandler<TNotification>), typeof(TNotificationAsyncHandler), ServiceLifetime.Transient);

            if (!builder.Services.Contains(descriptor))
            {
                builder.Services.Add(descriptor);
            }

            return builder;
        }
    }
}
