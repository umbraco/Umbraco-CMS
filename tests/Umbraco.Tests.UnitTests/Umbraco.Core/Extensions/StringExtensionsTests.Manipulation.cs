// Copyright (c) Umbraco.
// See LICENSE for more details.

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

    [TestCase("hello", "hello")] // no HTML
    [TestCase("", "")] // empty string
    [TestCase("<p>hello</p>", "hello")] // simple tags
    [TestCase("<div>hello</div>", "hello")] // div tags
    [TestCase("<br/>", "")] // self-closing tag
    [TestCase("<br />", "")] // self-closing with space
    [TestCase("<img src=\"test.jpg\"/>", "")] // self-closing with attributes
    [TestCase("<a href=\"url\">link</a>", "link")] // tag with attributes
    [TestCase("<p class=\"test\" id=\"1\">text</p>", "text")] // multiple attributes
    [TestCase("<div><p>nested</p></div>", "nested")] // nested tags
    [TestCase("<p>one</p><p>two</p>", "onetwo")] // multiple tags
    [TestCase("before<p>middle</p>after", "beforemiddleafter")] // text around tags
    [TestCase("<p>line1\nline2</p>", "line1\nline2")] // preserves newlines in content
    [TestCase("<p\nclass=\"test\">text</p>", "text")] // tag spanning lines
    [TestCase("<script>alert('xss')</script>", "alert('xss')")] // script tag (content preserved)
    [TestCase("<!-- comment -->", "")] // HTML comment
    [TestCase("<!DOCTYPE html>", "")] // doctype
    [TestCase("<p>Hello &amp; World</p>", "Hello &amp; World")] // HTML entities preserved
    [TestCase("<>", "<>")] // empty angle brackets (not valid HTML tag)
    [TestCase("5 < 10 > 3", "5 < 10 > 3")] // comparison operators (not tags)
    public void StripHtml_ReturnsStringWithoutHtmlTags(string input, string expected)
    {
        var result = input.StripHtml();
        Assert.AreEqual(expected, result);
    }

    [TestCase("hello", '/', "/hello")] // prepends char
    [TestCase("/hello", '/', "/hello")] // already starts with char
    [TestCase("", '/', "/")] // empty string
    [TestCase("hello", 'h', "hello")] // already starts with char
    [TestCase("Hello", 'h', "hHello")] // case sensitive
    [TestCase("world", '!', "!world")] // special char
    [TestCase(" hello", ' ', " hello")] // space char already present
    [TestCase("hello", ' ', " hello")] // prepend space
    public void EnsureStartsWith_WithChar_ReturnsExpectedResult(string input, char value, string expected)
    {
        var result = input.EnsureStartsWith(value);
        Assert.AreEqual(expected, result);
    }

    [TestCase("hello", "/", "/hello")] // prepends string
    [TestCase("/hello", "/", "/hello")] // already starts with string
    [TestCase("", "/", "/")] // empty string
    [TestCase("hello", "hel", "hello")] // already starts with string
    [TestCase("Hello", "hel", "helHello")] // case sensitive
    [TestCase("world", "Hello ", "Hello world")] // multi-char prefix
    [TestCase("hello", "", "hello")] // empty prefix
    [TestCase("//hello", "/", "//hello")] // already starts, has duplicates
    public void EnsureStartsWith_WithString_ReturnsExpectedResult(string input, string toStartWith, string expected)
    {
        var result = input.EnsureStartsWith(toStartWith);
        Assert.AreEqual(expected, result);
    }

    [TestCase("hello", '/', "hello/")] // appends char
    [TestCase("hello/", '/', "hello/")] // already ends with char
    [TestCase("", '/', "/")] // empty string
    [TestCase("hello", 'o', "hello")] // already ends with char
    [TestCase("hellO", 'o', "hellOo")] // case sensitive
    [TestCase("world", '!', "world!")] // special char
    [TestCase("hello ", ' ', "hello ")] // space char already present
    [TestCase("hello", ' ', "hello ")] // append space
    public void EnsureEndsWith_WithChar_ReturnsExpectedResult(string input, char value, string expected)
    {
        var result = input.EnsureEndsWith(value);
        Assert.AreEqual(expected, result);
    }

    [TestCase("hello", "/", "hello/")] // appends string
    [TestCase("hello/", "/", "hello/")] // already ends with string
    [TestCase("", "/", "/")] // empty string
    [TestCase("hello", "llo", "hello")] // already ends with string
    [TestCase("hellO", "llo", "hellOllo")] // case sensitive
    [TestCase("Hello", " world", "Hello world")] // multi-char suffix
    [TestCase("hello", "", "hello")] // empty suffix
    [TestCase("hello//", "/", "hello//")] // already ends, has duplicates
    public void EnsureEndsWith_WithString_ReturnsExpectedResult(string input, string toEndWith, string expected)
    {
        var result = input.EnsureEndsWith(toEndWith);
        Assert.AreEqual(expected, result);
    }

    [TestCase("hello", "hello")] // no newlines
    [TestCase("", "")] // empty string
    [TestCase("hello\nworld", "helloworld")] // LF removed
    [TestCase("hello\rworld", "helloworld")] // CR removed
    [TestCase("hello\r\nworld", "helloworld")] // CRLF removed
    [TestCase("\n\r\n\r", "")] // only newlines
    [TestCase("line1\nline2\nline3", "line1line2line3")] // multiple LF
    [TestCase("line1\r\nline2\r\nline3", "line1line2line3")] // multiple CRLF
    [TestCase("  hello\n  world  ", "  hello  world  ")] // preserves spaces
    [TestCase("hello\tworld", "hello\tworld")] // preserves tabs
    [TestCase("\nhello\n", "hello")] // leading and trailing newlines
    public void StripNewLines_ReturnsStringWithoutNewLines(string input, string expected)
    {
        var result = input.StripNewLines();
        Assert.AreEqual(expected, result);
    }

    [TestCase(null, null)] // null input
    [TestCase("hello", "hello")] // no newlines
    [TestCase("", "")] // empty string
    [TestCase("hello\nworld", "hello world")] // LF to space
    [TestCase("hello\rworld", "hello world")] // CR to space
    [TestCase("hello\r\nworld", "hello world")] // CRLF to single space
    [TestCase("\n\r\n\r", "   ")] // only newlines become spaces
    [TestCase("line1\nline2\nline3", "line1 line2 line3")] // multiple LF
    [TestCase("line1\r\nline2\r\nline3", "line1 line2 line3")] // multiple CRLF
    [TestCase("  hello\n  world  ", "  hello   world  ")] // preserves existing spaces
    [TestCase("hello\tworld", "hello\tworld")] // preserves tabs
    [TestCase("\nhello\n", " hello ")] // leading and trailing become spaces
    public void ToSingleLine_ReplacesNewLinesWithSpaces(string? input, string expected)
    {
        var result = input.ToSingleLine();
        Assert.AreEqual(expected, result);
    }

    [TestCase("hello", 10, "...", "hello")] // shorter than limit
    [TestCase("hello", 5, "...", "hello")] // exactly at limit
    [TestCase("hello world", 8, "...", "hello...")]  // truncated with default suffix
    [TestCase("hello world", 8, "..", "hello..")]  // trailing space trimmed before suffix
    [TestCase("hello world", 5, "...", "he...")] // tight truncation
    [TestCase("hello world", 11, "...", "hello world")] // exactly at limit
    [TestCase("hello world", 12, "...", "hello world")] // longer than text
    [TestCase("hello world", 0, "...", "hello world")] // zero maxLength returns original
    [TestCase("hello world", -1, "...", "hello world")] // negative maxLength returns original
    [TestCase("hello world", 3, "...", "hello world")] // maxLength equal to suffix length returns original
    [TestCase("hello world", 2, "...", "hello world")] // maxLength less than suffix length returns original
    [TestCase("hello world", 4, "...", "h...")] // maxLength just above suffix length
    [TestCase("hello   world", 10, "...", "hello...")] // trailing spaces trimmed before suffix
    public void Truncate_ReturnsExpectedResult(string input, int maxLength, string suffix, string expected)
    {
        var result = input.Truncate(maxLength, suffix);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void TruncateWithUniqueHash_Short_String_Returns_Unchanged()
    {
        var result = "hello world".TruncateWithUniqueHash(50);

        Assert.AreEqual("hello world", result);
    }

    [Test]
    public void TruncateWithUniqueHash_Exactly_At_Limit_Returns_Unchanged()
    {
        var text = new string('x', 50);

        var result = text.TruncateWithUniqueHash(50);

        Assert.AreEqual(text, result);
    }

    [Test]
    public void TruncateWithUniqueHash_Exceeds_Limit_Truncates_To_Max_Length()
    {
        var text = new string('x', 51);

        var result = text.TruncateWithUniqueHash(50);

        Assert.That(result.Length, Is.EqualTo(50));
    }

    [Test]
    public void TruncateWithUniqueHash_Exceeds_Limit_Ends_With_Hash_Suffix()
    {
        var text = new string('x', 100);

        var result = text.TruncateWithUniqueHash(50);

        // Last 9 chars should be "-" + 8 hex digits.
        Assert.That(result[^9], Is.EqualTo('-'));
        Assert.That(result[^8..], Does.Match("^[0-9a-f]{8}$"));
    }

    [Test]
    public void TruncateWithUniqueHash_Preserves_Readable_Prefix()
    {
        var text = "RebuildExamineIndex-MyCustomIndex" + new string('x', 200);

        var result = text.TruncateWithUniqueHash(50);

        Assert.That(result, Does.StartWith("RebuildExamineIndex-MyCustomIndex"));
    }

    [Test]
    public void TruncateWithUniqueHash_Different_Inputs_Produce_Different_Results()
    {
        var textA = new string('a', 100);
        var textB = new string('b', 100);

        var resultA = textA.TruncateWithUniqueHash(50);
        var resultB = textB.TruncateWithUniqueHash(50);

        Assert.AreNotEqual(resultA, resultB);
    }

    [Test]
    public void TruncateWithUniqueHash_Inputs_Sharing_Prefix_Produce_Different_Results()
    {
        var sharedPrefix = new string('x', 100);
        var textA = sharedPrefix + "_alpha";
        var textB = sharedPrefix + "_bravo";

        var resultA = textA.TruncateWithUniqueHash(50);
        var resultB = textB.TruncateWithUniqueHash(50);

        Assert.That(resultA.Length, Is.EqualTo(50));
        Assert.That(resultB.Length, Is.EqualTo(50));
        Assert.AreNotEqual(resultA, resultB);
    }

    [Test]
    public void TruncateWithUniqueHash_Same_Input_Is_Deterministic()
    {
        var text = new string('z', 100);

        var result1 = text.TruncateWithUniqueHash(50);
        var result2 = text.TruncateWithUniqueHash(50);

        Assert.AreEqual(result1, result2);
    }
}
