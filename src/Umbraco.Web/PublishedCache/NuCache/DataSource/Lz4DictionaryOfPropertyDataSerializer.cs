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
        NucacheDatabase = 2
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
        private readonly IReadOnlyDictionary<string, (NucachePropertyCompressionLevel compress, NucachePropertyDecompressionLevel decompressionLevel, string mappedAlias)> _compressProperties;
        private readonly LZ4Level _compressionLevel;
        private readonly long? _minimumStringLengthForCompression;
        private readonly IReadOnlyDictionary<string, (NucachePropertyCompressionLevel compress, NucachePropertyDecompressionLevel decompressionLevel, string mappedAlias)> _uncompressProperties;


        public Lz4DictionaryOfPropertyDataSerializer(NucachePropertyOptions nucachePropertyOptions)
        {
            _compressProperties = nucachePropertyOptions.PropertyMap.ToList().ToDictionary(x => string.Intern(x.Value.mappedAlias), x => (x.Value.compress,x.Value.decompressionLevel, string.Intern(x.Value.mappedAlias)));
            _minimumStringLengthForCompression = nucachePropertyOptions.MinimumCompressibleStringLength;
            _uncompressProperties = _compressProperties.ToList().ToDictionary(x => x.Value.mappedAlias, x => (x.Value.compress, x.Value.decompressionLevel, x.Value.mappedAlias));

            _compressionLevel = nucachePropertyOptions.LZ4CompressionLevel;
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

                    var decompressionLevel = NucachePropertyDecompressionLevel.Immediate;
                    if ((map.Compress.Equals(NucachePropertyCompressionLevel.NucacheDatabase) || map.Compress.Equals(NucachePropertyCompressionLevel.SQLDatabase))
                        && pdata.Value != null && pdata.Value is byte[] byteArrayValue)
                    {
                        //Compressed string
                        switch (decompressionLevel)
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
                    if (pdata.Value is string stringValue && pdata.Value != null && map.Compress.Equals(NucachePropertyCompressionLevel.NucacheDatabase)
                        && (_minimumStringLengthForCompression == null
                        || !_minimumStringLengthForCompression.HasValue
                        || stringValue.Length > _minimumStringLengthForCompression.Value))
                    {
                        var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                        var compressedBytes = LZ4Pickler.Pickle(stringBytes, _compressionLevel);
                        WriteObject(compressedBytes, stream);
                    }
                    WriteObject(pdata.Value, stream);
                }
            }
        }
        private readonly (NucachePropertyCompressionLevel Compress, NucachePropertyDecompressionLevel decompressionLevel, string MappedAlias) DEFAULT_MAP =(NucachePropertyCompressionLevel.None, NucachePropertyDecompressionLevel.NotCompressed, null);
        public (NucachePropertyCompressionLevel Compress, NucachePropertyDecompressionLevel decompressionLevel, string MappedAlias) GetSerializationMap(string propertyAlias)
        {
            if (_compressProperties == null)
            {
                return DEFAULT_MAP;
            }
            if (_compressProperties.TryGetValue(propertyAlias, out (NucachePropertyCompressionLevel compress, NucachePropertyDecompressionLevel decompressionLevel, string mappedAlias) map))
            {
                return map;
            }

            return DEFAULT_MAP;
        }
        public (NucachePropertyCompressionLevel Compress, NucachePropertyDecompressionLevel decompressionLevel, string MappedAlias) GetDeSerializationMap(string propertyAlias)
        {
            if (_uncompressProperties == null)
            {
                return DEFAULT_MAP;
            }
            if (_uncompressProperties.TryGetValue(propertyAlias, out (NucachePropertyCompressionLevel compress, NucachePropertyDecompressionLevel decompressionLevel, string mappedAlias) map))
            {
                return map;
            }
            return DEFAULT_MAP;
        }
    }
}
