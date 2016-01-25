using System.Threading;
using System.Web;
using StackExchange.Profiling;

namespace Umbraco.Core.Profiling
{
    /// <summary>
    /// Allows us to profile items during app startup - before an HttpRequest is created
    /// </summary>
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

        private enum StartupPhase
        {
            None = 0,
            Boot = 1,
            Request = 2
        }

        private volatile StartupPhase _startupPhase;

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

                    ////Now we need to transfer some information from our startup phase to the normal
                    ////web request phase to output the startup profiled information.
                    ////Stop our internal startup profiler, this will write out it's results to storage.
                    //StopProfiler(_startupProfiler);
                    //SaveProfiler(_startupProfiler);

                    _startupPhase = StartupPhase.Request;
                }
            }
        }        

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