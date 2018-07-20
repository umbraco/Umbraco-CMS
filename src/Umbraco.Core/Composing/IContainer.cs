using System;
using System.Collections.Generic;
using System.Reflection;

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
        object GetInstance(Type type, object[] args);

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

        // fixme
        IEnumerable<Registration> GetRegistered(Type serviceType);

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
        void RegisterAuto(Type serviceBaseType);

        /// <summary>
        /// Registers a service with an ordered set of implementation types.
        /// </summary>
        void RegisterOrdered(Type serviceType, Type[] implementingTypes, Lifetime lifetime = Lifetime.Transient);

        /// <summary>
        /// Registers and instanciates a collection builder.
        /// </summary>
        /// <typeparam name="T">The type of the collection builder.</typeparam>
        /// <returns>A collection builder of the specified type.</returns>
        T RegisterCollectionBuilder<T>();

        // fixme - very LightInject specific? or?
        void RegisterConstructorDependency<TDependency>(Func<IContainer, ParameterInfo, TDependency> factory);
        void RegisterConstructorDependency<TDependency>(Func<IContainer, ParameterInfo, object[], TDependency> factory);

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

        IContainer ConfigureForUmbraco();

        IContainer ConfigureForWeb();

        IContainer EnablePerWebRequestScope();

        #endregion
    }
}
