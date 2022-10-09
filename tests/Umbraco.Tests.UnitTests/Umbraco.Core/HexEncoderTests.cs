// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

public class HexEncoderTests
{
    private static readonly char[] s_bytesToHexStringLookup =
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F',
    };

    [Test]
    public void ToHexStringCreatesCorrectValue()
    {
        var buffer = new byte[255];
        var random = new Random();
        random.NextBytes(buffer);

        var sb = new StringBuilder(buffer.Length * 2);
        for (var i = 0; i < buffer.Length; i++)
        {
            sb.Append(buffer[i].ToString("X2"));
        }

        var expected = sb.ToString();

        var actual = HexEncoder.Encode(buffer);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ToHexStringWithSeparatorCreatesCorrectValue()
    {
        var buffer = new byte[255];
        var random = new Random();
        random.NextBytes(buffer);

        var expected = ToHexString(buffer, '/', 2, 4);
        var actual = HexEncoder.Encode(buffer, '/', 2, 4);

        Assert.AreEqual(expected, actual);
    }

    // Reference implementation taken from original extension method.
    private static string ToHexString(byte[] bytes, char separator, int blockSize, int blockCount)
    {
        int p = 0, bytesLength = bytes.Length, count = 0, size = 0;
        var chars = new char[(bytesLength * 2) + blockCount];
        for (var i = 0; i < bytesLength; i++)
        {
            var b = bytes[i];
            chars[p++] = s_bytesToHexStringLookup[b / 0x10];
            chars[p++] = s_bytesToHexStringLookup[b % 0x10];
            if (count == blockCount)
            {
                continue;
            }

            if (++size < blockSize)
            {
                continue;
            }

            chars[p++] = separator;
            size = 0;
            count++;
        }

        return new string(chars, 0, chars.Length);
    }
}
