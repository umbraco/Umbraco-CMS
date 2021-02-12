using System.IO;
using CSharpTest.Net.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Serializes/Deserializes data to BTree data source for <see cref="ContentData"/>
    /// </summary>
    internal class ContentDataSerializer : ISerializer<ContentData>
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
            return new ContentData
            {
                Published = PrimitiveSerializer.Boolean.ReadFrom(stream),
                Name = PrimitiveSerializer.String.ReadFrom(stream),
                UrlSegment = PrimitiveSerializer.String.ReadFrom(stream),
                VersionId = PrimitiveSerializer.Int32.ReadFrom(stream),
                VersionDate = PrimitiveSerializer.DateTime.ReadFrom(stream),
                WriterId = PrimitiveSerializer.Int32.ReadFrom(stream),
                TemplateId = PrimitiveSerializer.Int32.ReadFrom(stream),
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
            if (value.TemplateId.HasValue)
            {
                PrimitiveSerializer.Int32.WriteTo(value.TemplateId.Value, stream);
            }
            _dictionaryOfPropertyDataSerializer.WriteTo(value.Properties, stream);
            DefaultCultureVariationsSerializer.WriteTo(value.CultureInfos, stream);
        }
    }
}
