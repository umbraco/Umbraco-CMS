using System;
using System.Threading;
using System.Threading.Tasks;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    /// <summary>
    /// This is the background task runner that persists the xml file to the file system
    /// </summary>
    /// <remarks>
    /// This is used so that all file saving is done on a web aware worker background thread and all logic is performed async so this
    /// process will not interfere with any web requests threads. This is also done as to not require any global locks and to ensure that
    /// if multiple threads are performing publishing tasks that the file will be persisted in accordance with the final resulting
    /// xml structure since the file writes are queued.
    /// </remarks>
    internal class XmlCacheFilePersister : LatchedBackgroundTaskBase
    {
        private readonly IBackgroundTaskRunner<XmlCacheFilePersister> _runner;
        private readonly content _content;
        private readonly ProfilingLogger _logger;
        private readonly object _locko = new object();
        private bool _released;
        private Timer _timer;
        private DateTime _initialTouch;
        private readonly AsyncLock _runLock = new AsyncLock(); // ensure we run once at a time

        // note:
        // as long as the runner controls the runs, we know that we run once at a time, but
        // when the AppDomain goes down and the runner has completed and yet the persister is
        // asked to save, then we need to run immediately - but the runner may be running, so
        // we need to make sure there's no collision - hence _runLock

        private const int WaitMilliseconds = 4000; // save the cache 4s after the last change (ie every 4s min)
        private const int MaxWaitMilliseconds = 30000; // save the cache after some time (ie no more than 30s of changes)

        // save the cache when the app goes down
        public override bool RunsOnShutdown { get { return _timer != null; } }

        // initialize the first instance, which is inactive (not touched yet)
        public XmlCacheFilePersister(IBackgroundTaskRunner<XmlCacheFilePersister> runner, content content, ProfilingLogger logger)
            : this(runner, content, logger, false)
        { }

        private XmlCacheFilePersister(IBackgroundTaskRunner<XmlCacheFilePersister> runner, content content, ProfilingLogger logger, bool touched)
        {
            _runner = runner;
            _content = content;
            _logger = logger;

            if (runner.TryAdd(this) == false)
            {
                _runner = null; // runner's down
                _released = true; // don't mess with timer
                return;
            }

            // runner could decide to run it anytime now

            if (touched == false) return;

            _logger.Logger.Debug<XmlCacheFilePersister>("Created, save in {0}ms.", () => WaitMilliseconds);
            _initialTouch = DateTime.Now;
            _timer = new Timer(_ => TimerRelease());
            _timer.Change(WaitMilliseconds, 0);
        }

        public XmlCacheFilePersister Touch()
        {
            // if _released is false then we're going to setup a timer
            //  then the runner wants to shutdown & run immediately
            //  this sets _released to true & the timer will trigger eventualy & who cares?
            // if _released is true, either it's a normal release, or
            //  a runner shutdown, in which case we won't be able to
            //  add a new task, and so we'll run immediately

            var ret = this;
            var runNow = false;

            lock (_locko)
            {
                if (_released) // our timer has triggered OR the runner is shutting down
                {
                    _logger.Logger.Debug<XmlCacheFilePersister>("Touched, was released...");

                    // release: has run or is running, too late, return a new task (adds itself to runner)
                    if (_runner == null)
                    {
                        _logger.Logger.Debug<XmlCacheFilePersister>("Runner is down, run now.");
                        runNow = true;
                    }
                    else
                    {
                        _logger.Logger.Debug<XmlCacheFilePersister>("Create new...");
                        ret = new XmlCacheFilePersister(_runner, _content, _logger, true);
                        if (ret._runner == null)
                        {
                            // could not enlist with the runner, runner is completed, must run now
                            _logger.Logger.Debug<XmlCacheFilePersister>("Runner is down, run now.");
                            runNow = true;
                        }
                    }
                }

                else if (_timer == null) // we don't have a timer yet
                {
                    _logger.Logger.Debug<XmlCacheFilePersister>("Touched, was idle, start and save in {0}ms.", () => WaitMilliseconds);
                    _initialTouch = DateTime.Now;
                    _timer = new Timer(_ => TimerRelease());
                    _timer.Change(WaitMilliseconds, 0);
                }

                else // we have a timer
                {
                    // change the timer to trigger in WaitMilliseconds unless we've been touched first more
                    // than MaxWaitMilliseconds ago and then leave the time unchanged

                    if (DateTime.Now - _initialTouch < TimeSpan.FromMilliseconds(MaxWaitMilliseconds))
                    {
                        _logger.Logger.Debug<XmlCacheFilePersister>("Touched, was waiting, can delay, save in {0}ms.", () => WaitMilliseconds);
                        _timer.Change(WaitMilliseconds, 0);
                    }
                    else
                    {
                        _logger.Logger.Debug<XmlCacheFilePersister>("Touched, was waiting, cannot delay.");
                    }
                }
            }

            if (runNow)
                //Run();
                LogHelper.Warn<XmlCacheFilePersister>("Cannot write now because we are going down, changes may be lost.");

            return ret; // this, by default, unless we created a new one
        }

        private void TimerRelease()
        {
            lock (_locko)
            {
                _logger.Logger.Debug<XmlCacheFilePersister>("Timer: release.");
                _released = true;

                Release();
            }
        }

        public override Task RunAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override bool IsAsync
        {
            get { return false; }
        }

        public override void Run()
        {
            lock (_locko)
            {
                _logger.Logger.Debug<XmlCacheFilePersister>("Run now (sync).");
                // not really needed but safer (it's only us invoking Run, but the method is public...)
                _released = true;
            }

            using (_runLock.Lock())
            {
                _content.SaveXmlToFile();
            }
        }

        protected override void DisposeResources()
        {
            base.DisposeResources();

            // stop the timer
            if (_timer == null) return;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
        }
    }
}