using System;

namespace Umbraco.Core
{
    internal static class GuidExtensions
    {
        internal static byte[] CombineToBytes(Guid guid1, Guid guid2)
        {
            var bytes1 = guid1.ToByteArray();
            var bytes2 = guid2.ToByteArray();
            var bytes = new byte[bytes1.Length];
            for (var i = 0; i < bytes1.Length; i++)
                bytes[i] = (byte)(bytes1[i] ^ bytes2[i]);
            return bytes;
        }

        internal static byte[] CombineToBytes(Guid guid1, Guid guid2, Guid guid3)
        {
            var bytes1 = guid1.ToByteArray();
            var bytes2 = guid2.ToByteArray();
            var bytes3 = guid3.ToByteArray();
            var bytes = new byte[bytes1.Length];
            for (var i = 0; i < bytes1.Length; i++)
                bytes[i] = (byte)(bytes1[i] ^ bytes2[i] ^ bytes3[i]);
            return bytes;
        }

        internal static Guid Combine(Guid guid1, Guid guid2)
        {
            var bytes = CombineToBytes(guid1, guid2);
            return new Guid(bytes);
        }

        internal static Guid Combine(Guid guid1, Guid guid2, Guid guid3)
        {
            var bytes = CombineToBytes(guid1, guid2, guid3);
            return new Guid(bytes);
        }
    }
}