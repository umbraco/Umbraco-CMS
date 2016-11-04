using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Hosting;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the main AppDomain running for a given application.
    /// </summary>
    /// <remarks>
    /// <para>There can be only one "main" AppDomain running for a given application at a time.</para>
    /// <para>When an AppDomain starts, it tries to acquire the main domain status.</para>
    /// <para>When an AppDomain stops (eg the application is restarting) it should release the main domain status.</para>
    /// <para>It is possible to register against the MainDom and be notified when it is released.</para>
    /// </remarks>
    internal class MainDom : IRegisteredObject
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
        private readonly SortedList<int, Action> _callbacks = new SortedList<int, Action>(new WeightComparer());

        private const int LockTimeoutMilliseconds = 90000; // (1.5 * 60 * 1000) == 1 min 30 seconds

        private class WeightComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                var result = x.CompareTo(y);
                // return "equal" as "greater than"
                return result == 0 ? 1 : result;
            }
        }

        #endregion

        #region Ctor

        // initializes a new instance of MainDom
        internal MainDom(ILogger logger)
        {
            _logger = logger;

            var appId = string.Empty;
            // HostingEnvironment.ApplicationID is null in unit tests, making ReplaceNonAlphanumericChars fail
            if (HostingEnvironment.ApplicationID != null)
                appId = HostingEnvironment.ApplicationID.ReplaceNonAlphanumericChars(string.Empty);

            // combining with the physical path because if running on eg IIS Express,
            // two sites could have the same appId even though they are different.
            //
            // now what could still collide is... two sites, running in two different processes
            // and having the same appId, and running on the same app physical path
            //
            // we *cannot* use the process ID here because when an AppPool restarts it is
            // a new process for the same application path

            var appPath = HostingEnvironment.ApplicationPhysicalPath;
            var hash = (appId + ":::" + appPath).ToSHA1();

            var lockName = "UMBRACO-" + hash + "-MAINDOM-LCK";
            _asyncLock = new AsyncLock(lockName);

            var eventName = "UMBRACO-" + hash + "-MAINDOM-EVT";
            _signal = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);
        }

        #endregion

        /// <summary>
        /// Registers a resource that requires the current AppDomain to be the main domain to function.
        /// </summary>
        /// <param name="release">An action to execute before the AppDomain releases the main domain status.</param>
        /// <param name="weight">An optional weight (lower goes first).</param>
        /// <returns>A value indicating whether it was possible to register.</returns>
        public bool Register(Action release, int weight = 100)
        {
            return Register(null, release, weight);
        }

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
                _logger.Info<MainDom>("Stopping...");
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
                _logger.Info<MainDom>("Released MainDom.");
            }
        }

        // acquires the main domain
        internal bool Acquire()
        {
            lock (_locko) // we don't want the hosting environment to interfere by signaling
            {
                // if signaled, too late to acquire, give up
                // the handler is not installed so that would be the hosting environment
                if (_signaled)
                {
                    _logger.Info<MainDom>("Cannot acquire MainDom (signaled).");
                    return false;
                }

                _logger.Info<MainDom>("Acquiring MainDom...");

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

                _logger.Info<MainDom>("Acquired MainDom.");
                return true;
            }
        }

        // gets a value indicating whether we are the main domain
        public bool IsMainDom
        {
            get { return _isMainDom; }
        }

        // IRegisteredObject
        void IRegisteredObject.Stop(bool immediate)
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
