using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    public class NuCachePropertyOptions
    {
        public IReadOnlyDictionary<string, (NucachePropertyCompressionLevel compress, NucachePropertyDecompressionLevel decompressionLevel,
            string mappedAlias)> PropertyMap
        { get; set; } = new Dictionary<string, (NucachePropertyCompressionLevel compress,
                NucachePropertyDecompressionLevel decompressionLevel, string mappedAlias)>();

        public K4os.Compression.LZ4.LZ4Level LZ4CompressionLevel { get; set; } = K4os.Compression.LZ4.LZ4Level.L00_FAST;

        public long? MinimumCompressibleStringLength { get; set; }
    }
}
