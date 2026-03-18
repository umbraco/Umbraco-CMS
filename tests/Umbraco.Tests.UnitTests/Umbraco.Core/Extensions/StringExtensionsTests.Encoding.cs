// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

/// <summary>
/// Unit tests for the StringExtensions class.
/// </summary>
[TestFixture]
public partial class StringExtensionsTests
{
    /// <summary>
    /// Tests that decoding a hexadecimal string returns the expected decoded string.
    /// </summary>
    /// <param name="hexInput">The hexadecimal string to decode.</param>
    /// <param name="expected">The expected decoded string result.</param>
    [TestCase("", "")] // empty string
    [TestCase("48656c6c6f", "Hello")] // "Hello"
    [TestCase("41", "A")] // single char
    [TestCase("4142", "AB")] // two chars
    [TestCase("616263", "abc")] // lowercase
    [TestCase("313233", "123")] // digits
    [TestCase("20", " ")] // space
    [TestCase("48656c6c6f20576f726c64", "Hello World")] // with space
    [TestCase("21402324", "!@#$")] // special characters
    [TestCase("0d0a", "\r\n")] // CRLF
    [TestCase("09", "\t")] // tab
    public void DecodeFromHex_ReturnsExpectedResult(string hexInput, string expected)
    {
        var result = hexInput.DecodeFromHex();
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that decoding a hex string with an odd length throws an ArgumentException.
    /// </summary>
    [Test]
    public void DecodeFromHex_WithOddStringLength_ThrowsArgumentException()
    {
        var hexInput = "123";
        Assert.Throws<ArgumentException>(() => hexInput.DecodeFromHex());
    }

    /// <summary>
    /// Tests that encoding a string to hex and then decoding it back returns the original string.
    /// </summary>
    [Test]
    public void DecodeFromHex_RoundTrip_WithConvertToHex()
    {
        var original = "Hello World! 123";
        var hex = original.ConvertToHex();
        var decoded = hex.DecodeFromHex();
        Assert.AreEqual(original, decoded);
    }

    /// <summary>
    /// Tests that the ConvertToHex extension method returns the expected hexadecimal string representation of the input string.
    /// </summary>
    /// <param name="input">The input string to convert to hexadecimal.</param>
    /// <param name="expected">The expected hexadecimal string result.</param>
    [TestCase("", "")] // empty string
    [TestCase("A", "41")] // single char
    [TestCase("AB", "4142")] // two chars
    [TestCase("Hello", "48656c6c6f")] // word
    [TestCase("abc", "616263")] // lowercase
    [TestCase("123", "313233")] // digits
    [TestCase(" ", "20")] // space
    [TestCase("Hello World", "48656c6c6f20576f726c64")] // with space
    public void ConvertToHex_ReturnsExpectedResult(string input, string expected)
    {
        var result = input.ConvertToHex();
        Assert.AreEqual(expected, result);
    }
}
