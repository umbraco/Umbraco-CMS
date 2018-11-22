using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightInject;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides extensions to LightInject.
    /// </summary>
    public static class LightInjectExtensions
    {
        /// <summary>
        /// Configure the container for Umbraco Core usage and assign to Current.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <remarks>The container is now the unique application container and is now accessible via Current.Container.</remarks>
        internal static void ConfigureUmbracoCore(this ServiceContainer container)
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
            container.AssemblyScanner = new AssemblyScanner(/*container.AssemblyScanner*/);

            // see notes in MixedLightInjectScopeManagerProvider
            container.ScopeManagerProvider = new MixedLightInjectScopeManagerProvider();

            // self-register
            container.Register<IServiceContainer>(_ => container);

            // configure the current container
            Current.Container = container;
        }

        private class AssemblyScanner : IAssemblyScanner
        {
            //private readonly IAssemblyScanner _scanner;

            //public AssemblyScanner(IAssemblyScanner scanner)
            //{
            //    _scanner = scanner;
            //}

            public void Scan(Assembly assembly, IServiceRegistry serviceRegistry, Func<ILifetime> lifetime, Func<Type, Type, bool> shouldRegister, Func<Type, Type, string> serviceNameProvider)
            {
                // nothing - we *could* scan non-Umbraco assemblies, though
            }

            public void Scan(Assembly assembly, IServiceRegistry serviceRegistry)
            {
                // nothing - we *could* scan non-Umbraco assemblies, though
            }
        }

        /// <summary>
        /// Registers a service implementation with a specified lifetime.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        /// <typeparam name="TLifetime">The type of the lifetime.</typeparam>
        /// <param name="container">The container.</param>
        public static void Register<TService, TImplementation, TLifetime>(this IServiceContainer container)
            where TImplementation : TService
            where TLifetime : ILifetime, new()
        {
            container.Register<TService, TImplementation>(new TLifetime());
        }

        /// <summary>
        /// Registers a service implementation with a specified lifetime.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TLifetime">The type of the lifetime.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="factory">A factory.</param>
        public static void Register<TService, TLifetime>(this IServiceContainer container, Func<IServiceFactory, TService> factory)
            where TLifetime : ILifetime, new()
        {
            container.Register(factory, new TLifetime());
        }

        /// <summary>
        /// Registers several service implementations with a specified lifetime.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TLifeTime">The type of the lifetime.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="implementations">The types of the implementations.</param>
        public static void RegisterMany<TService, TLifeTime>(this IServiceContainer container, IEnumerable<Type> implementations)
            where TLifeTime : ILifetime, new()
        {
            foreach (var implementation in implementations)
            {
                // if "typeof (TService)" is there then "implementation.FullName" MUST be there too
                container.Register(typeof(TService), implementation, implementation.FullName, new TLifeTime());
            }
        }

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
        /// Registers the TService with the TImplementation as a singleton.
        /// </summary>
        public static void RegisterSingleton<TService, TImplementation>(this IServiceRegistry container)
            where TImplementation : TService
        {
            var registration = container.GetAvailableService<TService>();

            if (registration == null)
            {
                container.Register<TService, TImplementation>(new PerContainerLifetime());
            }
            else
            {
                if (registration.Lifetime is PerContainerLifetime == false)
                    throw new InvalidOperationException("Existing registration lifetime is not PerContainer.");
                UpdateRegistration(registration, typeof(TImplementation), null);
            }
        }

        /// <summary>
        /// Registers a concrete type as a singleton service.
        /// </summary>
        public static void RegisterSingleton<TImplementation>(this IServiceRegistry container)
        {
            container.RegisterSingleton<TImplementation, TImplementation>();
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
        private static void UpdateRegistration(Registration registration, Type implementingType, Delegate factoryExpression)
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
        /// Gets the available service registrations for a service type.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The service registrations for the service type.</returns>
        public static IEnumerable<ServiceRegistration> GetAvailableServices<TService>(this IServiceRegistry container)
        {
            var typeofTService = typeof(TService);
            return container.AvailableServices.Where(x => x.ServiceType == typeofTService);
        }

        /// <summary>
        /// Gets the unique available service registration for a service type.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The unique service registration for the service type.</returns>
        /// <remarks>Can return <c>null</c>, but throws if more than one registration exist for the service type.</remarks>
        public static ServiceRegistration GetAvailableService<TService>(this IServiceRegistry container)
        {
            var typeofTService = typeof(TService);
            return container.AvailableServices.SingleOrDefault(x => x.ServiceType == typeofTService);
        }

        /// <summary>
        /// Gets the unique available service registration for a service type and a name.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="name">The name.</param>
        /// <returns>The unique service registration for the service type and the name.</returns>
        /// <remarks>Can return <c>null</c>, but throws if more than one registration exist for the service type and the name.</remarks>
        public static ServiceRegistration GetAvailableService<TService>(this IServiceRegistry container, string name)
        {
            var typeofTService = typeof(TService);
            return container.AvailableServices.SingleOrDefault(x => x.ServiceType == typeofTService && x.ServiceName == name);
        }

        /// <summary>
        /// Gets an instance of a TService or throws a meaningful exception.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="factory">The container.</param>
        /// <returns>The instance.</returns>
        public static TService GetInstanceOrThrow<TService>(this IServiceFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            try
            {
                return factory.GetInstance<TService>();
            }
            catch (Exception e)
            {
                LightInjectException.TryThrow(e);
                throw;
            }
        }

        /// <summary>
        /// Gets an instance of a TService or throws a meaningful exception.
        /// </summary>
        /// <param name="factory">The container.</param>
        /// <param name="tService">The type of the service.</param>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="implementingType">The implementing type.</param>
        /// <param name="args">Arguments.</param>
        /// <returns>The instance.</returns>
        internal static object GetInstanceOrThrow(this IServiceFactory factory, Type tService, string serviceName, Type implementingType, object[] args)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            // fixme temp - STOP doing this, it confuses LightInject and then we get ugly exception traces
            // we HAVE to let LightInject throw - and catch at THE OUTERMOST if InvalidOperationException in LightInject.Anything!

            return factory.GetInstance(tService, serviceName, args);
            //try
            //{
            //    return factory.GetInstance(tService, serviceName, args);
            //}
            //catch (Exception e)
            //{
            //    LightInjectException.TryThrow(e, implementingType);
            //    throw;
            //}
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
                //Current.Logger.Debug(typeof(LightInjectExtensions), $"Fallback for type {serviceType.FullName}.");
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

        /// <summary>
        /// Registers and instanciates a collection builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The collection builder.</returns>
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
