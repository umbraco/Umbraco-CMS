using System;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Defines a container for Umbraco.
    /// </summary>
    public interface IContainer
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

        #endregion

        #region Registry

        // fixme register direct type?
        // fixme register an instance?
        void RegisterSingleton<T>(Func<IContainer, T> factory);
        void Register<T>(Func<IContainer, T> factory);
        void Register<T, TService>(Func<IContainer, T, TService> factory);

        /// <summary>
        /// Registers and instanciates a collection builder.
        /// </summary>
        /// <typeparam name="T">The type of the collection builder.</typeparam>
        /// <returns>A collection builder of the specified type.</returns>
        T RegisterCollectionBuilder<T>();

        #endregion
    }
}
