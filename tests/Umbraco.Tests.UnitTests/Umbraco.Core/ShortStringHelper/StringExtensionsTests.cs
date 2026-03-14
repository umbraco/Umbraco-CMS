// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

/// <summary>
/// Unit tests for the extension methods of the StringExtensions class in the ShortStringHelper namespace.
/// </summary>
[TestFixture]
public class StringExtensionsTests
{
    private readonly IShortStringHelper _mockShortStringHelper = new MockShortStringHelper();

    /// <summary>
    /// Verifies that the <c>ToFriendlyName</c> extension method correctly converts an input string into a human-friendly name format.
    /// </summary>
    /// <param name="first">The original input string to be converted.</param>
    /// <param name="second">The expected result after conversion to a friendly name.</param>
    [TestCase("hello-world.png", "Hello World")]
    [TestCase("hello-world .png", "Hello World")]
    [TestCase("_hello-world __1.png", "Hello World 1")]
    public void To_Friendly_Name(string first, string second) => Assert.AreEqual(first.ToFriendlyName(), second);

    /// <summary>
    /// Verifies that the String.ToGuid() extension method produces equal GUIDs for identical strings and different GUIDs for different strings.
    /// </summary>
    /// <param name="first">The first string to convert to a GUID.</param>
    /// <param name="second">The second string to convert to a GUID.</param>
    /// <param name="result">True if the GUIDs generated from <paramref name="first"/> and <paramref name="second"/> are expected to be equal; otherwise, false.</param>
    [TestCase("hello", "world", false)]
    [TestCase("hello", "hello", true)]
    [TestCase("hellohellohellohellohellohellohello", "hellohellohellohellohellohellohelloo", false)]
    [TestCase(
        "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello",
        "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohelloo",
        false)]
    [TestCase(
        "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello",
        "hellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohellohello",
        true)]
    public void String_To_Guid(string first, string second, bool result)
    {
        Debug.Print("First: " + first.ToGuid());
        Debug.Print("Second: " + second.ToGuid());
        Assert.AreEqual(result, first.ToGuid() == second.ToGuid());
    }

    /// <summary>
    /// Verifies that the <see cref="StringExtensions.StripFileExtension(string)"/> extension method correctly removes the file extension from a given string.
    /// </summary>
    /// <param name="input">A string that may contain a file extension.</param>
    /// <param name="result">The expected string after removing the file extension from <paramref name="input"/>.</param>
    [TestCase("hello.txt", "hello")]
    [TestCase("this.is.a.Txt", "this.is.a")]
    [TestCase("this.is.not.a. Txt", "this.is.not.a. Txt")]
    [TestCase("not a file", "not a file")]
    public void Strip_File_Extension(string input, string result)
    {
        var stripped = input.StripFileExtension();
        Assert.AreEqual(stripped, result);
    }

    /// <summary>
    /// Verifies that the <c>GetFileExtension</c> extension method returns the expected file extension for a variety of file path and URL inputs.
    /// </summary>
    /// <param name="input">A string representing a file path or URL to extract the extension from.</param>
    /// <param name="result">The expected file extension, including the leading dot (e.g., ".txt"), or an empty string if no extension is present.</param>
    [TestCase("../wtf.js?x=wtf", ".js")]
    [TestCase(".htaccess", ".htaccess")]
    [TestCase("path/to/file/image.png", ".png")]
    [TestCase("c:\\abc\\def\\ghi.jkl", ".jkl")]
    [TestCase("/root/folder.name/file.ext", ".ext")]
    [TestCase("http://www.domain.com/folder/name/file.txt", ".txt")]
    [TestCase("i/don't\\have\\an/extension", "")]
    [TestCase("https://some.where/path/to/file.ext?query=this&more=that", ".ext")]
    [TestCase("double_query.string/file.ext?query=abc?something.else", ".ext")]
    [TestCase("test.tar.gz", ".gz")]
    [TestCase("wierd.file,but._legal", "._legal")]
    [TestCase("one_char.x", ".x")]
    public void Get_File_Extension(string input, string result)
    {
        var extension = input.GetFileExtension();
        Assert.AreEqual(result, extension);
    }

