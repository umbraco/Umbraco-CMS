using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{

    public class NuCachePropertyCompressionOptions
    {
        /// <summary>
        /// Returns empty options
        /// </summary>
        public static NuCachePropertyCompressionOptions Empty { get; } = new NuCachePropertyCompressionOptions();

        private NuCachePropertyCompressionOptions()
        {
        }

        public NuCachePropertyCompressionOptions(IReadOnlyDictionary<string, NuCacheCompressionOptions> propertyMap, LZ4Level lZ4CompressionLevel, long? minimumCompressibleStringLength)
        {
            PropertyMap = propertyMap ?? throw new ArgumentNullException(nameof(propertyMap));
            LZ4CompressionLevel = lZ4CompressionLevel;
            MinimumCompressibleStringLength = minimumCompressibleStringLength;
        }

        public IReadOnlyDictionary<string, NuCacheCompressionOptions> PropertyMap { get; } = new Dictionary<string, NuCacheCompressionOptions>();

        public LZ4Level LZ4CompressionLevel { get; } = LZ4Level.L00_FAST;

        // TODO: Unsure if we really want to keep this
        public long? MinimumCompressibleStringLength { get; }
    }
}
