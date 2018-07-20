using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core
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
        /// Gets an instance with arguments.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="args">Arguments.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>
        /// <para>Throws an exception if the container failed to get an instance of the specified type.</para>
        /// <para>The arguments are used as dependencies by the container.</para>
        /// </remarks>
        public static T GetInstance<T>(this IContainer container, object[] args)
            => (T) container.GetInstance(typeof(T), args);

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

        // fixme - document all these

        public static IEnumerable<Registration> GetRegistered<TService>(this IContainer container)
            => container.GetRegistered(typeof(TService));

        public static void Register<TService, TImplementing>(this IContainer container, Lifetime lifetime = Lifetime.Transient)
            => container.Register(typeof(TService), typeof(TImplementing), lifetime);

        public static void Register<TService, TImplementing>(this IContainer container, string name, Lifetime lifetime = Lifetime.Transient)
            => container.Register(typeof(TService), typeof(TImplementing), name, lifetime);

        public static void Register<TService>(this IContainer container, Lifetime lifetime = Lifetime.Transient)
            => container.Register(typeof(TService), lifetime);

        public static void RegisterSingleton<TService>(this IContainer container)
            => container.Register(typeof(TService), Lifetime.Singleton);

        public static void RegisterSingleton<TService, TImplementing>(this IContainer container)
            => container.Register(typeof(TService), typeof(TImplementing), Lifetime.Singleton);

        public static void RegisterSingleton<TService>(this IContainer container, Func<IContainer, TService> factory)
            => container.Register(factory, Lifetime.Singleton);

        public static void RegisterInstance<TService>(this IContainer container, TService instance)
            => container.RegisterInstance(typeof(TService), instance);

        public static void RegisterAuto<TServiceBase>(this IContainer container)
            => container.RegisterAuto(typeof(TServiceBase));
    }
}
