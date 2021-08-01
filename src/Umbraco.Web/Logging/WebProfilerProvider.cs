using System;
using System.Threading;
using System.Web;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;

namespace Umbraco.Web.Logging
{
    /// <summary>
    /// This is a custom MiniProfiler WebRequestProfilerProvider (which is generally the default) that allows
    /// us to profile items during app startup - before an HttpRequest is created
    /// </summary>
    /// <remarks>
    /// Once the boot phase is changed to BootPhase.BootRequest then the base class (default) provider will handle all
    /// profiling data and this sub class no longer performs any logic.
    /// </remarks>
    internal class WebProfilerProvider : AspNetRequestProvider
    {
        private readonly object _locker = new object();
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
            lock (_locker)
            {
                if (_bootPhase != BootPhase.Boot)
                    throw new InvalidOperationException("Invalid boot phase.");
                _bootPhase = BootPhase.BootRequest;

                // assign the profiler to be the current MiniProfiler for the request
                // is's already active, starting and all
                HttpContext.Current.Items[":mini-profiler:"] = _startupProfiler; 
            }
        }

        public void EndBootRequest()
        {
            lock (_locker)
            {
                if (_bootPhase != BootPhase.BootRequest)
                    throw new InvalidOperationException("Invalid boot phase.");
                _bootPhase = BootPhase.Booted;

                _startupProfiler = null; 
            }
        }

        /// <summary>
        /// Starts a new MiniProfiler.
        /// </summary>
        /// <remarks>
        /// <para>This is called when WebProfiler calls MiniProfiler.Start() so,
        /// - as a result of WebRuntime starting the WebProfiler, and
        /// - assuming profiling is enabled, on every BeginRequest that should be profiled,
        /// - except for the very first one which is the boot request.</para>
        /// </remarks>
        public override MiniProfiler Start(string profilerName, MiniProfilerBaseOptions options)
        {
            var first = Interlocked.Exchange(ref _first, 1) == 0;
            if (first == false)
            {
                var profiler = base.Start(profilerName, options);
                return profiler;
            }

            _startupProfiler = new MiniProfiler("StartupProfiler", options);
            CurrentProfiler = _startupProfiler;
            return _startupProfiler;
        }

        /// <summary>
        /// Gets the current profiler.
        /// </summary>
        /// <remarks>
        /// If the boot phase is not Booted, then this will return the startup profiler (this), otherwise
        /// returns the base class
        /// </remarks>
        public override MiniProfiler CurrentProfiler
        {
            get
            {
                // if not booting then just use base (fast)
                // no lock, _bootPhase is volatile
                if (_bootPhase == BootPhase.Booted)
                    return base.CurrentProfiler;

                // else
                try
                {
                    var current = base.CurrentProfiler;
                    return current ?? _startupProfiler;
                }
                catch
                {
                    return _startupProfiler;
                }
            }
            
        }
    }
}
