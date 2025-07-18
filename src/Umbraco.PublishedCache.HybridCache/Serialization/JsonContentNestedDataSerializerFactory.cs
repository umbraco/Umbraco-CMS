namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

internal sealed class JsonContentNestedDataSerializerFactory : IContentCacheDataSerializerFactory
{
    private readonly Lazy<JsonContentNestedDataSerializer> _serializer = new();

    public IContentCacheDataSerializer Create(ContentCacheDataSerializerEntityType types) => _serializer.Value;
}
