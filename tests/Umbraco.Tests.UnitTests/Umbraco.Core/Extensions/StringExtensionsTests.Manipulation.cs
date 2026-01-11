// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public partial class StringExtensionsTests
{
    [TestCase("hello", "-", "hello")] // no change needed
    [TestCase("Hello123", "-", "Hello123")] // alphanumeric only
    [TestCase("hello world", "-", "hello-world")] // space replaced
    [TestCase("hello-world", "_", "hello_world")] // hyphen replaced
    [TestCase("hello world!", "-", "hello-world-")] // space and punctuation
    [TestCase("a b c", "", "abc")] // empty replacement removes chars
    [TestCase("a.b.c", "--", "a--b--c")] // multi-char replacement
    [TestCase("test@email.com", "_", "test_email_com")] // email-like string
    [TestCase("", "-", "")] // empty string
    [TestCase("@#$%", "-", "----")] // all non-alphanumeric
    [TestCase("café", "-", "café")] // unicode letters preserved
    [TestCase("über", "-", "über")] // unicode letters preserved
    [TestCase("日本語", "-", "日本語")] // CJK characters preserved
    [TestCase("hello\tworld", "-", "hello-world")] // tab replaced
    [TestCase("hello\nworld", "-", "hello-world")] // newline replaced
    [TestCase("  multiple  spaces  ", "-", "--multiple--spaces--")] // multiple spaces
    public void ReplaceNonAlphanumericChars_WithStringReplacement_ReturnsExpectedResult(
        string input,
        string replacement,
        string expected)
    {
        var result = input.ReplaceNonAlphanumericChars(replacement);
        Assert.AreEqual(expected, result);
    }

    [TestCase("hello", '-', "hello")] // no change needed
    [TestCase("Hello123", '-', "Hello123")] // alphanumeric only
    [TestCase("hello world", '-', "hello-world")] // space replaced
    [TestCase("hello-world", '_', "hello_world")] // hyphen replaced
    [TestCase("hello world!", '-', "hello-world-")] // space and punctuation
    [TestCase("test@email.com", '_', "test_email_com")] // email-like string
    [TestCase("", '-', "")] // empty string
    [TestCase("@#$%", '-', "----")] // all non-alphanumeric
    [TestCase("café", '-', "café")] // unicode letters preserved
    [TestCase("über", '-', "über")] // unicode letters preserved
    [TestCase("日本語", '-', "日本語")] // CJK characters preserved
    [TestCase("hello\tworld", '-', "hello-world")] // tab replaced
    [TestCase("hello\nworld", '-', "hello-world")] // newline replaced
    [TestCase("  multiple  spaces  ", '-', "--multiple--spaces--")] // multiple spaces
    [TestCase("a1!b2@c3#", '_', "a1_b2_c3_")] // mixed content
    public void ReplaceNonAlphanumericChars_WithCharReplacement_ReturnsExpectedResult(
        string input,
        char replacement,
        string expected)
    {
        var result = input.ReplaceNonAlphanumericChars(replacement);
        Assert.AreEqual(expected, result);
    }

    [TestCase("hello", "hello")] // no whitespace
    [TestCase("Hello123", "Hello123")] // alphanumeric only
    [TestCase("hello world", "helloworld")] // single space
    [TestCase("hello  world", "helloworld")] // multiple spaces
    [TestCase("", "")] // empty string
    [TestCase("   ", "")] // only spaces
    [TestCase("hello\tworld", "helloworld")] // tab
    [TestCase("hello\nworld", "helloworld")] // newline
    [TestCase("hello\rworld", "helloworld")] // carriage return
    [TestCase("hello\r\nworld", "helloworld")] // CRLF
    [TestCase(" hello ", "hello")] // leading and trailing spaces
    [TestCase("\t\n\r ", "")] // only whitespace characters
    [TestCase("a b\tc\nd\re", "abcde")] // mixed whitespace types
    [TestCase("café", "café")] // unicode letters preserved
    [TestCase("日本語", "日本語")] // CJK characters preserved
    [TestCase("hello\u00A0world", "helloworld")] // non-breaking space
    [TestCase("hello\u2003world", "helloworld")] // em space
    [TestCase("a\u0009\u000A\u000B\u000C\u000D\u0020b", "ab")] // various ASCII whitespace
    public void StripWhitespace_ReturnsStringWithoutWhitespace(string input, string expected)
    {
        var result = input.StripWhitespace();
        Assert.AreEqual(expected, result);
    }
}
