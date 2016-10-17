using System.Threading;
using System.Web;
using StackExchange.Profiling;
using Umbraco.Core;

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
    internal class StartupWebProfilerProvider : WebRequestProfilerProvider
    {
        public StartupWebProfilerProvider()
        {
            _startupPhase = StartupPhase.Boot;
            //create the startup profiler
            _startupProfiler = new MiniProfiler("http://localhost/umbraco-startup", ProfileLevel.Verbose)
            {
                Name = "StartupProfiler"
            };            
        }

        private MiniProfiler _startupProfiler;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        /// <summary>
        /// Used to determine which phase the boot process is in
        /// </summary>
        private enum StartupPhase
        {
            None = 0,
            Boot = 1,
            Request = 2
        }

        private volatile StartupPhase _startupPhase;

        /// <summary>
        /// Executed once the application boot process is complete and changes the phase to Request
        /// </summary>
        public void BootComplete()
        {
            using (new ReadLock(_locker))
            {
                if (_startupPhase != StartupPhase.Boot) return;
            }

            using (var l = new UpgradeableReadLock(_locker))
            {
                if (_startupPhase == StartupPhase.Boot)
                {
                    l.UpgradeToWriteLock();
                    _startupPhase = StartupPhase.Request;
                }
            }
        }        

        /// <summary>
        /// Executed when a profiling operation is completed
        /// </summary>
        /// <param name="discardResults"></param>
        /// <remarks>
        /// This checks if the bootup phase is None, if so, it just calls the base class, otherwise it checks
        /// if a profiler is active (i.e. in startup), then sets the phase to None so that the base class will be used
        /// for all subsequent calls.
        /// </remarks>
        public override void Stop(bool discardResults)
        {
            using (new ReadLock(_locker))
            {
                if (_startupPhase == StartupPhase.None)
                {
                    base.Stop(discardResults);
                    return;
                }
            }

            using (var l = new UpgradeableReadLock(_locker))
            {
                if (_startupPhase > 0 && base.GetCurrentProfiler() == null)
                {
                    l.UpgradeToWriteLock();

                    _startupPhase = StartupPhase.None;                    

                    //This is required to pass the mini profiling context from before a request
                    // to the current startup request.
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.Items[":mini-profiler:"] = _startupProfiler;
                        base.Stop(discardResults);
                        _startupProfiler = null;
                    }
                }
                else
                {
                    base.Stop(discardResults);
                }
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
            using (new ReadLock(_locker))
            {
                if (_startupPhase > 0 && base.GetCurrentProfiler() == null)
                {
                    SetProfilerActive(_startupProfiler);
                    return _startupProfiler;
                }

                return base.Start(level);
            }
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
            using (new ReadLock(_locker))
            {
                if (_startupPhase > 0)
                {
                    try
                    {
                        var current = base.GetCurrentProfiler();
                        if (current == null) return _startupProfiler;
                    }
                    catch
                    {
                        return _startupProfiler;
                    }
                }

                return base.GetCurrentProfiler();
            }
        }
    }
}