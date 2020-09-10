using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Umbraco.Core.Composing
{
    public static class UmbracoServiceCollectionExtensions
    {
        public static ServiceLifetime ToServiceLifetime(this Lifetime lifetime)
        {
            return lifetime switch
            {
                Lifetime.Request => ServiceLifetime.Scoped,
                Lifetime.Scope => ServiceLifetime.Scoped,
                Lifetime.Singleton => ServiceLifetime.Singleton,
                Lifetime.Transient => ServiceLifetime.Transient,
                _ => throw new NotImplementedException($"Unknown lifetime {lifetime}")
            };
        }

        public static void AddUnique<TService>(this IServiceCollection services)
            where TService : class
        {
            services.Replace(ServiceDescriptor.Singleton<TService, TService>());
        }

        public static void AddUnique<TService, TTarget>(this IServiceCollection services)
            where TService : class
            where TTarget : class, TService
        {
            services.Replace(ServiceDescriptor.Singleton<TService, TTarget>());
        }

        public static void AddUnique<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory)
            where TService : class
        {
            services.Replace(ServiceDescriptor.Singleton<TService, TService>(factory));
        }

        public static void AddUnique<TService>(this IServiceCollection services, TService instance)
            where TService : class
        {
            services.Replace(ServiceDescriptor.Singleton<TService, TService>(sp => instance));
        }

        public static void AddUnique<TService, TTarget>(this IServiceCollection services, Func<IServiceProvider, TTarget> factory)
            where TService : class
            where TTarget : class, TService
        {
            services.Replace(ServiceDescriptor.Singleton<TService, TTarget>(factory));
        }
    }
}
