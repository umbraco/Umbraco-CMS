using System;
using System.Threading;
using System.Web;
using StackExchange.Profiling;

namespace Umbraco.Web.Profiling
{
    /// <summary>
    /// This is a custom MiniProfiler WebRequestProfilerProvider (which is generally the default) that allows
    /// us to profile items during app startup - before an HttpRequest is created
    /// </summary>
    /// <remarks>
    /// Once the boot phase is changed to StartupPhase.Request then the base class (default) provider will handle all
    /// profiling data and this sub class no longer performs any logic.
    /// </remarks>
    internal class WebProfilerProvider : WebRequestProfilerProvider
    {
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private MiniProfiler _startupProfiler;
        private int _first;
        private volatile BootPhase _bootPhase;

        public WebProfilerProvider()
        {
            // booting...
            _bootPhase = BootPhase.Boot;
        }

        /// <summary>
        /// Indicates the boot phase.
        /// </summary>
        private enum BootPhase
        {
            Boot = 0, // boot phase (before the 1st request even begins)
            BootRequest = 1, // request boot phase (during the 1st request)
            Booted = 2 // done booting
        }

        public void BeginBootRequest()
        {
            _locker.EnterWriteLock();
            try
            {
                if (_bootPhase != BootPhase.Boot)
                    throw new InvalidOperationException("Invalid boot phase.");
                _bootPhase = BootPhase.BootRequest;

                // assign the profiler to be the current MiniProfiler for the request
                // is's already active, starting and all
                HttpContext.Current.Items[":mini-profiler:"] = _startupProfiler;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void EndBootRequest()
        {
            _locker.EnterWriteLock();
            try
            {
                if (_bootPhase != BootPhase.BootRequest)
                    throw new InvalidOperationException("Invalid boot phase.");
                _bootPhase = BootPhase.Booted;

                _startupProfiler = null;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Executed when a profiling operation is started
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        /// <remarks>
        /// This checks if the startup phase is not None, if this is the case and the current profiler is NULL
        /// then this sets the startup profiler to be active. Otherwise it just calls the base class Start method.
        /// </remarks>
        public override MiniProfiler Start(ProfileLevel level)
        {
            var first = Interlocked.Exchange(ref _first, 1) == 0;
            if (first == false) return base.Start(level);

            _startupProfiler = new MiniProfiler("http://localhost/umbraco-startup") { Name = "StartupProfiler" };
            SetProfilerActive(_startupProfiler);
            return _startupProfiler;
        }

        /// <summary>
        /// This returns the current profiler
        /// </summary>
        /// <returns></returns>
        /// <remarks> 
        /// If the boot phase is not None, then this will return the startup profiler (this), otherwise
        /// returns the base class
        /// </remarks>
        public override MiniProfiler GetCurrentProfiler()
        {
            // if not booting then just use base (fast)
            // no lock, _bootPhase is volatile
            if (_bootPhase == BootPhase.Booted)
                return base.GetCurrentProfiler();

            // else
            try
            {
                var current = base.GetCurrentProfiler();
                return current ?? _startupProfiler;
            }
            catch
            {
                return _startupProfiler;
            }
        }
    }
}