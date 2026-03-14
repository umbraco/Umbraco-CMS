// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Unit tests for the IntExtensions class.
/// </summary>
[TestFixture]
public class IntExtensionsTests
{
    /// <summary>
    /// Tests that the <see cref="IntExtensions.ToGuid(int)"/> extension method creates the expected GUID from the given integer input.
    /// </summary>
    /// <param name="input">The integer input to convert to a GUID.</param>
    /// <param name="expected">The expected GUID string representation.</param>
    [TestCase(20, "00000014-0000-0000-0000-000000000000")]
    [TestCase(106, "0000006a-0000-0000-0000-000000000000")]
    [TestCase(999999, "000f423f-0000-0000-0000-000000000000")]
    [TestCase(555555555, "211d1ae3-0000-0000-0000-000000000000")]
    public void ToGuid_Creates_Expected_Guid(int input, string expected)
    {
        var result = input.ToGuid();
        Assert.AreEqual(expected, result.ToString());
    }

    /// <summary>
    /// Verifies that <c>IntExtensions.TryParseFromGuid</c> correctly parses an integer value from a given GUID string, or fails as expected.
    /// </summary>
    /// <param name="input">A string representation of a GUID to attempt to parse as an integer.</param>
    /// <param name="expected">The expected integer value if parsing succeeds; otherwise, <c>null</c> if parsing should fail.</param>
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
            Assert.IsFalse(result);
            Assert.IsFalse(intValue.HasValue);
        }
        else
        {
            Assert.IsTrue(result);
            Assert.AreEqual(expected, intValue.Value);
        }
    }
}
