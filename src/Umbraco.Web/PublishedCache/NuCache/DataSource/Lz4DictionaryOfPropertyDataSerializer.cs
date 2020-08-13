using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpTest.Net.Serialization;
using Umbraco.Core;
using System.Text;
using K4os.Compression.LZ4;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// If/where to compress custom properties for nucache
    /// </summary>
    public enum NucachePropertyCompressionLevel
    {
        None = 0,
        SQLDatabase = 1,
        NuCacheDatabase = 2
    }
    /// <summary>
    /// If/where to decompress custom properties for nucache
    /// </summary>
    public enum NucachePropertyDecompressionLevel
    {
        NotCompressed = 0,
        Immediate = 1,
        Lazy = 2
    }


    internal class Lz4DictionaryOfPropertyDataSerializer : SerializerBase, ISerializer<IDictionary<string, PropertyData[]>>, IDictionaryOfPropertyDataSerializer
    {
        private readonly IReadOnlyDictionary<string, NuCacheCompressionOptions> _compressProperties;
        private readonly IReadOnlyDictionary<string, NuCacheCompressionOptions> _uncompressProperties;


        public Lz4DictionaryOfPropertyDataSerializer(INuCachePropertyOptionsFactory nucachePropertyOptionsFactory)
        {
            var nucachePropertyOptions = nucachePropertyOptionsFactory.GetNuCachePropertyOptions();
            _compressProperties = nucachePropertyOptions.PropertyMap.ToList().ToDictionary(x => string.Intern(x.Key), x => new NuCacheCompressionOptions(x.Value.CompressLevel, x.Value.DecompressLevel, string.Intern(x.Value.MappedAlias)));
            _uncompressProperties = _compressProperties.ToList().ToDictionary(x => x.Value.MappedAlias, x => new NuCacheCompressionOptions(x.Value.CompressLevel, x.Value.DecompressLevel, x.Key));

            _nucachePropertyOptions = nucachePropertyOptions;
        }


        public IDictionary<string, PropertyData[]> ReadFrom(Stream stream)
        {
            var dict = new Dictionary<string, PropertyData[]>(StringComparer.InvariantCultureIgnoreCase);

            // read properties count
            var pcount = PrimitiveSerializer.Int32.ReadFrom(stream);

            // read each property
            for (var i = 0; i < pcount; i++)
            {

                // read property alias
                var alias = PrimitiveSerializer.String.ReadFrom(stream);
                var map = GetDeSerializationMap(alias);
                var key = string.Intern(map.MappedAlias ?? alias);

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
                    pdata.Culture = ReadStringObject(stream, true) ?? string.Empty;
                    pdata.Segment = ReadStringObject(stream, true) ?? string.Empty;
                    pdata.Value = ReadObject(stream);

                    if ((map.CompressLevel.Equals(NucachePropertyCompressionLevel.NuCacheDatabase) || map.CompressLevel.Equals(NucachePropertyCompressionLevel.SQLDatabase))
                        && pdata.Value != null && pdata.Value is byte[] byteArrayValue)
                    {
                        //Compressed string
                        switch (map.DecompressLevel)
                        {
                            case NucachePropertyDecompressionLevel.Lazy:
                                pdata.Value = new LazyCompressedString(byteArrayValue);
                                break;
                            case NucachePropertyDecompressionLevel.NotCompressed:
                                break;//Shouldn't be any not compressed
                            case NucachePropertyDecompressionLevel.Immediate:
                            default:
                                pdata.Value = Encoding.UTF8.GetString(LZ4Pickler.Unpickle(byteArrayValue));
                                break;
                        }
                    }
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
                var map = GetSerializationMap(alias);

                // write alias
                PrimitiveSerializer.String.WriteTo(map.MappedAlias ?? alias, stream);

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

                    //Only compress strings
                    if (pdata.Value is string stringValue && pdata.Value != null && map.CompressLevel.Equals(NucachePropertyCompressionLevel.NuCacheDatabase)
                        && (_nucachePropertyOptions.MinimumCompressibleStringLength == null
                        || !_nucachePropertyOptions.MinimumCompressibleStringLength.HasValue
                        || stringValue.Length > _nucachePropertyOptions.MinimumCompressibleStringLength.Value))
                    {
                        var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                        var compressedBytes = LZ4Pickler.Pickle(stringBytes, _nucachePropertyOptions.LZ4CompressionLevel);
                        WriteObject(compressedBytes, stream);
                    }
                    WriteObject(pdata.Value, stream);
                }
            }
        }
        private static readonly NuCacheCompressionOptions DefaultMap = new NuCacheCompressionOptions(NucachePropertyCompressionLevel.None, NucachePropertyDecompressionLevel.NotCompressed, null);
        private readonly NuCachePropertyOptions _nucachePropertyOptions;

        public NuCacheCompressionOptions GetSerializationMap(string propertyAlias)
        {
            if (_compressProperties == null)
            {
                return DefaultMap;
            }
            if (_compressProperties.TryGetValue(propertyAlias, out var map1))
            {
                return map1;
            }

            return DefaultMap;
        }
        public NuCacheCompressionOptions GetDeSerializationMap(string propertyAlias)
        {
            if (_uncompressProperties == null)
            {
                return DefaultMap;
            }
            if (_uncompressProperties.TryGetValue(propertyAlias, out var map2))
            {
                return map2;
            }
            return DefaultMap;
        }
    }
}
