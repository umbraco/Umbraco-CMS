using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core
{
    // represents the main domain
    class MainDom : IRegisteredObject
    {
        #region Vars

        private readonly ILogger _logger;

        // our own lock for local consistency
        private readonly object _locko = new object();

        // async lock representing the main domain lock
        private readonly AsyncLock _asyncLock;
        private IDisposable _asyncLocker;

        // event wait handle used to notify current main domain that it should 
        // release the lock because a new domain wants to be the main domain
        private readonly EventWaitHandle _signal;

        // indicates whether...
        private volatile bool _isMainDom; // we are the main domain
        private volatile bool _signaled; // we have been signaled

        // actions to run before releasing the main domain
        private readonly SortedList<int, Action> _callbacks = new SortedList<int, Action>();

        private const int LockTimeoutMilliseconds = 90000; // (1.5 * 60 * 1000) == 1 min 30 seconds

        #endregion

        #region Ctor

        // initializes a new instance of MainDom
        public MainDom(ILogger logger)
        {
            _logger = logger;

            var appId = string.Empty;
            // HostingEnvironment.ApplicationID is null in unit tests, making ReplaceNonAlphanumericChars fail
            if (HostingEnvironment.ApplicationID != null)
                appId = HostingEnvironment.ApplicationID.ReplaceNonAlphanumericChars(string.Empty);

            var lockName = "UMBRACO-" + appId + "-MAINDOM-LCK";
            _asyncLock = new AsyncLock(lockName);

            var eventName = "UMBRACO-" + appId + "-MAINDOM-EVT";
            _signal = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);
        }

        #endregion

        // register a main domain consumer
        public bool Register(Action release, int weight = 100)
        {
            return Register(null, release, weight);
        }

        // register a main domain consumer
        public bool Register(Action install, Action release, int weight = 100)
        {
            lock (_locko)
            {
                if (_signaled) return false;
                if (install != null)
                    install();
                if (release != null)
                    _callbacks.Add(weight, release);
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
                _logger.Debug<MainDom>("Signaled" + (_signaled ? " (again)" : "") + " (" + source + ").");
                if (_signaled) return;
                if (_isMainDom == false) return; // probably not needed
                _signaled = true;
            }

            try
            {
                _logger.Debug<MainDom>("Stopping...");
                foreach (var callback in _callbacks.Values)
                {
                    try
                    {
                        callback(); // no timeout on callbacks
                    }
                    catch (Exception e)
                    {
                        _logger.Error<MainDom>("Error while running callback, remaining callbacks will not run.", e);
                        throw;
                    }
                    
                }
                _logger.Debug<MainDom>("Stopped.");
            }
            finally
            {
                // in any case...
                _isMainDom = false;
                _asyncLocker.Dispose();
                _logger.Debug<MainDom>("Released MainDom.");
            }
        }

        // acquires the main domain
        public bool Acquire()
        {
            lock (_locko) // we don't want the hosting environment to interfere by signaling
            {
                // if signaled, too late to acquire, give up
                // the handler is not installed so that would be the hosting environment
                if (_signaled)
                {
                    _logger.Debug<MainDom>("Cannot acquire MainDom (signaled).");
                    return false;
                }

                _logger.Debug<MainDom>("Acquiring MainDom...");

                // signal other instances that we want the lock, then wait one the lock,
                // which may timeout, and this is accepted - see comments below

                // signal, then wait for the lock, then make sure the event is
                // resetted (maybe there was noone listening..)
                _signal.Set();

                // if more than 1 instance reach that point, one will get the lock
                // and the other one will timeout, which is accepted

                _asyncLocker = _asyncLock.Lock(LockTimeoutMilliseconds);
                _isMainDom = true;

                // we need to reset the event, because otherwise we would end up
                // signaling ourselves and commiting suicide immediately.
                // only 1 instance can reach that point, but other instances may
                // have started and be trying to get the lock - they will timeout,
                // which is accepted

                _signal.Reset();
                _signal.WaitOneAsync()
                    .ContinueWith(_ => OnSignal("signal"));

                HostingEnvironment.RegisterObject(this);

                _logger.Debug<MainDom>("Acquired MainDom.");
                return true;
            }
        }

        // gets a value indicating whether we are the main domain
        public bool IsMainDom
        {
            get { return _isMainDom; }
        }

        // IRegisteredObject
        public void Stop(bool immediate)
        {
            try
            {
                OnSignal("environment"); // will run once
            }
            finally
            {
                HostingEnvironment.UnregisterObject(this);
            }
        }
    }
}
