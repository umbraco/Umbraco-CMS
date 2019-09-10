using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Logging;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Used to notify the <see cref="IPublishedSnapshotService"/> of changes using a background thread
    /// </summary>
    /// <remarks>
    /// When in Pure Live mode, the models need to be rebuilt before the IPublishedSnapshotService is notified which can result in performance penalties so
    /// this performs these actions on a background thread so the user isn't waiting for the rebuilding to occur on a UI thread.
    /// When using this, even when not in Pure Live mode, it still means that the cache is notified on a background thread.
    /// In order to wait for the processing to complete, this class has a Wait() method which will block until the processing is finished.
    /// </remarks>
    public sealed class BackgroundPublishedSnapshotNotifier
    {
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly BackgroundTaskRunner<IBackgroundTask> _runner;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="publishedModelFactory"></param>
        /// <param name="publishedSnapshotService"></param>
        /// <param name="logger"></param>
        public BackgroundPublishedSnapshotNotifier(IPublishedModelFactory publishedModelFactory, IPublishedSnapshotService publishedSnapshotService, ILogger logger)
        {
            _publishedModelFactory = publishedModelFactory;
            _publishedSnapshotService = publishedSnapshotService;

            // TODO: We have the option to check if we are in live mode and only run on a background thread if that is the case, else run normally?
            // IMO I think we should just always run on a background thread, then no matter what state the app is in, it's always doing a consistent operation.

            _runner = new BackgroundTaskRunner<IBackgroundTask>("RebuildModelsAndCache", logger);
        }

        /// <summary>
        /// Blocks until the background operation is completed
        /// </summary>
        /// <returns>Returns true if waiting was necessary</returns>
        public bool Wait()
        {
            var running = _runner.IsRunning;
            _runner.StoppedAwaitable.GetAwaiter().GetResult(); //TODO: do we need a try/catch?
            return running;
        }

        /// <summary>
        /// Notify the <see cref="IPublishedSnapshotService"/> of content type changes
        /// </summary>
        /// <param name="payloads"></param>
        public void NotifyWithSafeLiveFactory(ContentTypeCacheRefresher.JsonPayload[] payloads)
        {
            _runner.TryAdd(new RebuildModelsAndCacheTask(payloads, _publishedModelFactory, _publishedSnapshotService));
        }

        /// <summary>
        /// Notify the <see cref="IPublishedSnapshotService"/> of data type changes
        /// </summary>
        /// <param name="payloads"></param>
        public void NotifyWithSafeLiveFactory(DataTypeCacheRefresher.JsonPayload[] payloads)
        {
            _runner.TryAdd(new RebuildModelsAndCacheTask(payloads, _publishedModelFactory, _publishedSnapshotService));
        }

        /// <summary>
        /// A simple background task that notifies the <see cref="IPublishedSnapshotService"/> of changes
        /// </summary>
        private class RebuildModelsAndCacheTask : IBackgroundTask
        {
            private readonly DataTypeCacheRefresher.JsonPayload[] _dataTypePayloads;
            private readonly ContentTypeCacheRefresher.JsonPayload[] _contentTypePayloads;
            private readonly IPublishedModelFactory _publishedModelFactory;
            private readonly IPublishedSnapshotService _publishedSnapshotService;

            private RebuildModelsAndCacheTask(IPublishedModelFactory publishedModelFactory, IPublishedSnapshotService publishedSnapshotService)
            {
                _publishedModelFactory = publishedModelFactory;
                _publishedSnapshotService = publishedSnapshotService;
            }

            public RebuildModelsAndCacheTask(DataTypeCacheRefresher.JsonPayload[] payloads, IPublishedModelFactory publishedModelFactory, IPublishedSnapshotService publishedSnapshotService)
                : this(publishedModelFactory, publishedSnapshotService)
            {
                _dataTypePayloads = payloads;
            }

            public RebuildModelsAndCacheTask(ContentTypeCacheRefresher.JsonPayload[] payloads, IPublishedModelFactory publishedModelFactory, IPublishedSnapshotService publishedSnapshotService)
                : this(publishedModelFactory, publishedSnapshotService)
            {
                _contentTypePayloads = payloads;
            }

            public void Run()
            {
                // we have to refresh models before we notify the published snapshot
                // service of changes, else factories may try to rebuild models while
                // we are using the database to load content into caches

                _publishedModelFactory.WithSafeLiveFactory(() =>
                {
                    if (_dataTypePayloads != null)
                        _publishedSnapshotService.Notify(_dataTypePayloads);
                    if (_contentTypePayloads != null)
                        _publishedSnapshotService.Notify(_contentTypePayloads);

                    //Thread.Sleep(10000);

                    var asdf = "";
                });
            }

            public Task RunAsync(CancellationToken token) => throw new System.NotImplementedException();

            public bool IsAsync => false;

            public void Dispose()
            {
            }
        }
    }
}
