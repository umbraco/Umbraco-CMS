// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class StringExtensionsPerformanceTests
{
    [TestCase("hello world", "helloworld")]
    [TestCase("  spaces  everywhere  ", "spaceseverywhere")]
    [TestCase("tabs\there", "tabshere")]
    [TestCase("new\nlines", "newlines")]
    public void StripWhitespace_RemovesAllWhitespace(string input, string expected)
        => Assert.AreEqual(expected, input.StripWhitespace());

    [TestCase("file.txt", ".txt")]
    [TestCase("path/to/file.png", ".png")]
    [TestCase("file.tar.gz", ".gz")]
    [TestCase("noextension", "")]
    public void GetFileExtension_ReturnsCorrectExtension(string input, string expected)
        => Assert.AreEqual(expected, input.GetFileExtension());

    [TestCase("<p>Hello</p>", "Hello")]
    [TestCase("<div><span>Text</span></div>", "Text")]
    [TestCase("No tags here", "No tags here")]
    [TestCase("<br/>", "")]
    public void StripHtml_RemovesAllHtmlTags(string input, string expected)
        => Assert.AreEqual(expected, input.StripHtml());

    [TestCase('a', true)]
    [TestCase('z', true)]
    [TestCase('A', false)]
    [TestCase('Z', false)]
    [TestCase('5', true)]
    public void IsLowerCase_ReturnsCorrectResult(char input, bool expected)
        => Assert.AreEqual(expected, input.IsLowerCase());

    [TestCase('A', true)]
    [TestCase('Z', true)]
    [TestCase('a', false)]
    [TestCase('z', false)]
    [TestCase('5', true)]
    public void IsUpperCase_ReturnsCorrectResult(char input, bool expected)
        => Assert.AreEqual(expected, input.IsUpperCase());

    [TestCase("hello-world", "-", "hello-world")]
    [TestCase("test_123", "_", "test_123")]
    [TestCase("abc!@#def", "***", "abc*********def")]
    public void ReplaceNonAlphanumericChars_String_ReplacesCorrectly(string input, string replacement, string expected)
        => Assert.AreEqual(expected, input.ReplaceNonAlphanumericChars(replacement));
}
