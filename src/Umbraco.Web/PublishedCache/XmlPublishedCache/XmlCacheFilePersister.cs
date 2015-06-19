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
    internal class XmlCacheFilePersister : ILatchedBackgroundTask
    {
        private readonly IBackgroundTaskRunner<XmlCacheFilePersister> _runner;
        private readonly content _content;
        private readonly ProfilingLogger _logger;
        private readonly ManualResetEventSlim _latch = new ManualResetEventSlim(false);
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
        public bool RunsOnShutdown { get { return true; } }

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
                if (_timer != null)
                    _timer.Dispose();
                _timer = null;
                _released = true;

                // if running (because of shutdown) this will have no effect
                // else it tells the runner it is time to run the task
                _latch.Set();
            }
        }

        public WaitHandle Latch
        {
            get { return _latch.WaitHandle; }
        }

        public bool IsLatched
        {
            get { return true; }
        }

        public async Task RunAsync(CancellationToken token)
        {
            lock (_locko)
            {
                _logger.Logger.Debug<XmlCacheFilePersister>("Run now (async).");
                // just make sure - in case the runner is running the task on shutdown
                _released = true;
            }

            // http://stackoverflow.com/questions/13489065/best-practice-to-call-configureawait-for-all-server-side-code
            // http://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
            // do we really need that ConfigureAwait here?
            
            // - In theory, no, because we are already executing on a background thread because we know it is there and
            // there won't be any SynchronizationContext to resume to, however this is 'library' code and 
            // who are we to say that this will never be executed in a sync context... this is best practice to be sure 
            // it won't cause problems.
            // .... so yes we want it.

            using (await _runLock.LockAsync())
            {
                await _content.SaveXmlToFileAsync().ConfigureAwait(false);
            }
        }

        public bool IsAsync
        {
            get { return true; }
        }

        public void Dispose()
        { }

        public void Run()
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
    }
}