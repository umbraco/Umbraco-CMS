using System;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class JsonContentNestedDataSerializerFactory : IContentCacheDataSerializerFactory
    {
        private Lazy<JsonContentNestedDataSerializer> _serializer = new Lazy<JsonContentNestedDataSerializer>();
        public IContentCacheDataSerializer Create(ContentCacheDataSerializerEntityType types) => _serializer.Value;
    }
}
