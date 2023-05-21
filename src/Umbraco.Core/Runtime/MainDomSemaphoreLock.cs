using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Runtime;

/// <summary>
///     Uses a system-wide Semaphore and EventWaitHandle to synchronize the current AppDomain
/// </summary>
public class MainDomSemaphoreLock : IMainDomLock
{
    private readonly ILogger<MainDomSemaphoreLock> _logger;

    // event wait handle used to notify current main domain that it should
    // release the lock because a new domain wants to be the main domain
    private readonly EventWaitHandle _signal;
    private readonly SystemLock _systemLock;
    private IDisposable? _lockRelease;

    public MainDomSemaphoreLock(ILogger<MainDomSemaphoreLock> logger, IHostingEnvironment hostingEnvironment)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("MainDomSemaphoreLock is only supported on Windows.");
        }

        var mainDomId = MainDom.GetMainDomId(hostingEnvironment);
        var lockName = "UMBRACO-" + mainDomId + "-MAINDOM-LCK";
        _systemLock = new SystemLock(lockName);

        var eventName = "UMBRACO-" + mainDomId + "-MAINDOM-EVT";
        _signal = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);
        _logger = logger;
    }

    // WaitOneAsync (ext method) will wait for a signal without blocking the main thread, the waiting is done on a background thread
    public Task ListenAsync() => _signal.WaitOneAsync();

    public Task<bool> AcquireLockAsync(int millisecondsTimeout)
    {
        // signal other instances that we want the lock, then wait on the lock,
        // which may timeout, and this is accepted - see comments below

        // signal, then wait for the lock, then make sure the event is
        // reset (maybe there was noone listening..)
        _signal.Set();

        // if more than 1 instance reach that point, one will get the lock
        // and the other one will timeout, which is accepted

        // This can throw a TimeoutException - in which case should this be in a try/finally to ensure the signal is always reset.
        try
        {
            _lockRelease = _systemLock.Lock(millisecondsTimeout);
            return Task.FromResult(true);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex.Message);
            return Task.FromResult(false);
        }
        finally
        {
            // we need to reset the event, because otherwise we would end up
            // signaling ourselves and committing suicide immediately.
            // only 1 instance can reach that point, but other instances may
            // have started and be trying to get the lock - they will timeout,
            // which is accepted
            _signal.Reset();
        }
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _lockRelease?.Dispose();
                _signal.Close();
                _signal.Dispose();
            }

            disposedValue = true;
        }
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose() =>

        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);

    #endregion
}
