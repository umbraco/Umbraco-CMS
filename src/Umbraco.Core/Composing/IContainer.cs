using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Composing
{
    // fixme - must document!

    /// <summary>
    /// Defines a container for Umbraco.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Gets an instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>Throws an exception if the container failed to get an instance of the specified type.</remarks>
        T GetInstance<T>();

        /// <summary>
        /// Gets an instance.
        /// </summary>
        /// <param name="type">The type of the instance.</param>
        /// <returns>An instance of the specified type.</returns>
        /// <remarks>Throws an exception if the container failed to get an instance of the specified type.</remarks>
        object GetInstance(Type type);

        /// <summary>
        /// Tries to get an instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <returns>An instance of the specified type, or null.</returns>
        /// <remarks>Returns null if the container does not know how to get an instance
        /// of the specified type. Throws an exception if the container does know how
        /// to get an instance of the specified type, but failed to do so.</remarks>
        T TryGetInstance<T>();

        // fixme document
        T GetInstance<T>(object[] args);

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

        // fixme move away!
        object ConcreteContainer { get; }
    }

    // fixme would be nicer
    //public interface IContainer<T> : IContainer
    //{
    //    T ConcreteContainer { get; }
    //}
}
