using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Logging;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Cache
{
    public sealed class BackgroundSafeLiveFactory
    {
        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private BackgroundTaskRunner<IBackgroundTask> _runner;

        public BackgroundSafeLiveFactory(IPublishedModelFactory publishedModelFactory, IPublishedSnapshotService publishedSnapshotService, ILogger logger)
        {
            _publishedModelFactory = publishedModelFactory;
            _publishedSnapshotService = publishedSnapshotService;
            _runner = new BackgroundTaskRunner<IBackgroundTask>("RebuildModelsAndCache", logger);
        }

        public void Execute(ContentTypeCacheRefresher.JsonPayload[] payloads)
        {
            _runner.TryAdd(new RebuildModelsAndCacheTask(payloads, _publishedModelFactory, _publishedSnapshotService));
        }

        public void Execute(DataTypeCacheRefresher.JsonPayload[] payloads)
        {
            _runner.TryAdd(new RebuildModelsAndCacheTask(payloads, _publishedModelFactory, _publishedSnapshotService));
        }

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
                });
            }

            public Task RunAsync(CancellationToken token)
            {
                throw new System.NotImplementedException();
            }

            public bool IsAsync => false;

            public void Dispose()
            {
            }
        }
    }
}
