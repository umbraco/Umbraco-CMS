// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class IntExtensionsTests
{
    [TestCase(20, "00000014-0000-0000-0000-000000000000")]
    [TestCase(106, "0000006a-0000-0000-0000-000000000000")]
    [TestCase(999999, "000f423f-0000-0000-0000-000000000000")]
    [TestCase(555555555, "211d1ae3-0000-0000-0000-000000000000")]
    public void ToGuid_Creates_Expected_Guid(int input, string expected)
    {
        var result = input.ToGuid();
        Assert.AreEqual(expected, result.ToString());
    }
}
