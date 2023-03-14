namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

[Flags]
public enum ContentCacheDataSerializerEntityType
{
    Document = 1,
    Media = 2,
    Member = 4,
}
