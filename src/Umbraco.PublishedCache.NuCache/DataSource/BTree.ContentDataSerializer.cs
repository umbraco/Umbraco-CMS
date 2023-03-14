using CSharpTest.Net.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

/// <summary>
///     Serializes/Deserializes data to BTree data source for <see cref="ContentData" />
/// </summary>
public class ContentDataSerializer : ISerializer<ContentData>
{
    private static readonly DictionaryOfPropertyDataSerializer S_defaultPropertiesSerializer = new();
    private static readonly DictionaryOfCultureVariationSerializer S_defaultCultureVariationsSerializer = new();
    private readonly IDictionaryOfPropertyDataSerializer? _dictionaryOfPropertyDataSerializer;

    public ContentDataSerializer(IDictionaryOfPropertyDataSerializer? dictionaryOfPropertyDataSerializer = null)
    {
        _dictionaryOfPropertyDataSerializer = dictionaryOfPropertyDataSerializer;
        if (_dictionaryOfPropertyDataSerializer == null)
        {
            _dictionaryOfPropertyDataSerializer = S_defaultPropertiesSerializer;
        }
    }

    public ContentData ReadFrom(Stream stream)
    {
        var published = PrimitiveSerializer.Boolean.ReadFrom(stream);
        var name = PrimitiveSerializer.String.ReadFrom(stream);
        var urlSegment = PrimitiveSerializer.String.ReadFrom(stream);
        var versionId = PrimitiveSerializer.Int32.ReadFrom(stream);
        DateTime versionDate = PrimitiveSerializer.DateTime.ReadFrom(stream);
        var writerId = PrimitiveSerializer.Int32.ReadFrom(stream);
        var templateId = PrimitiveSerializer.Int32.ReadFrom(stream);
        IDictionary<string, PropertyData[]>?
            properties =
                _dictionaryOfPropertyDataSerializer?.ReadFrom(stream); // TODO: We don't want to allocate empty arrays
        IReadOnlyDictionary<string, CultureVariation> cultureInfos =
            S_defaultCultureVariationsSerializer.ReadFrom(stream); // TODO: We don't want to allocate empty arrays
        var cachedTemplateId = templateId == 0 ? (int?)null : templateId;
        return new ContentData(name, urlSegment, versionId, versionDate, writerId, cachedTemplateId, published, properties, cultureInfos);
    }

    public void WriteTo(ContentData value, Stream stream)
    {
        PrimitiveSerializer.Boolean.WriteTo(value.Published, stream);
        PrimitiveSerializer.String.WriteTo(value.Name, stream);
        PrimitiveSerializer.String.WriteTo(value.UrlSegment, stream);
        PrimitiveSerializer.Int32.WriteTo(value.VersionId, stream);
        PrimitiveSerializer.DateTime.WriteTo(value.VersionDate, stream);
        PrimitiveSerializer.Int32.WriteTo(value.WriterId, stream);
        PrimitiveSerializer.Int32.WriteTo(value.TemplateId ?? 0, stream);
        _dictionaryOfPropertyDataSerializer?.WriteTo(value.Properties, stream);
        S_defaultCultureVariationsSerializer.WriteTo(value.CultureInfos, stream);
    }
}
