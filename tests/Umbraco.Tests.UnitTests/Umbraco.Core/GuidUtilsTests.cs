// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

public class GuidUtilsTests
{
    [Test]
    public void GuidCombineMethodsAreEqual()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        Assert.AreEqual(GuidUtils.Combine(a, b).ToByteArray(), Combine(a, b));
    }

    [Test]
    public void GuidThingTest()
    {
        var guid = new Guid("f918382f-2bba-453f-a3e2-1f594016ed3b");
        Assert.AreEqual("f22br4n0fm5fli5c", GuidUtils.ToBase32String(guid, 16));
        Assert.AreEqual("f22br4n0f", GuidUtils.ToBase32String(guid, 9));
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
