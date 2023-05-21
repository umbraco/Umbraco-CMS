namespace Umbraco.Cms.Core.Runtime;

/// <summary>
///     An application-wide distributed lock
/// </summary>
/// <remarks>
///     Disposing releases the lock
/// </remarks>
public interface IMainDomLock : IDisposable
{
    /// <summary>
    ///     Acquires an application-wide distributed lock
    /// </summary>
    /// <param name="millisecondsTimeout"></param>
    /// <returns>
    ///     An awaitable boolean value which will be false if the elapsed millsecondsTimeout value is exceeded
    /// </returns>
    Task<bool> AcquireLockAsync(int millisecondsTimeout);

    /// <summary>
    ///     Wait on a background thread to receive a signal from another AppDomain
    /// </summary>
    /// <returns></returns>
    Task ListenAsync();
}
