using System.Configuration;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.PublishedCache
{
    /// <summary>
    /// Rebuilds the database cache if required when the serializer changes
    /// </summary>
    public class NuCacheStartupHandler : INotificationHandler<UmbracoApplicationStartingNotification>
    {
        // TODO: Eventually we should kill this since at some stage we shouldn't even support JSON since we know
        // this is faster.

        internal const string Nucache_Serializer_Key = "Umbraco.Web.PublishedCache.NuCache.Serializer";
        private const string JSON_SERIALIZER_VALUE = "JSON";
        private readonly IPublishedSnapshotService _service;
        private readonly IKeyValueService _keyValueService;
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<NuCacheStartupHandler> _logger;

        public NuCacheStartupHandler(
            IPublishedSnapshotService service,
            IKeyValueService keyValueService,
            IProfilingLogger profilingLogger,
            ILogger<NuCacheStartupHandler> logger)
        {
            _service = service;
            _keyValueService = keyValueService;
            _profilingLogger = profilingLogger;
            _logger = logger;
        }

        public void Handle(UmbracoApplicationStartingNotification notification)
            => RebuildDatabaseCacheIfSerializerChanged();

        private void RebuildDatabaseCacheIfSerializerChanged()
        {
            var serializer = ConfigurationManager.AppSettings[Nucache_Serializer_Key];
            var currentSerializer = _keyValueService.GetValue(Nucache_Serializer_Key);

            if (currentSerializer == null)
            {
                currentSerializer = JSON_SERIALIZER_VALUE;
            }
            if (serializer == null)
            {
                serializer = JSON_SERIALIZER_VALUE;
            }

            if (serializer != currentSerializer)
            {
                _logger.LogWarning("Database NuCache was serialized using {CurrentSerializer}. Currently configured NuCache serializer {Serializer}. Rebuilding Nucache", currentSerializer, serializer);

                using (_profilingLogger.TraceDuration<NuCacheStartupHandler>($"Rebuilding NuCache database with {currentSerializer} serializer"))
                {
                    _service.Rebuild();
                    _keyValueService.SetValue(Nucache_Serializer_Key, serializer);
                }
            }
        }
    }
}
