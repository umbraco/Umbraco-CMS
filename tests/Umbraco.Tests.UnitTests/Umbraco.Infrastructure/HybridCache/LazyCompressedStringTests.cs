// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using K4os.Compression.LZ4;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HybridCache;

[TestFixture]
public class LazyCompressedStringTests
{
    [Test]
    public void Can_Report_Compressed_Byte_Length_Without_Decompressing()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        var sut = new LazyCompressedString(bytes);

        Assert.That(sut.GetApproximateByteCount(), Is.EqualTo(bytes.Length));

        // The value must still be compressed afterwards — GetBytes throws once decompressed, so this
        // guards against GetApproximateByteCount being "simplified" into forcing a decompression.
        Assert.DoesNotThrow(() => sut.GetBytes());
    }

    [Test]
    public void Can_Report_String_Length_After_Decompression()
    {
        const string value = "hello world";
        var sut = new LazyCompressedString(LZ4Pickler.Pickle(Encoding.UTF8.GetBytes(value)));

        sut.DecompressString();

        Assert.That(sut.GetApproximateByteCount(), Is.EqualTo(value.Length));
    }
}
