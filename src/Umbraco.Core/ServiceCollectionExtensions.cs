using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Core.Composing;

namespace Umbraco.Core
{
    public static class ServiceCollectionExtensions
    {
        public static void AddUnique<TService, TImplementing>(this IServiceCollection services)
            where TService : class
            where TImplementing : class, TService
            => services.Replace(ServiceDescriptor.Singleton<TService, TImplementing>());

        /// <summary>
        /// Registers a unique service as a single instance implementing two interfaces.
        /// </summary>
        /// <remarks>
        /// Hat-tip: https://stackoverflow.com/a/55402016/489433
        /// </remarks>
        public static void AddUnique<TService1, TService2, TImplementing>(this IServiceCollection services)
            where TService1 : class
            where TService2 : class
            where TImplementing : class, TService1, TService2
        {
            services.Replace(ServiceDescriptor.Singleton<TService1, TImplementing>());
            services.Replace(ServiceDescriptor.Singleton<TService2, TImplementing>(x => (TImplementing)x.GetService<TService1>()));
        }

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
            => services.Replace(ServiceDescriptor.Singleton<TService>(instance));

        public static IServiceCollection AddLazySupport(this IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Transient(typeof(Lazy<>), typeof(LazyResolve<>)));
            return services;
        }
    }
}
