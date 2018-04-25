using System.Collections.Generic;
using System.IO;
using CSharpTest.Net.Serialization;
using Umbraco.Core;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class DictionaryOfPropertyDataSerializer : SerializerBase, ISerializer<IDictionary<string, PropertyData[]>>
    {
        public IDictionary<string, PropertyData[]> ReadFrom(Stream stream)
        {
            var dict = new Dictionary<string, PropertyData[]>();

            // read properties count
            var pcount = PrimitiveSerializer.Int32.ReadFrom(stream);

            // read each property
            for (var i = 0; i < pcount; i++)
            {
                // read property alias
                var key = PrimitiveSerializer.String.ReadFrom(stream);

                // read values count
                var vcount = PrimitiveSerializer.Int32.ReadFrom(stream);

                // create pdata and add to the dictionary
                var pdatas = new List<PropertyData>();

                // for each value, read and add to pdata
                for (var j = 0; j < vcount; j++)
                {
                    var pdata = new PropertyData();
                    pdatas.Add(pdata);

                    // everything that can be null is read/written as object
                    pdata.LanguageId = ReadIntObject(stream);
                    pdata.Segment = ReadStringObject(stream);
                    pdata.Value = ReadObject(stream);
                }

                dict[key] = pdatas.ToArray();
            }
            return dict;
        }

        public void WriteTo(IDictionary<string, PropertyData[]> value, Stream stream)
        {
            // write properties count
            PrimitiveSerializer.Int32.WriteTo(value.Count, stream);

            // write each property
            foreach (var (alias, values) in value)
            {
                // write alias
                PrimitiveSerializer.String.WriteTo(alias, stream);

                // write values count
                PrimitiveSerializer.Int32.WriteTo(values.Length, stream);

                // write each value
                foreach (var pdata in values)
                {
                    // everything that can be null is read/written as object
                    WriteObject(pdata.LanguageId, stream);
                    WriteObject(pdata.Segment, stream);
                    WriteObject(pdata.Value, stream);
                }
            }
        }
    }
}