    /// <summary>
    /// Verifies that the <c>CleanForXss</c> extension method properly sanitizes input strings to remove or neutralize potential XSS (Cross-Site Scripting) content.
    /// </summary>
    /// <param name="input">The potentially unsafe input string to be sanitized.</param>
    /// <param name="result">The expected sanitized output string after cleaning.</param>
    [TestCase("'+alert(1234)+'", "+alert1234+")]
    [TestCase("'+alert(56+78)+'", "+alert56+78+")]
    [TestCase("{{file}}", "file")]
    [TestCase("'+alert('hello')+'", "+alerthello+")]
    [TestCase("Test", "Test")]
    public void Clean_From_XSS(string input, string result)
    {
        var cleaned = input.CleanForXss();
        Assert.AreEqual(cleaned, result);
    }

    [TestCase("Hello this is my string", " string", "Hello this is my")]
    [TestCase("Hello this is my string strung", " string", "Hello this is my string strung")]
    [TestCase("Hello this is my string string", " string", "Hello this is my")]
    [TestCase("Hello this is my string string", "g", "Hello this is my string strin")]
    [TestCase("Hello this is my string string", "ello this is my string string", "H")]
    [TestCase("Hello this is my string string", "Hello this is my string string", "")]
    public void TrimEnd(string input, string forTrimming, string shouldBe)
    {
        var trimmed = input.TrimEnd(forTrimming);
        Assert.AreEqual(shouldBe, trimmed);
    }

    /// <summary>
    /// Verifies that the <c>TrimStart</c> extension method correctly removes the specified substring from the start of the input string, if present.
    /// </summary>
    /// <param name="input">The original string to be trimmed.</param>
    /// <param name="forTrimming">The substring to remove from the start of <paramref name="input"/>.</param>
    /// <param name="shouldBe">The expected string result after trimming.</param>
    /// <remarks>
    /// This test checks various cases where the substring to trim is present at the start of the input string.
    /// </remarks>
    [TestCase("Hello this is my string", "hello", " this is my string")]
    [TestCase("Hello this is my string", "Hello this", " is my string")]
    [TestCase("Hello this is my string", "Hello this is my ", "string")]
    [TestCase("Hello this is my string", "Hello this is my string", "")]
    public void TrimStart(string input, string forTrimming, string shouldBe)
    {
        var trimmed = input.TrimStart(forTrimming);
        Assert.AreEqual(shouldBe, trimmed);
    }

    /// <summary>
    /// Verifies that the Replace extension method correctly replaces all occurrences of a specified substring in the input string with a new substring, using the provided <see cref="StringComparison"/> option.
    /// </summary>
    /// <param name="input">The original string on which to perform the replacement.</param>
    /// <param name="oldString">The substring to search for and replace.</param>
    /// <param name="newString">The substring to insert in place of <paramref name="oldString"/>.</param>
    /// <param name="shouldBe">The expected result string after performing the replacement.</param>
    /// <param name="stringComparison">The <see cref="StringComparison"/> option that determines how <paramref name="oldString"/> is matched within <paramref name="input"/>.</param>
    [TestCase(
        "Hello this is my string",
        "hello",
        "replaced",
        "replaced this is my string",
        StringComparison.CurrentCultureIgnoreCase)]
    [TestCase(
        "Hello this is hello my string",
        "hello",
        "replaced",
        "replaced this is replaced my string",
        StringComparison.CurrentCultureIgnoreCase)]
    [TestCase(
        "Hello this is my string",
        "nonexistent",
        "replaced",
        "Hello this is my string",
        StringComparison.CurrentCultureIgnoreCase)]
    [TestCase(
        "Hellohello this is my string",
        "hello",
        "replaced",
        "replacedreplaced this is my string",
        StringComparison.CurrentCultureIgnoreCase)]

