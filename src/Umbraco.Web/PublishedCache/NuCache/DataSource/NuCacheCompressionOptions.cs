using System;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    public struct NuCacheCompressionOptions : IEquatable<NuCacheCompressionOptions>
    {
        public NuCacheCompressionOptions(NucachePropertyCompressionLevel compressLevel, NucachePropertyDecompressionLevel decompressLevel, string mappedAlias)
        {
            CompressLevel = compressLevel;
            DecompressLevel = decompressLevel;
            MappedAlias = mappedAlias ?? throw new ArgumentNullException(nameof(mappedAlias));
        }

        public NucachePropertyCompressionLevel CompressLevel { get; private set; }
        public NucachePropertyDecompressionLevel DecompressLevel { get; private set; }
        public string MappedAlias { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is NuCacheCompressionOptions options && Equals(options);
        }

        public bool Equals(NuCacheCompressionOptions other)
        {
            return CompressLevel == other.CompressLevel &&
                   DecompressLevel == other.DecompressLevel &&
                   MappedAlias == other.MappedAlias;
        }

        public override int GetHashCode()
        {
            var hashCode = 961370163;
            hashCode = hashCode * -1521134295 + CompressLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + DecompressLevel.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(MappedAlias);
            return hashCode;
        }

        public static bool operator ==(NuCacheCompressionOptions left, NuCacheCompressionOptions right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NuCacheCompressionOptions left, NuCacheCompressionOptions right)
        {
            return !(left == right);
        }
    }
}
