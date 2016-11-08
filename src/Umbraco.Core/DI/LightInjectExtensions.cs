using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightInject;

namespace Umbraco.Core.DI
{
    internal static class LightInjectExtensions
    {
        /// <summary>
        /// Configure the container for Umbraco Core usage and assign to Current.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <remarks>The container is now the unique application container and is now accessible via Current.Container.</remarks>
        public static void ConfigureUmbracoCore(this ServiceContainer container)
        {
            // supports annotated constructor injections
            // eg to specify the service name on some services
            container.EnableAnnotatedConstructorInjection();

            // from the docs: "LightInject considers all read/write properties a dependency, but implements
            // a loose strategy around property dependencies, meaning that it will NOT throw an exception
            // in the case of an unresolved property dependency."
            //
            // in Umbraco we do NOT want to do property injection by default, so we have to disable it.
            // from the docs, the following line will cause the container to "now only try to inject
            // dependencies for properties that is annotated with the InjectAttribute."
            //
            // could not find it documented, but tests & code review shows that LightInject considers a
            // property to be "injectable" when its setter exists and is not static, nor private, nor
            // it is an index property. which means that eg protected or internal setters are OK.
            container.EnableAnnotatedPropertyInjection();

            // ensure that we do *not* scan assemblies
            // we explicitely RegisterFrom our own composition roots and don't want them scanned
            container.AssemblyScanner = new AssemblyScanner(container.AssemblyScanner);

            // see notes in MixedScopeManagerProvider
            container.ScopeManagerProvider = new MixedScopeManagerProvider();

            // self-register
            container.Register<IServiceContainer>(_ => container);

            // configure the current container
            Current.Container = container;
        }

        private class AssemblyScanner : IAssemblyScanner
        {
            private readonly IAssemblyScanner _scanner;

            public AssemblyScanner(IAssemblyScanner scanner)
            {
                _scanner = scanner;
            }

            public void Scan(Assembly assembly, IServiceRegistry serviceRegistry, Func<ILifetime> lifetime, Func<Type, Type, bool> shouldRegister)
            {
                // nothing - we *could* scan non-Umbraco assemblies, though
            }

            public void Scan(Assembly assembly, IServiceRegistry serviceRegistry)
            {
                // nothing - we *could* scan non-Umbraco assemblies, though
            }
        }

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

        // FIXME or just use names?!

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
        public static void RegisterCollection<TLifetime>(this IServiceContainer container, IEnumerable<Type> implementationTypes)
            where TLifetime : ILifetime, new()
        {
            foreach (var type in implementationTypes)
                container.Register(type, new TLifetime());
        }

        public static void RegisterCollection<TLifetime>(this IServiceContainer container, Func<IServiceFactory, IEnumerable<Type>> implementationTypes)
            where TLifetime : ILifetime, new()
        {
            foreach (var type in implementationTypes(container))
                container.Register(type, new TLifetime());
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
        public static void RegisterCollection(this IServiceContainer container, IEnumerable<Type> implementationTypes)
        {
            foreach (var type in implementationTypes)
            {
                container.Register(type);
            }
        }

        public static void RegisterCollection(this IServiceContainer container, Func<IServiceFactory, IEnumerable<Type>> implementationTypes)
        {
            foreach (var type in implementationTypes(container))
                container.Register(type);
        }

        /// <summary>
        /// Registers a base type for auto-registration.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="container">The container.</param>
        /// <remarks>
        /// <para>Any type that inherits/implements the base type will be auto-registered on-demand.</para>
        /// <para>This methods works with actual types. Use the other overload for eg generic definitions.</para>
        /// </remarks>
        public static void RegisterAuto<T>(this IServiceContainer container)
        {
            container.RegisterFallback((serviceType, serviceName) =>
            {
                // https://github.com/seesharper/LightInject/issues/173
                if (typeof(T).IsAssignableFrom(serviceType))
                    container.Register(serviceType);
                return false;
            }, null);
        }

        /// <summary>
        /// Registers a base type for auto-registration.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The base type.</param>
        /// <remarks>
        /// <para>Any type that inherits/implements the base type will be auto-registered on-demand.</para>
        /// <para>This methods works with actual types, as well as generic definitions eg <c>typeof(MyBase{})</c>.</para>
        /// </remarks>
        public static void RegisterAuto(this IServiceContainer container, Type type)
        {
            container.RegisterFallback((serviceType, serviceName) =>
            {
                //Current.Logger.Debug(typeof(LightInjectExtensions), $"Fallback for type {serviceType.FullName}.");
                // https://github.com/seesharper/LightInject/issues/173
                if (type.IsAssignableFromGtd(serviceType))
                    container.Register(serviceType);
                return false;
            }, null);
        }

        public static TBuilder RegisterCollectionBuilder<TBuilder>(this IServiceContainer container)
        {
            // make sure it's not already registered
            // we just don't want to support re-registering collection builders
            var registration = container.GetAvailableService<TBuilder>();
            if (registration != null)
                throw new InvalidOperationException("Collection builders should be registered only once.");

            // register the builder - per container
            var builderLifetime = new PerContainerLifetime();
            container.Register<TBuilder>(builderLifetime);

            // return the builder
            // (also initializes the builder)
            return container.GetInstance<TBuilder>();
        }
    }
}
