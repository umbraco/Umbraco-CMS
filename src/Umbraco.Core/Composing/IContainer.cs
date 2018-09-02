using System;
using System.Collections.Generic;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Defines a container for Umbraco.
    /// </summary>
    public interface IContainer : IDisposable
    {
        /// <summary>
        /// Gets the concrete container.
        /// </summary>
        object ConcreteContainer { get; }

        #region Factory

        // notes
        // when implementing IContainer, the following rules apply
        // - always pick the constructor with the most parameters
        // - always prefer Lazy parameters over non-Lazy in constructors

        /// <summary>
        /// Gets an instance.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>Throws an exception if the container failed to get an instance of the specified type.</remarks>
        object GetInstance(Type type);

        /// <summary>
        /// Gets a named instance.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <param name="name">The name of the instance.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>Throws an exception if the container failed to get an instance of the specified type.</remarks>
        object GetInstance(Type type, string name);

        /// <summary>
        /// Tries to get an instance.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <returns>An instance of the specified type, or null.</returns>
        /// <remarks>Returns null if the container does not know how to get an instance
        /// of the specified type. Throws an exception if the container does know how
        /// to get an instance of the specified type, but failed to do so.</remarks>
        object TryGetInstance(Type type);

        /// <summary>
        /// Gets all instances of a service.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        IEnumerable<object> GetAllInstances(Type serviceType);

        /// <summary>
        /// Gets all instances of a service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        IEnumerable<TService> GetAllInstances<TService>();

        /// <summary>
        /// Gets registration for a service.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>The registrations for the service.</returns>
        IEnumerable<Registration> GetRegistered(Type serviceType);

        /// <summary>
        /// Creates an instance with arguments.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <param name="args">Named arguments.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>
        /// <para>The instance type does not need to be registered into the container.</para>
        /// <para>The arguments are used as dependencies by the container. Other dependencies
        /// are retrieved from the container.</para>
        /// </remarks>
        object CreateInstance(Type type, IDictionary<string, object> args);

        #endregion

        #region Registry

        // notes
        // when implementing IContainer, the following rules apply
        // - it is possible to register a service, even after some instances of other services have been created
        // - it is possible to re-register a service, as long as an instance of that service has not been created
        // - once an instance of a service has been created, it is not possible to change its registration

        /// <summary>
        /// Registers a service as its own implementation.
        /// </summary>
        void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers a service with an implementation type.
        /// </summary>
        void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers a service with a named implementation type.
        /// </summary>
        void Register(Type serviceType, Type implementingType, string name, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers a named service with an implementation factory.
        /// </summary>
        void Register<TService>(Func<IContainer, TService> factory, string name, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers a service with an implementation factory.
        /// </summary>
        void Register<TService>(Func<IContainer, TService> factory, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers a service with an implementing instance.
        /// </summary>
        void RegisterInstance(Type serviceType, object instance);

        /// <summary>
        /// Registers a base type for auto-registration.
        /// </summary>
        /// <remarks>
        /// <para>Auto-registration means that anytime the container is asked to create an instance
        /// of a type deriving from <paramref name="serviceBaseType"/>, it will first register that
        /// type automatically.</para>
        /// <para>This can be used for instance for views or controllers. Then, one just needs to
        /// register a common base class or interface, and the container knows how to create instances.</para>
        /// </remarks>
        void RegisterAuto(Type serviceBaseType);

        /// <summary>
        /// Registers a service with an ordered set of implementation types.
        /// </summary>
        void RegisterOrdered(Type serviceType, Type[] implementingTypes, Lifetime lifetime = Lifetime.Transient);

        #endregion

        #region Control

        /// <summary>
        /// Begins a scope.
        /// </summary>
        /// <remarks>
        /// <para>When the scope is disposed, scoped instances that have been created during the scope are disposed.</para>
        /// <para>Scopes can be nested. Each instance is disposed individually.</para>
        /// </remarks>
        IDisposable BeginScope();

        /// <summary>
        /// Configures the container for web support.
        /// </summary>
        /// <returns>The container.</returns>
        /// <remarks>
        /// <para>Enables support for MVC, WebAPI, but *not* per-request scope. This is used early in the boot
        /// process, where anything "scoped" should not be linked to a web request.</para>
        /// </remarks>
        IContainer ConfigureForWeb();

        /// <summary>
        /// Enables per-request scope.
        /// </summary>
        /// <returns>The container.</returns>
        /// <remarks>
        /// <para>Ties scopes to web requests.</para>
        /// </remarks>
        IContainer EnablePerWebRequestScope();

        #endregion
    }
}
