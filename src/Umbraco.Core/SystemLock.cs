using System.Runtime.ConstrainedExecution;

namespace Umbraco.Cms.Core;

// https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-6-asynclock/
//
// notes:
// - this is NOT a reader/writer lock
// - this is NOT a recursive lock
//
// using a named Semaphore here and not a Mutex because mutexes have thread
// affinity which does not work with async situations
//
// it is important that managed code properly release the Semaphore before
// going down else it will maintain the lock - however note that when the
// whole process (w3wp.exe) goes down and all handles to the Semaphore have
// been closed, the Semaphore system object is destroyed - so in any case
// an iisreset should clean up everything
//
public class SystemLock
{
    private readonly IDisposable? _releaser;
    private readonly Task<IDisposable>? _releaserTask;
    private readonly SemaphoreSlim? _semaphore;
    private readonly Semaphore? _semaphore2;

    public SystemLock()
        : this(null)
    {
    }

    public SystemLock(string? name)
    {
        // WaitOne() waits until count > 0 then decrements count
        // Release() increments count
        // initial count: the initial count value
        // maximum count: the max value of count, and then Release() throws
        if (string.IsNullOrWhiteSpace(name))
        {
            // anonymous semaphore
            // use one unique releaser, that will not release the semaphore when finalized
            // because the semaphore is destroyed anyway if the app goes down
            _semaphore = new SemaphoreSlim(1, 1); // create a local (to the app domain) semaphore
            _releaser = new SemaphoreSlimReleaser(_semaphore);
            _releaserTask = Task.FromResult(_releaser);
        }
        else
        {
            // named semaphore
            // use dedicated releasers, that will release the semaphore when finalized
            // because the semaphore is system-wide and we cannot leak counts
            _semaphore2 = new Semaphore(1, 1, name); // create a system-wide named semaphore
        }
    }

    public IDisposable? Lock()
    {
        if (_semaphore != null)
        {
            _semaphore.Wait();
        }
        else
        {
            _semaphore2?.WaitOne();
        }

        return _releaser ?? CreateReleaser(); // anonymous vs named
    }

    private IDisposable? CreateReleaser() =>

        // for anonymous semaphore, use the unique releaser, else create a new one
        _semaphore != null
            ? _releaser // (IDisposable)new SemaphoreSlimReleaser(_semaphore)
            : new NamedSemaphoreReleaser(_semaphore2);

    public IDisposable? Lock(int millisecondsTimeout)
    {
        var entered = _semaphore != null
            ? _semaphore.Wait(millisecondsTimeout)
            : _semaphore2?.WaitOne(millisecondsTimeout);
        if (entered == false)
        {
            throw new TimeoutException("Failed to enter the lock within timeout.");
        }

        return _releaser ?? CreateReleaser(); // anonymous vs named
    }

    // note - before making those classes some structs, read
    // about "impure methods" and mutating readonly structs...
    private class NamedSemaphoreReleaser : CriticalFinalizerObject, IDisposable
    {
        private readonly Semaphore? _semaphore;

        // This code added to correctly implement the disposable pattern.
        private bool _disposedValue; // To detect redundant calls

        internal NamedSemaphoreReleaser(Semaphore? semaphore) => _semaphore = semaphore;

        // we WANT to release the semaphore because it's a system object, ie a critical
        // non-managed resource - and if it is not released then noone else can acquire
        // the lock - so we inherit from CriticalFinalizerObject which means that the
        // finalizer "should" run in all situations - there is always a chance that it
        // does not run and the semaphore remains "acquired" but then chances are the
        // whole process (w3wp.exe...) is going down, at which point the semaphore will
        // be destroyed by Windows.

        // however, the semaphore is a managed object, and so when the finalizer runs it
        // might have been finalized already, and then we get a, ObjectDisposedException
        // in the finalizer - which is bad.

        // in order to prevent this we do two things
        // - use a GCHandler to ensure the semaphore is still there when the finalizer
        //   runs, so we can actually release it
        // - wrap the finalizer code in a try...catch to make sure it never throws
        ~NamedSemaphoreReleaser()
        {
            try
            {
                Dispose(false);
            }
            catch
            {
                // we do NOT want the finalizer to throw - never ever
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // finalize will not run
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                try
                {
                    _semaphore?.Release();
                }
                finally
                {
                    try
                    {
                        _semaphore?.Dispose();
                    }
                    catch
                    {
                    }
                }

                _disposedValue = true;
            }
        }
    }

    private class SemaphoreSlimReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        internal SemaphoreSlimReleaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

        ~SemaphoreSlimReleaser() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // normal
                _semaphore.Release();
            }
        }
    }
}
