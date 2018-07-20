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
    }
}
