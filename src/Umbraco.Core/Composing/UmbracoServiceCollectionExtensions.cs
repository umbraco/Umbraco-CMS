using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Umbraco.Core.Composing
{
    public static class UmbracoServiceCollectionExtensions
    {
        private static ServiceLifetime ToServiceLifetime(this Lifetime lifetime)
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

        public static void Register(this IServiceCollection services, Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            services.Add(new ServiceDescriptor(serviceType, serviceType, lifetime.ToServiceLifetime()));
        }

        public static void Register(this IServiceCollection services, object instance)
        {
            services.AddSingleton(instance);
        }

        public static void Register<TService>(this IServiceCollection services, Lifetime lifetime = Lifetime.Transient)
        {
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), lifetime.ToServiceLifetime()));
        }

        public static void Register<TService, TTarget>(this IServiceCollection services, Lifetime lifetime = Lifetime.Transient)
        {
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TTarget), lifetime.ToServiceLifetime()));
        }
        
        public static void Register(this IServiceCollection services, Type serviceType, Type implementingType,
            Lifetime lifetime = Lifetime.Transient)
        {
            services.Add(new ServiceDescriptor(serviceType, implementingType, lifetime.ToServiceLifetime()));
        }


        public static void Register<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            services.Add(new ServiceDescriptor(typeof(TService), factory, lifetime.ToServiceLifetime()));
        }
        public static void Register(this IServiceCollection services, Type serviceType, object instance)
        {
            services.Add(new ServiceDescriptor(serviceType, instance));
        }
        public static void RegisterFor<TService, TTarget>(this IServiceCollection services, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            services.Replace(new ServiceDescriptor(typeof(TService), typeof(TTarget), lifetime.ToServiceLifetime()));
        }
        public static void RegisterFor(this IServiceCollection services, Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
        {
            services.Replace(new ServiceDescriptor(serviceType, implementingType, lifetime.ToServiceLifetime()));
        }
        public static void RegisterFor<TService>(this IServiceCollection services, Type implementingType, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            services.Replace(new ServiceDescriptor(typeof(TService), implementingType, lifetime.ToServiceLifetime()));
        }
        public static void RegisterFor<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            services.Replace(new ServiceDescriptor(typeof(TService), factory, lifetime.ToServiceLifetime()));
        }
        public static void RegisterFor<TService>(this IServiceCollection services, TService instance)
            where TService : class
        {
            services.Replace(new ServiceDescriptor(typeof(TService), instance));
        }
        public static void RegisterFor(this IServiceCollection services, Type serviceType, object instance)
        {
            services.Replace(new ServiceDescriptor(serviceType, instance));
        }

        // TODO: MSDI, this needs to die
        public static object GetInstance(this IServiceProvider container, Type type)
        {
            return container.GetRequiredService(type);
        }

        // TODO: MSDI, this needs to die
        public static TService GetInstance<TService>(this IServiceProvider container)
        {
            return (TService) container.GetRequiredService(typeof(TService));
        }

        // TODO: MSDI, this needs to die
        public static TService GetInstance<TService>(this IServiceProvider container, params object[] parameters)
        {
            return ActivatorUtilities.CreateInstance<TService>(container, parameters);
        }

        // TODO: MSDI, this needs to die
        public static object GetInstance(this IServiceProvider container, Type serviceType, params object[] parameters)
        {
            return ActivatorUtilities.CreateInstance(container, serviceType, parameters);
        }

        // TODO: MSDI, this needs to die
        public static TService TryGetInstance<TService>(this IServiceProvider container)
         where TService : class
        {
            return container.GetService(typeof(TService)) as TService;
        }
    }
}
