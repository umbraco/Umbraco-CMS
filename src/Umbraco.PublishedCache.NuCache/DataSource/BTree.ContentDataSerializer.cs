using System.IO;
using CSharpTest.Net.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource
{
    /// <summary>
    /// Serializes/Deserializes data to BTree data source for <see cref="ContentData"/>
    /// </summary>
    public class ContentDataSerializer : ISerializer<ContentData>
    {
        public ContentDataSerializer(IDictionaryOfPropertyDataSerializer? dictionaryOfPropertyDataSerializer = null)
        {
            _dictionaryOfPropertyDataSerializer = dictionaryOfPropertyDataSerializer;
            if(_dictionaryOfPropertyDataSerializer == null)
            {
                _dictionaryOfPropertyDataSerializer = s_defaultPropertiesSerializer;
            }
        }
        private static readonly DictionaryOfPropertyDataSerializer s_defaultPropertiesSerializer = new DictionaryOfPropertyDataSerializer();
        private static readonly DictionaryOfCultureVariationSerializer s_defaultCultureVariationsSerializer = new DictionaryOfCultureVariationSerializer();
        private readonly IDictionaryOfPropertyDataSerializer? _dictionaryOfPropertyDataSerializer;

        public ContentData ReadFrom(Stream stream)
        {
            var published = PrimitiveSerializer.Boolean.ReadFrom(stream);
            var name = ArrayPoolingLimitedSerializer.StringSerializer.ReadString(stream);
            var urlSegment = ArrayPoolingLimitedSerializer.StringSerializer.ReadString(stream);
            var versionId = PrimitiveSerializer.Int32.ReadFrom(stream);
            var versionDate = PrimitiveSerializer.DateTime.ReadFrom(stream);
            var writerId = PrimitiveSerializer.Int32.ReadFrom(stream);
            var templateId = PrimitiveSerializer.Int32.ReadFrom(stream);
            var properties = _dictionaryOfPropertyDataSerializer?.ReadFrom(stream); // TODO: We don't want to allocate empty arrays
            var cultureInfos = s_defaultCultureVariationsSerializer.ReadFrom(stream); // TODO: We don't want to allocate empty arrays
            return new ContentData(name, urlSegment, versionId, versionDate, writerId, templateId, published, properties, cultureInfos);
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
            s_defaultCultureVariationsSerializer.WriteTo(value.CultureInfos, stream);
        }
    }
}
