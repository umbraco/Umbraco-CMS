using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;

namespace Umbraco.Core.DependencyInjection
{
    internal static class LightInjectExtensions
    {
        /// <summary>
        /// Registers the TService with the factory that describes the dependencies of the service, as a singleton.
        /// </summary>
        public static void RegisterSingleton<TService>(this IServiceRegistry container, Func<IServiceFactory, TService> factory, string serviceName)
        {
            var registration = container.GetAvailableService<TService>(serviceName);
            if (registration == null)
                container.Register(factory, serviceName, new PerContainerLifetime());
            else
                container.UpdateRegistration(registration, null, factory);
        }

        /// <summary>
        /// Registers the TService with the TImplementation as a singleton.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="container"></param>
        public static void RegisterSingleton<TService, TImplementation>(this IServiceRegistry container)
            where TImplementation : TService
        {
            var registration = container.GetAvailableService<TService>();

            if (registration == null)
                container.Register<TService, TImplementation>(new PerContainerLifetime());
            else
                container.UpdateRegistration(registration, typeof(TImplementation), null);
        }

        /// <summary>
        /// Registers a concrete type as a singleton service.
        /// </summary>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="container"></param>
        public static void RegisterSingleton<TImplementation>(this IServiceRegistry container)
        {
            var registration = container.GetAvailableService<TImplementation>();
            if (registration == null)
                container.Register<TImplementation>(new PerContainerLifetime());
            else
                container.UpdateRegistration(registration, typeof(TImplementation), null);
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
                container.UpdateRegistration(registration, null, factory);
        }

        // fixme - what's below ALSO applies to non-singleton ie transient services
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

        private static void UpdateRegistration(this IServiceRegistry container, ServiceRegistration registration, Type implementingType, Delegate factoryExpression)
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

        public static IEnumerable<ServiceRegistration> GetAvailableServices<TService>(this IServiceRegistry container)
        {
            var typeofTService = typeof(TService);
            return container.AvailableServices.Where(x => x.ServiceType == typeofTService);
        }

        public static ServiceRegistration GetAvailableService<TService>(this IServiceRegistry container)
        {
            var typeofTService = typeof(TService);
            return container.AvailableServices.SingleOrDefault(x => x.ServiceType == typeofTService);
        }

        public static ServiceRegistration GetAvailableService<TService>(this IServiceRegistry container, string name)
        {
            var typeofTService = typeof(TService);
            return container.AvailableServices.SingleOrDefault(x => x.ServiceType == typeofTService && x.ServiceName == name);
        }

        /// <summary>
        /// In order for LightInject to deal with enumerables of the same type, each one needs to be registered as their explicit types
        /// </summary>
        /// <typeparam name="TLifetime"></typeparam>
        /// <param name="container"></param>
        /// <param name="implementationTypes"></param>
        /// <remarks>
        /// This works as of 3.0.2.2: https://github.com/seesharper/LightInject/issues/68#issuecomment-70611055
        /// but means that the explicit type is registered, not the implementing type
        /// </remarks>
        public static void RegisterBuilderCollection<TLifetime>(this IServiceContainer container, IEnumerable<Type> implementationTypes)
            where TLifetime : ILifetime, new()
        {
            foreach (var type in implementationTypes)
            {
                container.Register(type, new TLifetime());
            }
        }

        /// <summary>
        /// In order for LightInject to deal with enumerables of the same type, each one needs to be registered as their explicit types
        /// </summary>
        /// <param name="container"></param>
        /// <param name="implementationTypes"></param>
        /// <remarks>
        /// This works as of 3.0.2.2: https://github.com/seesharper/LightInject/issues/68#issuecomment-70611055
        /// but means that the explicit type is registered, not the implementing type
        /// </remarks>
        public static void RegisterBuilderCollection(this IServiceContainer container, IEnumerable<Type> implementationTypes)
        {
            foreach (var type in implementationTypes)
            {
                container.Register(type);
            }
        }

        /// <summary>
        /// Registers an injected collection.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <typeparam name="TCollection">The type of the collection.</typeparam>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="container">A container.</param>
        public static void RegisterBuilderCollection<TBuilder, TCollection, TItem>(this IServiceRegistry container)
            where TBuilder : ICollectionBuilder<TCollection, TItem>
            where TCollection : IBuilderCollection<TItem>
        {
            // register the builder
            container.Register<TBuilder>(new PerContainerLifetime());

            // register the collection
            container.Register(factory => factory.GetInstance<TBuilder>().GetCollection());
        }

        /// <summary>
        /// Registers an injected collection.
        /// </summary>
        /// <typeparam name="TCollection">The type of the collection.</typeparam>
        /// <typeparam name="TBuilder">The type of the builder.</typeparam>
        /// <typeparam name="TLifetime">A lifetime type.</typeparam>
        /// <typeparam name="TItem">The type of the items.</typeparam>
        /// <param name="container">A container.</param>
        public static void RegisterBuilderCollection<TBuilder, TCollection, TItem, TLifetime>(this IServiceRegistry container)
            where TBuilder : ICollectionBuilder<TCollection, TItem>
            where TCollection : IBuilderCollection<TItem>
            where TLifetime : ILifetime, new()
        {
            // register the builder
            container.Register<TBuilder>(new PerContainerLifetime());

            // register the collection
            container.Register(factory => factory.GetInstance<TBuilder>().GetCollection(), new TLifetime());
        }
    }
}
