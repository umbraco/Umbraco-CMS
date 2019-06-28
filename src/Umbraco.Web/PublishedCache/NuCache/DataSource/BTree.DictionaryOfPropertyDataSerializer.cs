﻿using System;
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
            var dict = new Dictionary<string, PropertyData[]>(StringComparer.InvariantCultureIgnoreCase);

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
                    //  even though - culture and segment should never be null here, as 'null' represents
                    //  the 'current' value, and string.Empty should be used to represent the invariant or
                    //  neutral values - PropertyData throws when getting nulls, so falling back to
                    //  string.Empty here - what else?
                    pdata.Culture = ReadStringObject(stream) ?? string.Empty;
                    pdata.Segment = ReadStringObject(stream) ?? string.Empty;
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
                    //  even though - culture and segment should never be null here,
                    //  see note in ReadFrom() method above
                    WriteObject(pdata.Culture ?? string.Empty, stream);
                    WriteObject(pdata.Segment ?? string.Empty, stream);
                    WriteObject(pdata.Value, stream);
                }
            }
        }
    }
}
