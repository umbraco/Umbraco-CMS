// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
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
            var descriptor = new UniqueServiceDescriptor(typeof(INotificationHandler<TNotification>), typeof(TNotificationHandler), ServiceLifetime.Transient);

            // TODO: Waiting on feedback here https://github.com/umbraco/Umbraco-CMS/pull/9556/files#r548365396 about whether
            // we perform this duplicate check or not.
            if (!builder.Services.Contains(descriptor))
            {
                builder.Services.Add(descriptor);
            }

            return builder;
        }

        // This is required because the default implementation doesn't implement Equals or GetHashCode.
        // see: https://github.com/dotnet/runtime/issues/47262
        private class UniqueServiceDescriptor : ServiceDescriptor, IEquatable<UniqueServiceDescriptor>
        {
            public UniqueServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
                : base(serviceType, implementationType, lifetime)
            {
            }

            public override bool Equals(object obj) => Equals(obj as UniqueServiceDescriptor);
            public bool Equals(UniqueServiceDescriptor other) => other != null && Lifetime == other.Lifetime && EqualityComparer<Type>.Default.Equals(ServiceType, other.ServiceType) && EqualityComparer<Type>.Default.Equals(ImplementationType, other.ImplementationType) && EqualityComparer<object>.Default.Equals(ImplementationInstance, other.ImplementationInstance) && EqualityComparer<Func<IServiceProvider, object>>.Default.Equals(ImplementationFactory, other.ImplementationFactory);

            public override int GetHashCode()
            {
                int hashCode = 493849952;
                hashCode = hashCode * -1521134295 + Lifetime.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(ServiceType);
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(ImplementationType);
                hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(ImplementationInstance);
                hashCode = hashCode * -1521134295 + EqualityComparer<Func<IServiceProvider, object>>.Default.GetHashCode(ImplementationFactory);
                return hashCode;
            }
        }

    }
}
