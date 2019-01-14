using System;
using System.Collections.Generic;

namespace Umbraco.Core.Composing
{
    // Implementing:
    //
    // The factory
    // - always picks the constructor with the most parameters
    // - supports Lazy parameters (and prefers them over non-Lazy) in constructors
    // - what happens with 'releasing' is unclear

    /// <summary>
    /// Defines a service factory for Umbraco.
    /// </summary>
    public interface IFactory : IServiceProvider
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
        IEnumerable<TService> GetAllInstances<TService>();

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

        /// <summary>
        /// Begins a scope.
        /// </summary>
        /// <remarks>
        /// <para>When the scope is disposed, scoped instances that have been created during the scope are disposed.</para>
        /// <para>Scopes can be nested. Each instance is disposed individually.</para>
        /// </remarks>
        IDisposable BeginScope();

        /// <summary>
        /// Enables per-request scope.
        /// </summary>
        /// <remarks>
        /// <para>Ties scopes to web requests.</para>
        /// </remarks>
        void EnablePerWebRequestScope();
    }
}
