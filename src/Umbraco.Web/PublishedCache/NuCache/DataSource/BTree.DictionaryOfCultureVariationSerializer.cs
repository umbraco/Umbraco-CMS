using System;
using System.Collections.Generic;
using System.IO;
using CSharpTest.Net.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// Serializes/Deserializes culture variant data as a dictionary for BTree
    /// </summary>
    internal class DictionaryOfCultureVariationSerializer : SerializerBase, ISerializer<IReadOnlyDictionary<string, CultureVariation>>
    {
        public IReadOnlyDictionary<string, CultureVariation> ReadFrom(Stream stream)
        {
            // read variations count
            var pcount = PrimitiveSerializer.Int32.ReadFrom(stream);
            if (pcount == 0) return Empty;

            // read each variation
            var dict = new Dictionary<string, CultureVariation>(StringComparer.InvariantCultureIgnoreCase);
            for (var i = 0; i < pcount; i++)
            {
                var languageId = string.Intern(PrimitiveSerializer.String.ReadFrom(stream));
                var cultureVariation = new CultureVariation
                {
                    Name = ReadStringObject(stream),
                    UrlSegment = ReadStringObject(stream),
                    Date = ReadDateTime(stream)
                };
                dict[languageId] = cultureVariation;
            }
            return dict;
        }

        private static readonly IReadOnlyDictionary<string, CultureVariation> Empty = new Dictionary<string, CultureVariation>();

        public void WriteTo(IReadOnlyDictionary<string, CultureVariation> value, Stream stream)
        {
            var variations = value ?? Empty;

            // write variations count
            PrimitiveSerializer.Int32.WriteTo(variations.Count, stream);

            // write each variation
            foreach (var (culture, variation) in variations)
            {
                // TODO: it's weird we're dealing with cultures here, and languageId in properties

                PrimitiveSerializer.String.WriteTo(culture, stream); // should never be null
                WriteObject(variation.Name, stream); // write an object in case it's null (though... should not happen)
                WriteObject(variation.UrlSegment, stream); // write an object in case it's null (though... should not happen)
                PrimitiveSerializer.DateTime.WriteTo(variation.Date, stream);
            }
        }
    }
}
