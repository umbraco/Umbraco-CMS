namespace Umbraco.Cms.Core;

/// <summary>
///     Defines an object that can be registered with the application and notified when the application shuts down.
/// </summary>
public interface IRegisteredObject
{
    /// <summary>
    ///     Requests the object to stop processing.
    /// </summary>
    /// <param name="immediate">
    ///     <c>true</c> to indicate the registered object should stop immediately;
    ///     <c>false</c> to indicate the registered object should complete any pending work before stopping.
    /// </param>
    void Stop(bool immediate);
}
