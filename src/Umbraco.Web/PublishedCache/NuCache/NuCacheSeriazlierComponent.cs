using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache.NuCache
{
    /// <summary>
    /// Rebuilds the database cache if required when the serializer changes
    /// </summary>
    [RuntimeLevel(MinLevel = Core.RuntimeLevel.Run)]
    public class NuCacheSerializerComponent : IComponent
    {
        private const string Nucache_Serializer_Key = "Umbraco.Web.PublishedCache.NuCache.Serializer";
        private const string JSON_SERIALIZER_VALUE = "JSON";
        private readonly IPublishedSnapshotService _service;
        private readonly IKeyValueService _keyValueService;

        public NuCacheSerializerComponent(IPublishedSnapshotService service, IKeyValueService keyValueService)
        {
            // service: nothing - this just ensures that the service is created at boot time
            _service = service;
            _keyValueService = keyValueService;
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
                _service.Rebuild();
                _keyValueService.SetValue(Nucache_Serializer_Key, serializer);
            }
        }

        public void Terminate()
        { }
    }
}
