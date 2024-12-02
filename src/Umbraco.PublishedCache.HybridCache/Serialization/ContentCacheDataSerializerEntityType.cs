namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

[Flags]
public enum ContentCacheDataSerializerEntityType
{
    Document = 1,
    Media = 2,
    Member = 4,
}
