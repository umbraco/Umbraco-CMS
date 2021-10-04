using System.IO;
using CSharpTest.Net.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource
{
    /// <summary>
    /// Serializes/Deserializes data to BTree data source for <see cref="ContentData"/>
    /// </summary>
    public class ContentDataSerializer : ISerializer<ContentData>
    {
        public ContentDataSerializer(IDictionaryOfPropertyDataSerializer dictionaryOfPropertyDataSerializer = null)
        {
            _dictionaryOfPropertyDataSerializer = dictionaryOfPropertyDataSerializer;
            if(_dictionaryOfPropertyDataSerializer == null)
            {
                _dictionaryOfPropertyDataSerializer = DefaultPropertiesSerializer;
            }
        }
        private static readonly DictionaryOfPropertyDataSerializer DefaultPropertiesSerializer = new DictionaryOfPropertyDataSerializer();
        private static readonly DictionaryOfCultureVariationSerializer DefaultCultureVariationsSerializer = new DictionaryOfCultureVariationSerializer();
        private readonly IDictionaryOfPropertyDataSerializer _dictionaryOfPropertyDataSerializer;

        public ContentData ReadFrom(Stream stream)
        {
            var published = PrimitiveSerializer.Boolean.ReadFrom(stream);
            var name = PrimitiveSerializer.String.ReadFrom(stream);
            var urlSegment = PrimitiveSerializer.String.ReadFrom(stream);
            var versionId = PrimitiveSerializer.Int32.ReadFrom(stream);
            var versionDate = PrimitiveSerializer.DateTime.ReadFrom(stream);
            var writerId = PrimitiveSerializer.Int32.ReadFrom(stream);
            var templateId = PrimitiveSerializer.Int32.ReadFrom(stream);
            return new ContentData
            {
                Published = published,
                Name = name,
                UrlSegment = urlSegment,
                VersionId = versionId,
                VersionDate = versionDate,
                WriterId = writerId,
                TemplateId = templateId == 0 ? (int?)null : templateId,
                Properties = _dictionaryOfPropertyDataSerializer.ReadFrom(stream), // TODO: We don't want to allocate empty arrays
                CultureInfos = DefaultCultureVariationsSerializer.ReadFrom(stream) // TODO: We don't want to allocate empty arrays
            };
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
            _dictionaryOfPropertyDataSerializer.WriteTo(value.Properties, stream);
            DefaultCultureVariationsSerializer.WriteTo(value.CultureInfos, stream);
        }
    }
}
