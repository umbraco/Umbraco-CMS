using System;
using System.Collections.Generic;

namespace Umbraco.Core.Composing
{
    // Implementing IContainer:
    //
    // The factory
    // - always picks the constructor with the most parameters
    // - supports Lazy parameters (and prefers them over non-Lazy) in constructors
    // - what happens with 'releasing' is unclear
    //
    // The registry
    // - supports registering a service, even after some instances of other services have been created
    // - supports re-registering a service, as long as no instance of that service has been created
    // - throws when re-registering a service, and an instance of that service has been created
    //
    // - registers only one implementation of a nameless service, re-registering replaces the previous
    //   registration - names are required to register multiple implementations - and getting an
    //   IEnumerable of the service, nameless, returns them all

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

        /// <summary>
        /// Gets an instance of a service.
        /// </summary>
        /// <param name="type">The type of the service.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>Throws an exception if the container failed to get an instance of the specified type.</remarks>
        object GetInstance(Type type);

        /// <summary>
        /// Tries to get an instance of a service.
        /// </summary>
        /// <param name="type">The type of the service.</param>
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
        /// Gets registrations for a service.
        /// </summary>
        /// <param name="serviceType">The type of the service.</param>
        /// <returns>The registrations for the service.</returns>
        IEnumerable<Registration> GetRegistered(Type serviceType);

        /// <summary>
        /// Releases an instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <remarks>
        /// See https://stackoverflow.com/questions/14072208 and http://kozmic.net/2010/08/27/must-i-release-everything-when-using-windsor/,
        /// you only need to release instances you specifically resolved, and even then, if done right, that might never be needed. For
        /// instance, LightInject does not require this and does not support it - should work with scopes.
        /// </remarks> 
        void Release(object instance);

        #endregion

        #region Registry

        /// <summary>
        /// Registers a service as its own implementation.
        /// </summary>
        void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers a service with an implementation type.
        /// </summary>
        void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient);

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
