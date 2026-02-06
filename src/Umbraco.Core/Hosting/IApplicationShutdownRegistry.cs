namespace Umbraco.Cms.Core.Hosting;

/// <summary>
/// Provides a registry for objects that need to be notified during application shutdown.
/// </summary>
/// <remarks>
/// Registered objects receive callbacks when the application is stopping, allowing them
/// to perform cleanup operations before the application terminates.
/// </remarks>
public interface IApplicationShutdownRegistry
{
    /// <summary>
    /// Registers an object to receive shutdown notifications.
    /// </summary>
    /// <param name="registeredObject">The object to register for shutdown notifications.</param>
    void RegisterObject(IRegisteredObject registeredObject);

    /// <summary>
    /// Unregisters an object from receiving shutdown notifications.
    /// </summary>
    /// <param name="registeredObject">The object to unregister.</param>
    void UnregisterObject(IRegisteredObject registeredObject);
}
