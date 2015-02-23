using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
        private readonly string _xmlFileName;
        private readonly ProfilingLogger _logger;
        private readonly content _content;
        private readonly ManualResetEventSlim _latch = new ManualResetEventSlim(false);
        private readonly object _locko = new object();
        private bool _released;
        private Timer _timer;
        private DateTime _initialTouch;

        private const int WaitMilliseconds = 4000; // save the cache 4s after the last change (ie every 4s min)
        private const int MaxWaitMilliseconds = 30000; // save the cache after some time (ie no more than 30s of changes)

        // save the cache when the app goes down
        public bool RunsOnShutdown { get { return true; } }

        public XmlCacheFilePersister(IBackgroundTaskRunner<XmlCacheFilePersister> runner, content content, string xmlFileName, ProfilingLogger logger, bool touched = false)
        {
            _runner = runner;
            _content = content;
            _xmlFileName = xmlFileName;
            _logger = logger;

            if (touched == false) return;

            LogHelper.Debug<XmlCacheFilePersister>("Create new touched, start.");

            _initialTouch = DateTime.Now;
            _timer = new Timer(_ => Release());

            LogHelper.Debug<XmlCacheFilePersister>("Save in {0}ms.", () => WaitMilliseconds);
            _timer.Change(WaitMilliseconds, 0);
        }

        public XmlCacheFilePersister Touch()
        {
            lock (_locko)
            {
                if (_released)
                {
                    LogHelper.Debug<XmlCacheFilePersister>("Touched, was released, create new.");

                    // released, has run or is running, too late, add & return a new task
                    var persister = new XmlCacheFilePersister(_runner, _content, _xmlFileName, _logger, true);
                    _runner.Add(persister);
                    return persister;
                }

                if (_timer == null)
                {
                    LogHelper.Debug<XmlCacheFilePersister>("Touched, was idle, start.");

                    // not started yet, start
                    _initialTouch = DateTime.Now;
                    _timer = new Timer(_ => Release());
                    LogHelper.Debug<XmlCacheFilePersister>("Save in {0}ms.", () => WaitMilliseconds);
                    _timer.Change(WaitMilliseconds, 0);
                    return this;
                }

                // set the timer to trigger in WaitMilliseconds unless we've been touched first more
                // than MaxWaitMilliseconds ago and then release now

                if (DateTime.Now - _initialTouch < TimeSpan.FromMilliseconds(MaxWaitMilliseconds))
                {
                    LogHelper.Debug<XmlCacheFilePersister>("Touched, was waiting, wait.", () => WaitMilliseconds);
                    LogHelper.Debug<XmlCacheFilePersister>("Save in {0}ms.", () => WaitMilliseconds);
                    _timer.Change(WaitMilliseconds, 0);
                }
                else
                {
                    LogHelper.Debug<XmlCacheFilePersister>("Save now, release.");
                    ReleaseLocked();
                }

                return this; // still available
            }
        }

        private void Release()
        {
            lock (_locko)
            {
                ReleaseLocked();
            }
        }

        private void ReleaseLocked()
        {
            LogHelper.Debug<XmlCacheFilePersister>("Timer: save now, release.");
            if (_timer != null)
                _timer.Dispose();
            _timer = null;
            _released = true;
            _latch.Set();
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
            LogHelper.Debug<XmlCacheFilePersister>("Run now.");
            var doc = _content.XmlContentInternal;
            await PersistXmlToFileAsync(doc);
        }

        public bool IsAsync
        {
            get { return true; }
        }

        /// <summary>
        /// Persist a XmlDocument to the Disk Cache
        /// </summary>
        /// <param name="xmlDoc"></param>
        internal async Task PersistXmlToFileAsync(XmlDocument xmlDoc)
        {
            if (xmlDoc != null)
            {
                using (_logger.DebugDuration<XmlCacheFilePersister>(
                    string.Format("Saving content to disk on thread '{0}' (Threadpool? {1})", Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread),
                    string.Format("Saved content to disk on thread '{0}' (Threadpool? {1})", Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread)))
                {
                    try
                    {
                        // Try to create directory for cache path if it doesn't yet exist
                        var directoryName = Path.GetDirectoryName(_xmlFileName);
                        // create dir if it is not there, if it's there, this will proceed as normal
                        Directory.CreateDirectory(directoryName);

                        await xmlDoc.SaveAsync(_xmlFileName);
                    }
                    catch (Exception ee)
                    {
                        // If for whatever reason something goes wrong here, invalidate disk cache
                        DeleteXmlCache();

                        LogHelper.Error<XmlCacheFilePersister>("Error saving content to disk", ee);
                    }
                }

                
            }
        }

        private void DeleteXmlCache()
        {
            if (File.Exists(_xmlFileName) == false) return;

            // Reset file attributes, to make sure we can delete file
            try
            {
                File.SetAttributes(_xmlFileName, FileAttributes.Normal);
            }
            finally
            {
                File.Delete(_xmlFileName);
            }
        }

        public void Dispose()
        { }

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}