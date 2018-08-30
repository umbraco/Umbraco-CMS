using System;
using System.Linq;
using LightInject;

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Provides extensions to LightInject.
    /// </summary>
    public static class LightInjectExtensions
    {
        /// <summary>
        /// Registers the TService with the factory that describes the dependencies of the service, as a singleton.
        /// </summary>
        public static void RegisterSingleton<TService>(this IServiceRegistry container, Func<IServiceFactory, TService> factory, string serviceName)
        {
            var registration = container.GetAvailableService<TService>(serviceName);
            if (registration == null)
            {
                container.Register(factory, serviceName, new PerContainerLifetime());
            }
            else
            {
                if (registration.Lifetime is PerContainerLifetime == false)
                    throw new InvalidOperationException("Existing registration lifetime is not PerContainer.");
                UpdateRegistration(registration, null, factory);
            }
        }

        /// <summary>
        /// Registers a service with an implementation as a singleton.
        /// </summary>
        public static void RegisterSingleton<TService, TImplementation>(this IServiceRegistry container)
            where TImplementation : TService
        {
            container.RegisterSingleton(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Registers a service with an implementation as a singleton.
        /// </summary>
        public static void RegisterSingleton(this IServiceRegistry container, Type serviceType, Type implementingType)
        {
            var registration = container.GetAvailableService(serviceType);

            if (registration == null)
            {
                container.Register(serviceType, implementingType, new PerContainerLifetime());
            }
            else
            {
                if (registration.Lifetime is PerContainerLifetime == false)
                    throw new InvalidOperationException("Existing registration lifetime is not PerContainer.");
                UpdateRegistration(registration, implementingType, null);
            }
        }

        /// <summary>
        /// Registers a service with a named implementation as a singleton.
        /// </summary>
        public static void RegisterSingleton(this IServiceRegistry container, Type serviceType, Type implementingType, string name)
        {
            var registration = container.AvailableServices.FirstOrDefault(x => x.ServiceType == serviceType && x.ServiceName == name);

            if (registration == null)
            {
                container.Register(serviceType, implementingType, name, new PerContainerLifetime());
            }
            else
            {
                if (registration.Lifetime is PerContainerLifetime == false)
                    throw new InvalidOperationException("Existing registration lifetime is not PerContainer.");
                UpdateRegistration(registration, implementingType, null);
            }
        }

        /// <summary>
        /// Registers a concrete type as a singleton service.
        /// </summary>
        public static void RegisterSingleton<TImplementation>(this IServiceRegistry container)
        {
            container.RegisterSingleton(typeof(TImplementation));
        }

        /// <summary>
        /// Registers a concrete type as a singleton service.
        /// </summary>
        public static void RegisterSingleton(this IServiceRegistry container, Type serviceType)
        {
            var registration = container.GetAvailableService(serviceType);
            if (registration == null)
            {
                container.Register(serviceType, new PerContainerLifetime());
            }
            else
            {
                if (registration.Lifetime is PerContainerLifetime == false)
                    throw new InvalidOperationException("Existing registration lifetime is not PerContainer.");
                UpdateRegistration(registration, serviceType, null);
            }
        }

        /// <summary>
        /// Registers the TService with the factory that describes the dependencies of the service, as a singleton.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="container"></param>
        /// <param name="factory"></param>
        public static void RegisterSingleton<TService>(this IServiceRegistry container, Func<IServiceFactory, TService> factory)
        {
            var registration = container.GetAvailableService<TService>();
            if (registration == null)
                container.Register(factory, new PerContainerLifetime());
            else
                UpdateRegistration(registration, null, factory);
        }

        // note - what's below ALSO applies to non-singleton ie transient services
        //
        // see https://github.com/seesharper/LightInject/issues/133
        //
        // note: we *could* use tracking lifetimes for singletons to ensure they have not been resolved
        // already but that would not work for transient as the transient lifetime is null (and that is
        // optimized in LightInject)
        //
        // also, RegisterSingleton above is dangerous because ppl could still use Register with a
        // PerContainerLifetime and it will not work + the default Register would not work either for other
        // lifetimes
        //
        // all in all, not sure we want to let ppl have direct access to the container
        // we might instead want to expose some methods in UmbracoComponentBase or whatever?

        /// <summary>
        /// Updates a registration.
        /// </summary>
        private static void UpdateRegistration(ServiceRegistration registration, Type implementingType, Delegate factoryExpression)
        {
            // if the container has compiled already then the registrations have been captured,
            // and re-registering - although updating available services - does not modify the
            // output of GetInstance
            //
            // so we have to rely on different methods
            //
            // assuming the service has NOT been resolved, both methods below work, but the first
            // one looks simpler. it would be good to check whether the service HAS been resolved
            // but I am not sure how to do it right now, depending on the lifetime
            //
            // if the service HAS been resolved then updating is probably a bad idea

            // not sure which is best? that one works, though, and looks simpler
            registration.ImplementingType = implementingType;
            registration.FactoryExpression = factoryExpression;

            //container.Override(
            //    r => r.ServiceType == typeof (TService) && (registration.ServiceName == null || r.ServiceName == registration.ServiceName),
            //    (f, r) =>
            //    {
            //        r.ImplementingType = implementingType;
            //        r.FactoryExpression = factoryExpression;
            //        return r;
            //    });
        }

        /// <summary>
        /// Gets the unique available service registration for a service type.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The unique service registration for the service type.</returns>
        /// <remarks>Can return <c>null</c>, but throws if more than one registration exist for the service type.</remarks>
        private static ServiceRegistration GetAvailableService<TService>(this IServiceRegistry container)
        {
            var typeofTService = typeof(TService);
            return container.AvailableServices.SingleOrDefault(x => x.ServiceType == typeofTService);
        }

        /// <summary>
        /// Gets the unique available service registration for a service type.
        /// </summary>
        private static ServiceRegistration GetAvailableService(this IServiceRegistry container, Type serviceType)
        {
            return container.AvailableServices.SingleOrDefault(x => x.ServiceType == serviceType);
        }

        /// <summary>
        /// Gets the unique available service registration for a service type and a name.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="name">The name.</param>
        /// <returns>The unique service registration for the service type and the name.</returns>
        /// <remarks>Can return <c>null</c>, but throws if more than one registration exist for the service type and the name.</remarks>
        private static ServiceRegistration GetAvailableService<TService>(this IServiceRegistry container, string name)
        {
            var typeofTService = typeof(TService);
            return container.AvailableServices.SingleOrDefault(x => x.ServiceType == typeofTService && x.ServiceName == name);
        }
    }
}