    // Ensure replacing with the same string doesn't cause infinite loop.
    [TestCase(
        "Hello this is my string",
        "hello",
        "hello",
        "hello this is my string",
        StringComparison.CurrentCultureIgnoreCase)]
    public void ReplaceWithStringComparison(
        string input,
        string oldString,
        string newString,
        string shouldBe,
        StringComparison stringComparison)
    {
        var replaced = input.Replace(oldString, newString, stringComparison);
        Assert.AreEqual(shouldBe, replaced);
    }

    /// <summary>
    /// Tests the ToFirstUpper extension method to ensure it converts the first character of the string to uppercase.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <param name="expected">The expected result after conversion.</param>
    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase("x", "X")]
    [TestCase("xyzT", "XyzT")]
    [TestCase("XyzT", "XyzT")]
    public void ToFirstUpper(string input, string expected)
    {
        var output = input.ToFirstUpper();
        Assert.AreEqual(expected, output);
    }

    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase("X", "x")]
    [TestCase("XyZ", "xyZ")]
    [TestCase("xyZ", "xyZ")]
    public void ToFirstLower(string input, string expected)
    {
        var output = input.ToFirstLower();
        Assert.AreEqual(expected, output);
    }

    /// <summary>
    /// Unit test for verifying that the <c>ContainsAny</c> string extension method correctly determines whether the specified <paramref name="haystack"/> contains any of the provided <paramref name="needles"/>, using the given <paramref name="comparison"/> type.
    /// </summary>
    /// <param name="haystack">The string to search within.</param>
    /// <param name="needles">The collection of strings to search for in the haystack.</param>
    /// <param name="comparison">The type of string comparison to use when searching.</param>
    /// <param name="expected">The expected result indicating whether any needle is found in the haystack.</param>
    [TestCase("pineapple", new[] { "banana", "apple", "blueberry", "strawberry" }, StringComparison.CurrentCulture, true)]
    [TestCase("PINEAPPLE", new[] { "banana", "apple", "blueberry", "strawberry" }, StringComparison.CurrentCulture, false)]
    [TestCase("pineapple", new[] { "banana", "Apple", "blueberry", "strawberry" }, StringComparison.CurrentCulture, false)]
    [TestCase("pineapple", new[] { "banana", "Apple", "blueberry", "strawberry" }, StringComparison.OrdinalIgnoreCase, true)]
    [TestCase("pineapple", new[] { "banana", "blueberry", "strawberry" }, StringComparison.OrdinalIgnoreCase, false)]
    [TestCase("Strawberry unicorn pie", new[] { "Berry" }, StringComparison.OrdinalIgnoreCase, true)]
    [TestCase("empty pie", new string[0], StringComparison.OrdinalIgnoreCase, false)]
    public void ContainsAny(string haystack, IEnumerable<string> needles, StringComparison comparison, bool expected)
    {
        var output = haystack.ContainsAny(needles, comparison);
        Assert.AreEqual(expected, output);
    }

    /// <summary>
    /// Verifies that the <see cref="StringExtensions.IsNullOrWhiteSpace(string)"/> method correctly determines whether the specified <paramref name="value"/> is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to evaluate.</param>
    /// <param name="expected">True if <paramref name="value"/> is expected to be null, empty, or only white-space; otherwise, false.</param>
    [TestCase("", true)]
    [TestCase(" ", true)]
    [TestCase("\r\n\r\n", true)]
    [TestCase("\r\n", true)]
    [TestCase(
        @"
        Hello
        ",
        false)]
    [TestCase(null, true)]
    [TestCase("a", false)]
    [TestCase("abc", false)]
    [TestCase("abc   ", false)]
    [TestCase("   abc", false)]
    [TestCase("   abc   ", false)]
    public void IsNullOrWhiteSpace(string value, bool expected)
    {
        // Act
        var result = value.IsNullOrWhiteSpace();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests that a string can be correctly URL token encoded and then decoded back to its original value.
    /// </summary>
    /// <param name="value">The input string to encode and decode.</param>
    /// <param name="expected">The expected result of encoding <paramref name="value"/> as a URL token.</param>
    [TestCase("hello", "aGVsbG8")]
    [TestCase("tad", "dGFk")]
    [TestCase("AmqGr+Fd!~ééé", "QW1xR3IrRmQhfsOpw6nDqQ")]
    public void UrlTokenEncoding(string value, string expected)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Console.WriteLine("base64: " + Convert.ToBase64String(bytes));
        var encoded = bytes.UrlTokenEncode();
        Assert.AreEqual(expected, encoded);

        var backBytes = encoded.UrlTokenDecode();
        var backString = Encoding.UTF8.GetString(backBytes);
        Assert.AreEqual(value, backString);
    }

    // FORMAT STRINGS

    // note: here we just ensure that the proper helper gets called properly
    // but the "legacy" tests have moved to the legacy helper tests
    /// <summary>
    /// Tests that the <c>ToUrlSegment</c> extension method calls the provided short string helper correctly
    /// and returns the expected URL alias output.
    /// </summary>
    [Test]
    public void ToUrlAlias()
    {
        var output = "JUST-ANYTHING".ToUrlSegment(_mockShortStringHelper);
        Assert.AreEqual("URL-SEGMENT::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Tests the FormatUrl extension method to ensure it formats the URL segment correctly.
    /// </summary>
    [Test]
    public void FormatUrl()
    {
        var output = "JUST-ANYTHING".ToUrlSegment(_mockShortStringHelper);
        Assert.AreEqual("URL-SEGMENT::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Unit test for the <see cref="ToSafeAlias"/> extension method, verifying that it correctly converts a string to a safe Umbraco alias using the provided short string helper.
    /// Ensures that the output matches the expected safe alias format for a sample input.
    /// </summary>
    [Test]
    public void ToUmbracoAlias()
    {
        var output = "JUST-ANYTHING".ToSafeAlias(_mockShortStringHelper);
        Assert.AreEqual("SAFE-ALIAS::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Unit test for the ToSafeAlias extension method, verifying that it returns the expected safe alias string for a given input.
    /// Ensures that "JUST-ANYTHING" is converted to "SAFE-ALIAS::JUST-ANYTHING".
    /// </summary>
    [Test]
    public void ToSafeAlias()
    {
        var output = "JUST-ANYTHING".ToSafeAlias(_mockShortStringHelper);
        Assert.AreEqual("SAFE-ALIAS::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Verifies that the <c>ToSafeAlias</c> method correctly generates a safe alias when provided with a culture parameter.
    /// Ensures the output matches the expected format for the given input string and culture context.
    /// </summary>
    [Test]
    public void ToSafeAliasWithCulture()
    {
        var output = "JUST-ANYTHING".ToSafeAlias(_mockShortStringHelper, null);
        Assert.AreEqual("SAFE-ALIAS-CULTURE::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Tests that the <c>ToUrlSegment</c> extension method correctly converts a string to its expected URL segment format using a mocked short string helper.
    /// Verifies that the output matches the expected value for a sample input.
    /// </summary>
    [Test]
    public void ToUrlSegment()
    {
        var output = "JUST-ANYTHING".ToUrlSegment(_mockShortStringHelper);
        Assert.AreEqual("URL-SEGMENT::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Verifies that the <c>ToUrlSegment</c> extension method correctly generates a URL segment when provided with a culture parameter.
    /// Ensures the output matches the expected format for culture-specific URL segments.
    /// </summary>
    [Test]
    public void ToUrlSegmentWithCulture()
    {
        var output = "JUST-ANYTHING".ToUrlSegment(_mockShortStringHelper, null);
        Assert.AreEqual("URL-SEGMENT-CULTURE::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Verifies that the <c>ToSafeFileName</c> extension method returns the expected safe file name output for a given input string.
    /// </summary>
    [Test]
    public void ToSafeFileName()
    {
        var output = "JUST-ANYTHING".ToSafeFileName(_mockShortStringHelper);
        Assert.AreEqual("SAFE-FILE-NAME::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Tests that the <c>ToSafeFileName</c> extension method correctly applies culture-specific logic
    /// when generating a safe file name from a string, using a mocked short string helper.
    /// Verifies that the output matches the expected culture-specific safe file name format.
    /// </summary>
    [Test]
    public void ToSafeFileNameWithCulture()
    {
        var output = "JUST-ANYTHING".ToSafeFileName(_mockShortStringHelper, null);
        Assert.AreEqual("SAFE-FILE-NAME-CULTURE::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Verifies that the ConvertCase test correctly checks the ToCleanString extension method with the Unchanged CleanStringType, asserting the expected cleaned string output.
    /// </summary>
    [Test]
    public void ConvertCase()
    {
        var output = "JUST-ANYTHING".ToCleanString(_mockShortStringHelper, CleanStringType.Unchanged);
        Assert.AreEqual("CLEAN-STRING-A::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Tests that the <c>SplitPascalCasing</c> extension method correctly splits a Pascal-cased string using the provided short string helper.
    /// </summary>
    [Test]
    public void SplitPascalCasing()
    {
        var output = "JUST-ANYTHING".SplitPascalCasing(_mockShortStringHelper);
        Assert.AreEqual("SPLIT-PASCAL-CASING::JUST-ANYTHING", output);
    }

    /// <summary>
    /// Verifies that the <c>ReplaceMany</c> extension method correctly replaces multiple substrings in an input string using a provided character replacement map.
    /// This test ensures that special characters and substrings are replaced according to the specified dictionary.
    /// </summary>
    [Test] // can't do cases with an IDictionary
    public void ReplaceManyWithCharMap()
    {
        const string input = "télévisiön tzvâr ßup &nbsp; pof";
        const string expected = "television tzvar ssup   pof";
        IDictionary<string, string> replacements = new Dictionary<string, string>
        {
            { "é", "e" },
            { "ö", "o" },
            { "â", "a" },
            { "ß", "ss" },
            { "&nbsp;", " " },
        };
        var output = input.ReplaceMany(replacements);
        Assert.AreEqual(expected, output);
    }

    [TestCase("val$id!ate|this|str'ing", "$!'", '-', "val-id-ate|this|str-ing")]
    [TestCase("val$id!ate|this|str'ing", "$!'", '*', "val*id*ate|this|str*ing")]
    public void ReplaceManyByOneChar(string input, string toReplace, char replacement, string expected)
    {
        var output = input.ReplaceMany(toReplace.ToArray(), replacement);
        Assert.AreEqual(expected, output);
    }

    /// <summary>
    /// Tests the ReplaceFirst extension method to ensure it correctly replaces the first occurrence of a specified substring.
    /// </summary>
    /// <param name="input">The original string to perform the replacement on.</param>
    /// <param name="search">The substring to find and replace.</param>
    /// <param name="replacement">The string to replace the first occurrence of the search string with.</param>
    /// <param name="expected">The expected result after replacement.</param>
    [TestCase("test to test", "test", "release", "release to test")]
    [TestCase("nothing to do", "test", "release", "nothing to do")]
    public void ReplaceFirst(string input, string search, string replacement, string expected)
    {
        var output = input.ReplaceFirst(search, replacement);
        Assert.AreEqual(expected, output);
    }

    /// <summary>
    /// Unit test for the <c>IsFullPath</c> extension method, verifying that it correctly identifies whether a given string represents a full file path on both Windows and Linux platforms.
    /// <para>
    /// The test covers various scenarios, including:
    /// <list type="bullet">
    /// <item>Valid full paths on Windows (e.g., <c>C:\dir\file.ext</c>, UNC paths)</item>
    /// <item>Valid full paths on Linux (e.g., <c>/some/file</c>)</item>
    /// <item>Strings that are not full paths on either platform</item>
    /// <item>Invalid or empty path strings</item>
    /// </list>
    /// </para>
    /// </summary>
    [Test]
    public void IsFullPath()
    {
        bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        // These are full paths on Windows, but not on Linux
        TryIsFullPath(@"C:\dir\file.ext", isWindows);
        TryIsFullPath(@"C:\dir\", isWindows);
        TryIsFullPath(@"C:\dir", isWindows);
        TryIsFullPath(@"C:\", isWindows);
        TryIsFullPath(@"\\unc\share\dir\file.ext", isWindows);
        TryIsFullPath(@"\\unc\share", isWindows);

        // These are full paths on Linux, but not on Windows
        TryIsFullPath(@"/some/file", !isWindows);
        TryIsFullPath(@"/dir", !isWindows);
        TryIsFullPath(@"/", !isWindows);

        // Not full paths on either Windows or Linux
        TryIsFullPath(@"file.ext", false);
        TryIsFullPath(@"dir\file.ext", false);
        TryIsFullPath(@"\dir\file.ext", false);
        TryIsFullPath(@"C:", false);
        TryIsFullPath(@"C:dir\file.ext", false);
        TryIsFullPath(@"\dir", false); // An "absolute", but not "full" path

        // Invalid on both Windows and Linux
        TryIsFullPath(string.Empty, false, false);
        TryIsFullPath("   ", false, false); // technically, a valid filename on Linux
    }

    /// <summary>
    /// Unit test for the <see cref="StringExtensions.IsEmail"/> extension method, verifying whether the specified string is correctly identified as a valid email address.
    /// </summary>
    /// <param name="email">The input string to evaluate as an email address.</param>
    /// <param name="isEmail">The expected result: <c>true</c> if the input should be recognized as a valid email address; otherwise, <c>false</c>.</param>
    [TestCase("test@test.com", true)]
    [TestCase("test@test", true)]
    [TestCase("testtest.com", false)]
    [TestCase("test@test.dk", true)]
    [TestCase("test@test.se", true)]
    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase(" ", false)]
    public void IsEmail(string? email, bool isEmail) => Assert.AreEqual(isEmail, email.IsEmail());

    private static void TryIsFullPath(string path, bool expectedIsFull, bool expectedIsValid = true)
    {
        Assert.AreEqual(expectedIsFull, path.IsFullPath(), "IsFullPath('" + path + "')");

        if (expectedIsFull)
        {
            Assert.AreEqual(path, Path.GetFullPath(path));
        }
        else if (expectedIsValid)
        {
            Assert.AreNotEqual(path, Path.GetFullPath(path));
        }
    }

    /// <summary>
    /// Verifies that <c>GetIdsFromPathReversed</c> returns a comma-separated string of IDs in reverse order from the input string, skipping any invalid or non-numeric IDs.
    /// </summary>
    /// <param name="input">The input string containing comma-separated IDs, which may include invalid entries.</param>
    /// <param name="expected">The expected output string of valid IDs in reverse order, separated by commas.</param>
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("1,2,3,4,5", "5,4,3,2,1")]
    [TestCase("1,2,x,4,5", "5,4,2,1")]
    public void GetIdsFromPathReversed_ReturnsExpectedResult(string input, string expected)
    {
        var ids = input.GetIdsFromPathReversed();
        Assert.AreEqual(expected, string.Join(",", ids));
    }

    /// <summary>
    /// Unit test for the <see cref="StringExtensions.EnsureCultureCode(string?)"/> extension method, verifying that it returns the expected normalized culture code for a given input.
    /// </summary>
    /// <param name="culture">The input culture code to normalize. Can be null, empty, or a specific culture code string.</param>
    /// <param name="expected">The expected normalized culture code result.</param>
    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase("*", "*")]
    [TestCase("en", "en")]
    [TestCase("EN", "en")]
    [TestCase("en-US", "en-US")]
    [TestCase("en-gb", "en-GB")]
    public void EnsureCultureCode_ReturnsExpectedResult(string? culture, string? expected) => Assert.AreEqual(expected, culture.EnsureCultureCode());

    /// <summary>
    /// Tests that EnsureCultureCode throws a CultureNotFoundException when given an unrecognised culture code.
    /// </summary>
    [Test]
    [Platform(Include = "Win")]
    public void EnsureCultureCode_ThrowsOnUnrecognisedCode() => Assert.Throws<CultureNotFoundException>(() => "xxx-xxx".EnsureCultureCode());
}
