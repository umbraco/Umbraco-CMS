namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

public interface IDictionaryOfPropertyDataSerializer
{
    IDictionary<string, PropertyData[]> ReadFrom(Stream stream);

    void WriteTo(IDictionary<string, PropertyData[]> value, Stream stream);
}
