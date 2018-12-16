using System;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.CoreThings
{
    public class GuidUtilsTests
    {
        [Test]
        public void GuidCombineMethodsAreEqual()
        {
            var a = Guid.NewGuid();
            var b = Guid.NewGuid();

            Assert.AreEqual(GuidUtils.Combine(a, b).ToByteArray(), Combine(a, b));
        }

        // Reference implementation taken from original code.
        private static byte[] Combine(Guid guid1, Guid guid2)
        {
            var bytes1 = guid1.ToByteArray();
            var bytes2 = guid2.ToByteArray();
            var bytes = new byte[bytes1.Length];
            for (var i = 0; i < bytes1.Length; i++)
            {
                bytes[i] = (byte)(bytes1[i] ^ bytes2[i]);
            }

            return bytes;
        }
    }
}
