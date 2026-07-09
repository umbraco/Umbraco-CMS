namespace Umbraco.Cms.Core.Events;

/// <summary>
/// Extensions for <see cref="ServiceFactory" />.
/// </summary>
public static class ServiceFactoryExtensions
{
    /// <summary>
    /// Gets an instance of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <param name="factory">The service factory.</param>
    /// <returns>
    /// The new instance.
    /// </returns>
    public static T GetInstance<T>(this ServiceFactory factory)
        => (T)factory(typeof(T));

    /// <summary>
    /// Gets a collection of instances of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The collection item type to return.</typeparam>
    /// <param name="factory">The service factory.</param>
    /// <returns>
    /// The new instance collection.
    /// </returns>
    public static IEnumerable<T> GetInstances<T>(this ServiceFactory factory)
        => (IEnumerable<T>)factory(typeof(IEnumerable<T>));
}
