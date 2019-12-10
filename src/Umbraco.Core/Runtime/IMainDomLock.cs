using System;
using System.Threading.Tasks;

namespace Umbraco.Core.Runtime
{
    /// <summary>
    /// An application-wide distributed lock
    /// </summary>
    /// <remarks>
    /// Disposing releases the lock
    /// </remarks>
    public interface IMainDomLock : IDisposable
    {
        /// <summary>
        /// Acquires an application-wide distributed lock
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns>
        /// A disposable object which will be disposed in order to release the lock
        /// </returns>
        /// <exception cref="TimeoutException">Throws a <see cref="TimeoutException"/> if the elapsed millsecondsTimeout value is exceeded</exception>
        Task<bool> AcquireLockAsync(int millisecondsTimeout);

        /// <summary>
        /// Wait on a background thread to receive a signal from another AppDomain
        /// </summary>
        /// <returns></returns>
        Task ListenAsync();
    }
}
