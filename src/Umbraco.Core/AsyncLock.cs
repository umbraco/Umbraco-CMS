using System;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Core
{
    // http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx
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
    internal class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly Semaphore _semaphore2;
        private readonly IDisposable _releaser;
        private readonly Task<IDisposable> _releaserTask;

        public AsyncLock()
            : this (null)
        { }

        public AsyncLock(string name)
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

        private IDisposable CreateReleaser()
        {
            // for anonymous semaphore, use the unique releaser, else create a new one
            return _semaphore != null
                ? _releaser // (IDisposable)new SemaphoreSlimReleaser(_semaphore)
                : (IDisposable)new NamedSemaphoreReleaser(_semaphore2);
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = _semaphore != null 
                ? _semaphore.WaitAsync() 
                : WaitOneAsync(_semaphore2);

            return wait.IsCompleted 
                ? _releaserTask ?? Task.FromResult(CreateReleaser()) // anonymous vs named
                : wait.ContinueWith((_, state) => (((AsyncLock) state).CreateReleaser()),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public IDisposable Lock()
        {
            if (_semaphore != null)
                _semaphore.Wait();
            else
                _semaphore2.WaitOne();
            return _releaser ?? CreateReleaser(); // anonymous vs named
        }

        public IDisposable Lock(int millisecondsTimeout)
        {
            var entered = _semaphore != null
                ? _semaphore.Wait(millisecondsTimeout)
                : _semaphore2.WaitOne(millisecondsTimeout);
            if (entered == false)
                throw new TimeoutException("Failed to enter the lock within timeout.");
            return _releaser ?? CreateReleaser(); // anonymous vs named
        }

        // note - before making those classes some structs, read 
        // about "impure methods" and mutating readonly structs...

        private class NamedSemaphoreReleaser : CriticalFinalizerObject, IDisposable
        {
            private readonly Semaphore _semaphore;

            internal NamedSemaphoreReleaser(Semaphore semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                // critical
                _semaphore.Release();
            }

            // we WANT to release the semaphore because it's a system object
            // ie a critical non-managed resource - so we inherit from CriticalFinalizerObject
            // which means that the finalizer "should" run in all situations

            // however... that can fail with System.ObjectDisposedException because the
            // underlying handle was closed... because we cannot guarantee that the semaphore
            // is not gone already... unless we get a GCHandle = GCHandle.Alloc(_semaphore);
            // which should keep it around and then we free the handle?

            // so... I'm not sure this is safe really...

            ~NamedSemaphoreReleaser()
            {
                Dispose(false);
            }
        }

        private class SemaphoreSlimReleaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            internal SemaphoreSlimReleaser(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

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

            ~SemaphoreSlimReleaser()
            {
                Dispose(false);
            }
        }

        // http://stackoverflow.com/questions/25382583/waiting-on-a-named-semaphore-with-waitone100-vs-waitone0-task-delay100
        // http://blog.nerdbank.net/2011/07/c-await-for-waithandle.html
        // F# has a AwaitWaitHandle method that accepts a time out... and seems pretty complex...
        // version below should be OK

        private static Task WaitOneAsync(WaitHandle handle)
        {
            var tcs = new TaskCompletionSource<object>();
            var callbackHandleInitLock = new object();
            lock (callbackHandleInitLock)
            {
                RegisteredWaitHandle callbackHandle = null;
                // ReSharper disable once RedundantAssignment
                callbackHandle = ThreadPool.RegisterWaitForSingleObject(
                    handle,
                    (state, timedOut) =>
                    {
                        tcs.SetResult(null);

                        // we take a lock here to make sure the outer method has completed setting the local variable callbackHandle.
                        lock (callbackHandleInitLock)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            // ReSharper disable once AccessToModifiedClosure
                            callbackHandle.Unregister(null);
                        }
                    },
                    /*state:*/ null,
                    /*millisecondsTimeOutInterval:*/ Timeout.Infinite,
                    /*executeOnlyOnce:*/ true);
            }

            return tcs.Task;
        }
    }
}