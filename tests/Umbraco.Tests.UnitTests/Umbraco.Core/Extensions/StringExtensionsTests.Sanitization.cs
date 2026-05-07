// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public partial class StringExtensionsTests
{
    [TestCase("[]", true)]
    [TestCase("{}", true)]
    [TestCase("  [ ]  ", true)]
    [TestCase("{ \t\n }", true)]
    [TestCase("[1]", false)]
    [TestCase("{\"a\":1}", false)]
    [TestCase("", false)]
    [TestCase("null", false)]
    [TestCase("[}", false)]
    public void DetectIsEmptyJson_ReturnsExpected(string input, bool expected)
    {
        var result = input.DetectIsEmptyJson();
        Assert.AreEqual(expected, result);
    }

    [TestCase("", "")] // empty short-circuits
    [TestCase("hello", "hello")] // plain text preserved
    [TestCase("tab\there\nlf\rcr", "tab\there\nlf\rcr")] // valid XML whitespace preserved
    [TestCase("bad\u0000char", "badchar")] // NUL stripped
    [TestCase("bell\u0007end", "bellend")] // BEL stripped
    [TestCase("del\u007Fend", "delend")] // DEL stripped
    [TestCase("emoji:\uD83D\uDE00!", "emoji:\uD83D\uDE00!")] // valid surrogate pair preserved
    [TestCase("\uFEFFbom", "bom")] // BOM stripped
    [TestCase("café", "café")] // unicode letters preserved
    public void ToValidXmlString_RemovesInvalidCharacters(string input, string expected)
    {
        var result = input.ToValidXmlString();
        Assert.AreEqual(expected, result);
    }

    // Lone surrogates cannot survive round-tripping through [TestCase] attribute arguments
    // (stored as UTF-8 in metadata), so these cases must use method-body string literals.
    [Test]
    public void ToValidXmlString_StripsLoneSurrogates()
    {
        Assert.Multiple(() =>
        {
            Assert.AreEqual("hi!", "hi\uD83D!".ToValidXmlString()); // lone high
            Assert.AreEqual("hi!", "hi\uDE00!".ToValidXmlString()); // lone low
        });
    }
}
