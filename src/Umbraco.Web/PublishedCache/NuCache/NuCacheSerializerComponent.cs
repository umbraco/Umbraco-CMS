using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache.NuCache
{
    /// <summary>
    /// Rebuilds the database cache if required when the serializer changes
    /// </summary>
    public class NuCacheSerializerComponent : IComponent
    {
        internal const string Nucache_Serializer_Key = "Umbraco.Web.PublishedCache.NuCache.Serializer";
        internal const string Nucache_UnPublishedContentCompression_Key = "Umbraco.Web.PublishedCache.NuCache.CompressUnPublishedContent";
        private const string JSON_SERIALIZER_VALUE = "JSON";
        private readonly Lazy<IPublishedSnapshotService> _service;
        private readonly IKeyValueService _keyValueService;
        private readonly IProfilingLogger _profilingLogger;

        public NuCacheSerializerComponent(Lazy<IPublishedSnapshotService> service, IKeyValueService keyValueService, IProfilingLogger profilingLogger)
        {
            // We are using lazy here as a work around because the service does quite a lot of initialization in the ctor which
            // we want to avoid where possible. Since we only need the service if we are rebuilding, we don't want to eagerly
            // initialize anything unless we need to.
            _service = service;
            _keyValueService = keyValueService;
            _profilingLogger = profilingLogger;
        }

        public void Initialize()
        {
            RebuildDatabaseCacheIfSerializerChanged();
        }

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
                _profilingLogger.Warn<NuCacheSerializerComponent>($"Database NuCache was serialized using {currentSerializer}. Currently configured NuCache serializer {serializer}. Rebuilding Nucache");

                using (_profilingLogger.TraceDuration<NuCacheSerializerComponent>($"Rebuilding NuCache database with {serializer} serializer"))
                {
                    _service.Value.Rebuild();
                    _keyValueService.SetValue(Nucache_Serializer_Key, serializer);
                }
            }
        }

        public void Terminate()
        { }
    }
}
