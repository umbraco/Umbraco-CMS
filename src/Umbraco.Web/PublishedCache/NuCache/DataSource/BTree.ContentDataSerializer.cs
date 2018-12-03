using System.IO;
using CSharpTest.Net.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    class ContentDataSerializer : ISerializer<ContentData>
    {
        private static readonly DictionaryOfPropertyDataSerializer PropertiesSerializer = new DictionaryOfPropertyDataSerializer();
        private static readonly DictionaryOfCultureVariationSerializer CultureVariationsSerializer = new DictionaryOfCultureVariationSerializer();

        public ContentData ReadFrom(Stream stream)
        {
            return new ContentData
            {
                Published = PrimitiveSerializer.Boolean.ReadFrom(stream),
                Name = PrimitiveSerializer.String.ReadFrom(stream),
                VersionId = PrimitiveSerializer.Int32.ReadFrom(stream),
                VersionDate = PrimitiveSerializer.DateTime.ReadFrom(stream),
                WriterId = PrimitiveSerializer.Int32.ReadFrom(stream),
                TemplateId = PrimitiveSerializer.Int32.ReadFrom(stream),
                Properties = PropertiesSerializer.ReadFrom(stream),
                CultureInfos = CultureVariationsSerializer.ReadFrom(stream)
            };
        }

        public void WriteTo(ContentData value, Stream stream)
        {
            PrimitiveSerializer.Boolean.WriteTo(value.Published, stream);
            PrimitiveSerializer.String.WriteTo(value.Name, stream);
            PrimitiveSerializer.Int32.WriteTo(value.VersionId, stream);
            PrimitiveSerializer.DateTime.WriteTo(value.VersionDate, stream);
            PrimitiveSerializer.Int32.WriteTo(value.WriterId, stream);
            PrimitiveSerializer.Int32.WriteTo(value.TemplateId, stream);
            PropertiesSerializer.WriteTo(value.Properties, stream);
            CultureVariationsSerializer.WriteTo(value.CultureInfos, stream);
        }
    }
}