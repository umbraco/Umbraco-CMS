using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddUnique<TService, TImplementing>(this IServiceCollection services)
            where TService : class
            where TImplementing : class, TService
            => services.Replace(ServiceDescriptor.Singleton<TService, TImplementing>());

        /// <summary>
        /// Registers a singleton instance against multiple interfaces.
        /// </summary>
        public static void AddMultipleUnique<TService1, TService2, TImplementing>(this IServiceCollection services)
            where TService1 : class
            where TService2 : class
            where TImplementing : class, TService1, TService2
        {
            services.AddUnique<TService1, TImplementing>();
            services.AddUnique<TService2>(factory => (TImplementing)factory.GetRequiredService<TService1>());
        }

        // TODO(V11): Remove this function.
        [Obsolete("This method is functionally equivalent to AddSingleton<TImplementing>() please use that instead.")]
        public static void AddUnique<TImplementing>(this IServiceCollection services)
            where TImplementing : class
            => services.Replace(ServiceDescriptor.Singleton<TImplementing, TImplementing>());

        /// <summary>
        /// Registers a unique service with an implementation factory.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public static void AddUnique<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory)
            where TService : class
            => services.Replace(ServiceDescriptor.Singleton(factory));

        /// <summary>
        /// Registers a unique service with an implementing instance.
        /// </summary>
        /// <remarks>Unique services have one single implementation, and a Singleton lifetime.</remarks>
        public static void AddUnique(this IServiceCollection services, Type serviceType, object instance)
            => services.Replace(ServiceDescriptor.Singleton(serviceType, instance));

        /// <summary>
        /// Registers a unique service with an implementing instance.
        /// </summary>
        public static void AddUnique<TService>(this IServiceCollection services, TService instance)
            where TService : class
            => services.Replace(ServiceDescriptor.Singleton(instance));

        internal static IServiceCollection AddLazySupport(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Transient(typeof(Lazy<>), typeof(LazyResolve<>)));
            return services;
        }
    }
}
