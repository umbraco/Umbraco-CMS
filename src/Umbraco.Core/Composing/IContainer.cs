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
        /// Gets an instance with arguments.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <param name="args">Arguments.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>
        /// <para>Throws an exception if the container failed to get an instance of the specified type.</para>
        /// <para>The arguments are used as dependencies by the container.</para>
        /// </remarks>
        // fixme - some restrictions:
        //  method is not optimized, .Invoke-ing the ctor, no compiled dynamic method
        //  uses the ctor with most args, always, not trying to figure out which one to use
        object GetInstance(Type type, params object[] args);

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
        /// Registers a service with an implementation factory.
        /// </summary>
        void Register<TService>(Func<IContainer, TService> factory, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers a service with an implementation factory accepting an argument.
        /// </summary>
        void Register<T, TService>(Func<IContainer, T, TService> factory);

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

        // fixme - document all these

        /// <summary>
        /// Configures the container for Umbraco.
        /// </summary>
        /// <remarks>
        /// <para></para>
        /// </remarks>
        IContainer ConfigureForUmbraco();

        IContainer ConfigureForWeb();

        IContainer EnablePerWebRequestScope();

        #endregion
    }
}
