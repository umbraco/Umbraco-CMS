namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

internal class JsonContentNestedDataSerializerFactory : IContentCacheDataSerializerFactory
{
    private readonly Lazy<JsonContentNestedDataSerializer> _serializer = new();

    public IContentCacheDataSerializer Create(ContentCacheDataSerializerEntityType types) => _serializer.Value;
}
