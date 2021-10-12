using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Runtime
{

    /// <summary>
    /// Provides the full implementation of <see cref="IMainDom"/>.
    /// </summary>
    /// <remarks>
    /// <para>When an AppDomain starts, it tries to acquire the main domain status.</para>
    /// <para>When an AppDomain stops (eg the application is restarting) it should release the main domain status.</para>
    /// </remarks>
    public class MainDom : IMainDom, IRegisteredObject, IDisposable
    {
        #region Vars

        private readonly ILogger _logger;
        private readonly IMainDomLock _mainDomLock;

        // our own lock for local consistency
        private object _locko = new object();

        private bool _isInitialized;
        // indicates whether...
        private bool _isMainDom; // we are the main domain
        private volatile bool _signaled; // we have been signaled

        // actions to run before releasing the main domain
        private readonly List<KeyValuePair<int, Action>> _callbacks = new List<KeyValuePair<int, Action>>();

        private const int LockTimeoutMilliseconds = 40000; // 40 seconds

        private Task _listenTask;
        private Task _listenCompleteTask;

        #endregion

        #region Ctor

        // initializes a new instance of MainDom
        public MainDom(ILogger logger, IMainDomLock systemLock)
        {
            HostingEnvironment.RegisterObject(this);

            _logger = logger;
            _mainDomLock = systemLock;
        }

        #endregion

        /// <summary>
        /// Registers a resource that requires the current AppDomain to be the main domain to function.
        /// </summary>
        /// <param name="release">An action to execute before the AppDomain releases the main domain status.</param>
        /// <param name="weight">An optional weight (lower goes first).</param>
        /// <returns>A value indicating whether it was possible to register.</returns>
        public bool Register(Action release, int weight = 100)
            => Register(null, release, weight);

        /// <summary>
        /// Registers a resource that requires the current AppDomain to be the main domain to function.
        /// </summary>
        /// <param name="install">An action to execute when registering.</param>
        /// <param name="release">An action to execute before the AppDomain releases the main domain status.</param>
        /// <param name="weight">An optional weight (lower goes first).</param>
        /// <returns>A value indicating whether it was possible to register.</returns>
        /// <remarks>If registering is successful, then the <paramref name="install"/> action
        /// is guaranteed to execute before the AppDomain releases the main domain status.</remarks>
        public bool Register(Action install, Action release, int weight = 100)
        {
            lock (_locko)
            {
                if (_signaled) return false;
                if (_isMainDom == false)
                {
                    _logger.Warn<MainDom>("Register called when MainDom has not been acquired");
                    return false;
                }

                install?.Invoke();
                if (release != null)
                    _callbacks.Add(new KeyValuePair<int, Action>(weight, release));
                return true;
            }
        }

        // handles the signal requesting that the main domain is released
        private void OnSignal(string source)
        {
            // once signaled, we stop waiting, but then there is the hosting environment
            // so we have to make sure that we only enter that method once

            lock (_locko)
            {
                _logger.Debug<MainDom, string, string>("Signaled ({Signaled}) ({SignalSource})", _signaled ? "again" : "first", source);
                if (_signaled) return;
                if (_isMainDom == false) return; // probably not needed
                _signaled = true;

                try
                {
                    _logger.Info<MainDom, string>("Stopping ({SignalSource})", source);
                    foreach (var callback in _callbacks.OrderBy(x => x.Key).Select(x => x.Value))
                    {
                        try
                        {
                            callback(); // no timeout on callbacks
                        }
                        catch (Exception e)
                        {
                            _logger.Error<MainDom>(e, "Error while running callback");
                            continue;
                        }
                    }
                        
                    _logger.Debug<MainDom, string>("Stopped ({SignalSource})", source);
                }
                finally
                {
                    // in any case...
                    _isMainDom = false;
                    _mainDomLock.Dispose();
                    _logger.Info<MainDom, string>("Released ({SignalSource})", source);
                }

            }
        }

        // acquires the main domain
        private bool Acquire()
        {
            // if signaled, too late to acquire, give up
            // the handler is not installed so that would be the hosting environment
            if (_signaled)
            {
                _logger.Info<MainDom>("Cannot acquire (signaled).");
                return false;
            }

            _logger.Info<MainDom>("Acquiring.");

            // Get the lock
            var acquired = false;
            try
            {
                acquired = _mainDomLock.AcquireLockAsync(LockTimeoutMilliseconds).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Error<MainDom>(ex, "Error while acquiring");
            }

            if (!acquired)
            {
                _logger.Info<MainDom>("Cannot acquire (timeout).");

                // In previous versions we'd let a TimeoutException be thrown
                // and the appdomain would not start. We have the opportunity to allow it to
                // start without having MainDom? This would mean that it couldn't write
                // to nucache/examine and would only be ok if this was a super short lived appdomain.
                // maybe safer to just keep throwing in this case.

                throw new TimeoutException("Cannot acquire MainDom");
                // return false;
            }

            try
            {
                // Listen for the signal from another AppDomain coming online to release the lock
                _listenTask = _mainDomLock.ListenAsync();
                _listenCompleteTask = _listenTask.ContinueWith(t =>
                {
                    if (_listenTask.Exception != null)
                    {
                        _logger.Warn<MainDom>("Listening task completed with {TaskStatus}, Exception: {Exception}", _listenTask.Status, _listenTask.Exception);
                    }
                    else
                    {
                        _logger.Debug<MainDom>("Listening task completed with {TaskStatus}", _listenTask.Status);
                    }

                    OnSignal("signal");
                }, TaskScheduler.Default); // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
            }
            catch (OperationCanceledException ex)
            {
                // the waiting task could be canceled if this appdomain is naturally shutting down, we'll just swallow this exception
                _logger.Warn<MainDom>(ex, ex.Message);
            }

            _logger.Info<MainDom>("Acquired.");
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether the current domain is the main domain.
        /// </summary>
        /// <remarks>
        /// The lazy initializer call will only call the Acquire callback when it's not been initialized, else it will just return
        /// the value from _isMainDom which means when we set _isMainDom to false again after being signaled, this will return false;
        /// </remarks>
        public bool IsMainDom => LazyInitializer.EnsureInitialized(ref _isMainDom, ref _isInitialized, ref _locko, () => Acquire());

        // IRegisteredObject
        void IRegisteredObject.Stop(bool immediate)
        {
            OnSignal("environment"); // will run once

            // The web app is stopping, need to wind down
            Dispose(true);

            HostingEnvironment.UnregisterObject(this);
        }

        #region IDisposable Support

        // This code added to correctly implement the disposable pattern.

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _mainDomLock.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        public static string GetMainDomId()
        {
            // HostingEnvironment.ApplicationID is null in unit tests, making ReplaceNonAlphanumericChars fail
            var appId = HostingEnvironment.ApplicationID?.ReplaceNonAlphanumericChars(string.Empty) ?? string.Empty;

            // combining with the physical path because if running on eg IIS Express,
            // two sites could have the same appId even though they are different.
            //
            // now what could still collide is... two sites, running in two different processes
            // and having the same appId, and running on the same app physical path
            //
            // we *cannot* use the process ID here because when an AppPool restarts it is
            // a new process for the same application path

            var appPath = HostingEnvironment.ApplicationPhysicalPath?.ToLowerInvariant() ?? string.Empty;
            var hash = (appId + ":::" + appPath).GenerateHash<SHA1>();

            return hash;
        }
    }
}
