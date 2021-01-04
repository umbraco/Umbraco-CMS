// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            // TODO: Waiting on feedback here for TryAddTransient https://github.com/umbraco/Umbraco-CMS/pull/9556/files#r548365396
            // ... though this will fail tests so it's not the final answer so we'll see where that discussion goes.
            builder.Services.AddTransient(typeof(INotificationHandler<TNotification>), typeof(TNotificationHandler));
            return builder;
        }
    }
}
