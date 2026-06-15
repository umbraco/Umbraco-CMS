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
        Assert.That(result.ToString(), Is.EqualTo(expected));
    }

    [TestCase("00000014-0000-0000-0000-000000000000", 20)]
    [TestCase("0000006a-0000-0000-0000-000000000000", 106)]
    [TestCase("000f423f-0000-0000-0000-000000000000", 999999)]
    [TestCase("211d1ae3-0000-0000-0000-000000000000", 555555555)]
    [TestCase("0d93047e-558d-4311-8a9d-b89e6fca0337", null)]
    public void TryParseFromGuid_Parses_Expected_Integer(string input, int? expected)
    {
        var result = IntExtensions.TryParseFromGuid(Guid.Parse(input), out int? intValue);
        if (expected is null)
        {
            Assert.That(result, Is.False);
            Assert.That(intValue.HasValue, Is.False);
        }
        else
        {
            Assert.That(result, Is.True);
            Assert.That(intValue.Value, Is.EqualTo(expected));
        }
    }
}
