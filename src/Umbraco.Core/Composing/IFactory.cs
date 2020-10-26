using System;
using System.Collections.Generic;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Defines a service factory for Umbraco.
    /// </summary>
    public interface IFactory
    {
        /// <summary>
        /// Gets the concrete factory.
        /// </summary>
        object Concrete { get; }

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
        IEnumerable<TService> GetAllInstances<TService>()
            where TService : class;
        
        /// <summary>
        /// Begins a scope.
        /// </summary>
        /// <remarks>
        /// <para>When the scope is disposed, scoped instances that have been created during the scope are disposed.</para>
        /// <para>Scopes can be nested. Each instance is disposed individually.</para>
        /// </remarks>
        IDisposable BeginScope();
    }
}
