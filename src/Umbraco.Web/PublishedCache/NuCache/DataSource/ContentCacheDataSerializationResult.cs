using System;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    /// <summary>
    /// The serialization result from <see cref="IContentCacheDataSerializer"/> for which the serialized value
    /// will be either a string or a byte[]
    /// </summary>
    public struct ContentCacheDataSerializationResult : IEquatable<ContentCacheDataSerializationResult>
    {
        public ContentCacheDataSerializationResult(string stringData, byte[] byteData)
        {
            StringData = stringData;
            ByteData = byteData;
        }

        public string StringData { get; }
        public byte[] ByteData { get; }

        public override bool Equals(object obj)
        {
            return obj is ContentCacheDataSerializationResult result && Equals(result);
        }

        public bool Equals(ContentCacheDataSerializationResult other)
        {
            return StringData == other.StringData &&
                   EqualityComparer<byte[]>.Default.Equals(ByteData, other.ByteData);
        }

        public override int GetHashCode()
        {
            var hashCode = 1910544615;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StringData);
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(ByteData);
            return hashCode;
        }

        public static bool operator ==(ContentCacheDataSerializationResult left, ContentCacheDataSerializationResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ContentCacheDataSerializationResult left, ContentCacheDataSerializationResult right)
        {
            return !(left == right);
        }
    }

}
