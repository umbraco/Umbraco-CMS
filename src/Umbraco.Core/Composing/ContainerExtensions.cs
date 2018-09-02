using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides extension methods to the <see cref="IContainer"/> class.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Gets an instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>Throws an exception if the container failed to get an instance of the specified type.</remarks>
        public static T GetInstance<T>(this IContainer container)
            => (T) container.GetInstance(typeof(T));

        /// <summary>
        /// Gets a named instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="name">The name of the instance.</typeparam>
        /// <returns>An instance of the specified type and name.</returns>
        /// <remarks>Throws an exception if the container failed to get an instance of the specified type.</remarks>
        public static T GetInstance<T>(this IContainer container, string name)
            => (T) container.GetInstance(typeof(T), name);

        /// <summary>
        /// Tries to get an instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <returns>An instance of the specified type, or null.</returns>
        /// <remarks>Returns null if the container does not know how to get an instance
        /// of the specified type. Throws an exception if the container does know how
        /// to get an instance of the specified type, but failed to do so.</remarks>
        public static T TryGetInstance<T>(this IContainer container)
            => (T) container.TryGetInstance(typeof(T));

        /// <summary>
        /// Gets registration for a service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>The registrations for the service.</returns>
        public static IEnumerable<Registration> GetRegistered<TService>(this IContainer container)
            => container.GetRegistered(typeof(TService));

        /// <summary>
        /// Creates an instance with arguments.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="args">Arguments.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>
        /// <para>Throws an exception if the container failed to get an instance of the specified type.</para>
        /// <para>The arguments are used as dependencies by the container.</para>
        /// </remarks>
        public static T CreateInstance<T>(this IContainer container, IDictionary<string, object> args)
            => (T) container.CreateInstance(typeof(T), args);

        /// <summary>
        /// Registers a service with an implementation type.
        /// </summary>
        public static void Register<TService, TImplementing>(this IContainer container, Lifetime lifetime = Lifetime.Transient)
            => container.Register(typeof(TService), typeof(TImplementing), lifetime);

        /// <summary>
        /// Registers a service with a named implementation type.
        /// </summary>
        public static void Register<TService, TImplementing>(this IContainer container, string name, Lifetime lifetime = Lifetime.Transient)
            => container.Register(typeof(TService), typeof(TImplementing), name, lifetime);

        /// <summary>
        /// Registers a service with a named implementation type and factory.
        /// </summary>
        public static void Register<TService, TImplementing>(this IContainer container, string name, Func<IContainer, TService> factory, Lifetime lifetime = Lifetime.Transient)
            => container.Register(factory, name, lifetime);

        /// <summary>
        /// Registers a service as its own implementation.
        /// </summary>
        public static void Register<TService>(this IContainer container, Lifetime lifetime = Lifetime.Transient)
            => container.Register(typeof(TService), lifetime);

        /// <summary>
        /// Registers a singleton service as its own implementation.
        /// </summary>
        public static void RegisterSingleton<TService>(this IContainer container)
            => container.Register(typeof(TService), Lifetime.Singleton);

        /// <summary>
        /// Registers a singleton service with an implementation type.
        /// </summary>
        public static void RegisterSingleton<TService, TImplementing>(this IContainer container)
            => container.Register(typeof(TService), typeof(TImplementing), Lifetime.Singleton);

        /// <summary>
        /// Registers a singleton service with an implementation factory.
        /// </summary>
        public static void RegisterSingleton<TService>(this IContainer container, Func<IContainer, TService> factory)
            => container.Register(factory, Lifetime.Singleton);

        /// <summary>
        /// Registers a service with an implementing instance.
        /// </summary>
        public static void RegisterInstance<TService>(this IContainer container, TService instance)
            => container.RegisterInstance(typeof(TService), instance);

        /// <summary>
        /// Registers a base type for auto-registration.
        /// </summary>
        public static void RegisterAuto<TServiceBase>(this IContainer container)
            => container.RegisterAuto(typeof(TServiceBase));

        /// <summary>
        /// Registers and instanciates a collection builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
        /// <returns>A collection builder of the specified type.</returns>
        public static TBuilder RegisterCollectionBuilder<TBuilder>(this IContainer container)
        {
            // make sure it's not already registered
            // we just don't want to support re-registering collection builders
            if (container.GetRegistered<TBuilder>().Any())
                throw new InvalidOperationException("Collection builders should be registered only once.");

            // register the builder
            container.RegisterSingleton<TBuilder>();

            // initialize and return the builder
            return container.GetInstance<TBuilder>();
        }
    }
}
