// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

/// <summary>
/// Provides unit tests for the <see cref="DefaultShortStringHelper"/> class that do not require setup.
/// </summary>
[TestFixture]
public class DefaultShortStringHelperTestsWithoutSetup
{
    /// <summary>
    /// Verifies that <see cref="DefaultShortStringHelper"/> correctly processes Unicode and ASCII characters
    /// in URL segments, depending on configuration settings for character replacement and ASCII conversion.
    /// Ensures that Unicode characters are preserved or converted as expected based on the configuration.
    /// </summary>
    [Test]
    public void U4_4056()
    {
        var requestHandlerSettings = new RequestHandlerSettings
        {
            EnableDefaultCharReplacements = false,
            ConvertUrlsToAscii = "false",
        };

        const string input = "ÆØÅ and æøå and 中文测试 and  אודות האתר and größer БбДдЖж page";

        var helper =
            new DefaultShortStringHelper(
                new DefaultShortStringHelperConfig().WithDefault(requestHandlerSettings)); // unicode
        var output = helper.CleanStringForUrlSegment(input);
        Assert.AreEqual("æøå-and-æøå-and-中文测试-and-אודות-האתר-and-größer-ббдджж-page", output);

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(requestHandlerSettings)
            .WithConfig(CleanStringType.UrlSegment, new DefaultShortStringHelperConfig.Config
            {
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_',
                StringType = CleanStringType.LowerCase | CleanStringType.Ascii, // ascii
                Separator = '-',
            }));
        output = helper.CleanStringForUrlSegment(input);
        Assert.AreEqual("aeoa-and-aeoa-and-and-and-grosser-bbddzhzh-page", output);
    }

    /// <summary>
    /// Verifies that the <c>TryAscii</c> option of <see cref="DefaultShortStringHelper"/> correctly converts applicable characters to their ASCII equivalents when enabled, and preserves Unicode characters when not enabled.
    /// </summary>
    [Test]
    public void U4_4056_TryAscii()
    {
        var requestHandlerSettings = new RequestHandlerSettings
        {
            EnableDefaultCharReplacements = false,
            ConvertUrlsToAscii = "false",
        };

        const string input1 = "ÆØÅ and æøå and 中文测试 and  אודות האתר and größer БбДдЖж page";
        const string input2 = "ÆØÅ and æøå and größer БбДдЖж page";

        var helper =
            new DefaultShortStringHelper(
                new DefaultShortStringHelperConfig().WithDefault(requestHandlerSettings)); // unicode
        Assert.AreEqual("æøå-and-æøå-and-中文测试-and-אודות-האתר-and-größer-ббдджж-page", helper.CleanStringForUrlSegment(input1));
        Assert.AreEqual("æøå-and-æøå-and-größer-ббдджж-page", helper.CleanStringForUrlSegment(input2));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(requestHandlerSettings)
            .WithConfig(CleanStringType.UrlSegment, new DefaultShortStringHelperConfig.Config
            {
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_',
                StringType = CleanStringType.LowerCase | CleanStringType.TryAscii, // try ascii
                Separator = '-',
            }));
        Assert.AreEqual("æøå-and-æøå-and-中文测试-and-אודות-האתר-and-größer-ббдджж-page", helper.CleanStringForUrlSegment(input1));
        Assert.AreEqual("aeoa-and-aeoa-and-grosser-bbddzhzh-page", helper.CleanStringForUrlSegment(input2));
    }

    /// <summary>
    /// Verifies that the <c>CleanString</c> method correctly handles underscores within terms
    /// depending on the configuration: when underscores are allowed, they are preserved within terms;
    /// when not allowed, underscores are treated as separators.
    /// </summary>
    [Test]
    public void CleanStringUnderscoreInTerm()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                // underscore is accepted within terms
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_',
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                Separator = '*',
            }));
        Assert.AreEqual("foo_bar*nil", helper.CleanString("foo_bar nil", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                // underscore is not accepted within terms
                IsTerm = (c, leading) => char.IsLetterOrDigit(c),
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                Separator = '*',
            }));
        Assert.AreEqual("foo*bar*nil", helper.CleanString("foo_bar nil", CleanStringType.Alias));
    }

    /// <summary>
    /// Tests the behavior of <see cref="DefaultShortStringHelper.CleanString"/> when cleaning strings with different rules for valid leading characters.
    /// Verifies that the helper correctly handles cases where only letters or letters and digits are allowed as leading characters, and ensures that the output matches expected cleaned strings.
    /// Also tests <see cref="DefaultShortStringHelper.CleanStringForSafeAlias"/> for removing invalid leading characters.
    /// </summary>
    [Test]
    public void CleanStringLeadingChars()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                // letters and digits are valid leading chars
                IsTerm = (c, leading) => char.IsLetterOrDigit(c),
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                Separator = '*',
            }));
        Assert.AreEqual("0123foo*bar*543*nil*321", helper.CleanString("0123foo_bar 543 nil 321", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                // only letters are valid leading chars
                IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c),
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                Separator = '*',
            }));
        Assert.AreEqual("foo*bar*543*nil*321", helper.CleanString("0123foo_bar 543 nil 321", CleanStringType.Alias));
        Assert.AreEqual("foo*bar*543*nil*321", helper.CleanString("0123 foo_bar 543 nil 321", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(
            new DefaultShortStringHelperConfig().WithDefault(new RequestHandlerSettings()));
        Assert.AreEqual("child2", helper.CleanStringForSafeAlias("1child2"));
    }

    /// <summary>
    /// Tests the CleanString method behavior when breaking terms on uppercase letters.
    /// It verifies that the method correctly inserts separators when BreakTermsOnUpper is true,
    /// and does not insert separators when BreakTermsOnUpper is false.
    /// </summary>
    [Test]
    public void CleanStringTermOnUpper()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,

                // uppercase letter means new term
                BreakTermsOnUpper = true,
                Separator = '*',
            }));
        Assert.AreEqual("foo*Bar", helper.CleanString("fooBar", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,

                // uppercase letter is part of term
                BreakTermsOnUpper = false,
                Separator = '*',
            }));
        Assert.AreEqual("fooBar", helper.CleanString("fooBar", CleanStringType.Alias));
    }

    /// <summary>
    /// Tests the behavior of CleanString when handling acronyms with non-uppercase letters.
    /// Verifies the effect of the CutAcronymOnNonUpper setting on string cleaning.
    /// </summary>
    [Test]
    public void CleanStringAcronymOnNonUpper()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,

                // non-uppercase letter means cut acronym
                CutAcronymOnNonUpper = true,
                Separator = '*',
            }));
        Assert.AreEqual("foo*BAR*Rnil", helper.CleanString("foo BARRnil", CleanStringType.Alias));
        Assert.AreEqual("foo*BA*Rnil", helper.CleanString("foo BARnil", CleanStringType.Alias));
        Assert.AreEqual("foo*BAnil", helper.CleanString("foo BAnil", CleanStringType.Alias));
        Assert.AreEqual("foo*Bnil", helper.CleanString("foo Bnil", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,

                // non-uppercase letter means word
                CutAcronymOnNonUpper = false,
                Separator = '*',
            }));
        Assert.AreEqual("foo*BARRnil", helper.CleanString("foo BARRnil", CleanStringType.Alias));
        Assert.AreEqual("foo*BARnil", helper.CleanString("foo BARnil", CleanStringType.Alias));
        Assert.AreEqual("foo*BAnil", helper.CleanString("foo BAnil", CleanStringType.Alias));
        Assert.AreEqual("foo*Bnil", helper.CleanString("foo Bnil", CleanStringType.Alias));
    }

    /// <summary>
    /// Tests the CleanString method of DefaultShortStringHelper with greedy acronyms enabled and disabled.
    /// Verifies that acronyms are cut correctly based on the GreedyAcronyms setting.
    /// </summary>
    [Test]
    public void CleanStringGreedyAcronyms()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    CutAcronymOnNonUpper = true,
                    GreedyAcronyms = true,
                    Separator = '*',
                }));
        Assert.AreEqual("foo*BARR*nil", helper.CleanString("foo BARRnil", CleanStringType.Alias));
        Assert.AreEqual("foo*BAR*nil", helper.CleanString("foo BARnil", CleanStringType.Alias));
        Assert.AreEqual("foo*BA*nil", helper.CleanString("foo BAnil", CleanStringType.Alias));
        Assert.AreEqual("foo*Bnil", helper.CleanString("foo Bnil", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    CutAcronymOnNonUpper = true,
                    GreedyAcronyms = false,
                    Separator = '*',
                }));
        Assert.AreEqual("foo*BAR*Rnil", helper.CleanString("foo BARRnil", CleanStringType.Alias));
        Assert.AreEqual("foo*BA*Rnil", helper.CleanString("foo BARnil", CleanStringType.Alias));
        Assert.AreEqual("foo*BAnil", helper.CleanString("foo BAnil", CleanStringType.Alias));
        Assert.AreEqual("foo*Bnil", helper.CleanString("foo Bnil", CleanStringType.Alias));
    }

    /// <summary>
    /// Tests that the CleanString method correctly trims and replaces whitespace characters
    /// according to the specified configuration and separator.
    /// </summary>
    [Test]
    public void CleanStringWhiteSpace()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*',
                }));
        Assert.AreEqual("foo", helper.CleanString("   foo   ", CleanStringType.Alias));
        Assert.AreEqual("foo*bar", helper.CleanString("   foo   bar   ", CleanStringType.Alias));
    }

    /// <summary>
    /// Verifies that the <see cref="DefaultShortStringHelper"/> correctly cleans strings by replacing spaces with different separator characters, including '*', ' ', no separator, and a Unicode character.
    /// Ensures that the separator configuration is respected when cleaning strings of type <see cref="CleanStringType.Alias"/>.
    /// </summary>
    [Test]
    public void CleanStringSeparator()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*',
                }));
        Assert.AreEqual("foo*bar", helper.CleanString("foo bar", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = ' ',
                }));
        Assert.AreEqual("foo bar", helper.CleanString("foo bar", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                }));
        Assert.AreEqual("foobar", helper.CleanString("foo bar", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '文',
                }));
        Assert.AreEqual("foo文bar", helper.CleanString("foo bar", CleanStringType.Alias));
    }

    /// <summary>
    /// Tests that the <see cref="DefaultShortStringHelper.CleanString"/> method correctly replaces symbol characters in a string
    /// according to the provided configuration, ensuring that symbols such as parentheses are removed and replaced with the configured separator.
    /// Verifies that "house (2)" is cleaned to "house*2" when using an asterisk as the separator for alias cleaning.
    /// </summary>
    [Test]
    public void CleanStringSymbols()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*',
                }));
        Assert.AreEqual("house*2", helper.CleanString("house (2)", CleanStringType.Alias));

        // TODO: but for a filename we want to keep them!
        // TODO: and what about a URL?
    }

    /// <summary>
    /// Tests the handling of Unicode surrogate pairs and supplementary characters in strings,
    /// ensuring correct identification and encoding of surrogate pairs and their representation
    /// in UTF-8 byte arrays.
    /// </summary>
    [Test]
    public void Utf8Surrogates()
    {
        // Unicode values between 0x10000 and 0x10FFF are represented by two 16-bit "surrogate" characters
        const string str = "a\U00010F00z\uA74Ft";
        Assert.AreEqual(6, str.Length);
        Assert.IsTrue(char.IsSurrogate(str[1]));
        Assert.IsTrue(char.IsHighSurrogate(str[1]));
        Assert.IsTrue(char.IsSurrogate(str[2]));
        Assert.IsTrue(char.IsLowSurrogate(str[2]));
        Assert.AreEqual('z', str[3]);
        Assert.IsFalse(char.IsSurrogate(str[4]));
        Assert.AreEqual('\uA74F', str[4]);
        Assert.AreEqual('t', str[5]);

        Assert.AreEqual("z", str.Substring(3, 1));
        Assert.AreEqual("\U00010F00", str.Substring(1, 2));

        var bytes = Encoding.UTF8.GetBytes(str);
        Assert.AreEqual(10, bytes.Length);
        Assert.AreEqual('a', bytes[0]);

        // Then next string element is two chars (surrogate pair) or 4 bytes, 21 bits of code point.
        Assert.AreEqual('z', bytes[5]);

        // Then next string element is one char and 3 bytes, 16 bits of code point.
        Assert.AreEqual('t', bytes[9]);

        //// foreach (var b in bytes)
        ////    Debug.Print("{0:X}", b);

        Debug.Print("\U00010B70");
    }

    /// <summary>
    /// Tests that <see cref="Umbraco.Cms.Core.Strings.Utf8ToAsciiConverter.ToAsciiString"/> correctly converts a UTF-8 string containing non-ASCII characters to an ASCII-only string, replacing or omitting unsupported characters as expected.
    /// </summary>
    [Test]
    public void Utf8ToAsciiConverter()
    {
        const string str = "a\U00010F00z\uA74Ftéô";
        var output = global::Umbraco.Cms.Core.Strings.Utf8ToAsciiConverter.ToAsciiString(str);
        Assert.AreEqual("a?zooteo", output);
    }

    /// <summary>
    /// Tests the CleanString method of DefaultShortStringHelper with different encoding configurations.
    /// </summary>
    [Test]
    public void CleanStringEncoding()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*',
                }));
        Assert.AreEqual("中文测试", helper.CleanString("中文测试", CleanStringType.Alias));
        Assert.AreEqual("léger*中文测试*ZÔRG", helper.CleanString("léger 中文测试 ZÔRG", CleanStringType.Alias));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Ascii | CleanStringType.Unchanged,
                    Separator = '*',
                }));
        Assert.AreEqual(string.Empty, helper.CleanString("中文测试", CleanStringType.Alias));
        Assert.AreEqual("leger*ZORG", helper.CleanString("léger 中文测试 ZÔRG", CleanStringType.Alias));
    }

    /// <summary>
    /// Verifies that the <see cref="DefaultShortStringHelper"/> cleans strings correctly using its default configuration.
    /// This test checks the output of <c>CleanStringForSafeAlias</c>, <c>CleanStringForSafeFileName</c>, and <c>CleanStringForUrlSegment</c>
    /// methods for a sample input containing ASCII, Unicode, and special characters.
    /// It asserts that:
    /// <list type="bullet">
    /// <item>The alias is converted to a valid, ASCII-only, camel-cased string.</item>
    /// <item>The file name and URL segment are lower-cased, UTF-8, and use dash separators, removing illegal characters.</item>
    /// </list>
    /// </summary>
    [Test]
    public void CleanStringDefaultConfig()
    {
        var requestHandlerSettings = new RequestHandlerSettings
        {
            EnableDefaultCharReplacements = false,
            ConvertUrlsToAscii = "false",
        };

        var helper =
            new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(requestHandlerSettings));

        const string input = "0123 中文测试 中文测试 léger ZÔRG (2) a?? *x";

        var alias = helper.CleanStringForSafeAlias(input);
        var filename = helper.CleanStringForSafeFileName(input);
        var segment = helper.CleanStringForUrlSegment(input);

        // umbraco-cased ascii alias, must begin with a proper letter
        Assert.AreEqual("legerZORG2AX", alias, "alias");

        // lower-cased, utf8 filename, removing illegal filename chars, using dash-separator
        Assert.AreEqual("0123-中文测试-中文测试-léger-zôrg-2-a-x", filename, "filename");

        // lower-cased, utf8 URL segment, only letters and digits, using dash-separator
        Assert.AreEqual("0123-中文测试-中文测试-léger-zôrg-2-a-x", segment, "segment");
    }

    /// <summary>
    /// Tests the <c>CleanString</c> method of <see cref="DefaultShortStringHelper"/> with various casing options.
    /// Verifies that acronyms and words are cased correctly according to Microsoft naming conventions for camel case and Pascal case.
    /// Ensures that two-letter and multi-letter acronyms, as well as regular words, are handled as expected for each casing type.
    /// </summary>
    [Test]
    public void CleanStringCasing()
    {
        var helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = ' ',
                }));

        // BBB is an acronym
        // E is a word (too short to be an acronym)
        // FF is an acronym

        // TODO: "C" can't be an acronym
        // TODO: "DBXreview" = acronym?!
        Assert.AreEqual(
            "aaa BBB CCc Ddd E FF",
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias)); // unchanged
        Assert.AreEqual(
            "aaa Bbb Ccc Ddd E FF",
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.CamelCase));
        Assert.AreEqual("Aaa Bbb Ccc Ddd E FF", helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.PascalCase));
        Assert.AreEqual(
            "aaa bbb ccc ddd e ff",
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.LowerCase));
        Assert.AreEqual(
            "AAA BBB CCC DDD E FF",
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.UpperCase));
        Assert.AreEqual(
            "aaa BBB CCc Ddd E FF",
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.UmbracoCase));

        // MS rules & guidelines:
        // - Do capitalize both characters of two-character acronyms, except the first word of a camel-cased identifier.
        //     eg "DBRate" (pascal) or "ioHelper" (camel) - "SpecialDBRate" (pascal) or "specialIOHelper" (camel)
        // - Do capitalize only the first character of acronyms with three or more characters, except the first word of a camel-cased identifier.
        //     eg "XmlWriter (pascal) or "htmlReader" (camel) - "SpecialXmlWriter" (pascal) or "specialHtmlReader" (camel)
        // - Do not capitalize any of the characters of any acronyms, whatever their length, at the beginning of a camel-cased identifier.
        //     eg "xmlWriter" or "dbWriter" (camel)
        Assert.AreEqual(
            "aaa BB Ccc",
            helper.CleanString("aaa BB ccc", CleanStringType.Alias | CleanStringType.CamelCase));
        Assert.AreEqual(
            "aa Bb Ccc",
            helper.CleanString("AA bb ccc", CleanStringType.Alias | CleanStringType.CamelCase));
        Assert.AreEqual(
            "aaa Bb Ccc",
            helper.CleanString("AAA bb ccc", CleanStringType.Alias | CleanStringType.CamelCase));
        Assert.AreEqual("db Rate", helper.CleanString("DB rate", CleanStringType.Alias | CleanStringType.CamelCase));
        Assert.AreEqual(
            "special DB Rate",
            helper.CleanString("special DB rate", CleanStringType.Alias | CleanStringType.CamelCase));
        Assert.AreEqual(
            "xml Writer",
            helper.CleanString("XML writer", CleanStringType.Alias | CleanStringType.CamelCase));
        Assert.AreEqual(
            "special Xml Writer",
            helper.CleanString("special XML writer", CleanStringType.Alias | CleanStringType.CamelCase));

        Assert.AreEqual(
            "Aaa BB Ccc",
            helper.CleanString("aaa BB ccc", CleanStringType.Alias | CleanStringType.PascalCase));
        Assert.AreEqual(
            "AA Bb Ccc",
            helper.CleanString("AA bb ccc", CleanStringType.Alias | CleanStringType.PascalCase));
        Assert.AreEqual(
            "Aaa Bb Ccc",
            helper.CleanString("AAA bb ccc", CleanStringType.Alias | CleanStringType.PascalCase));
        Assert.AreEqual("DB Rate", helper.CleanString("DB rate", CleanStringType.Alias | CleanStringType.PascalCase));
        Assert.AreEqual(
            "Special DB Rate",
            helper.CleanString("special DB rate", CleanStringType.Alias | CleanStringType.PascalCase));
        Assert.AreEqual(
            "Xml Writer",
            helper.CleanString("XML writer", CleanStringType.Alias | CleanStringType.PascalCase));
        Assert.AreEqual(
            "Special Xml Writer",
            helper.CleanString("special XML writer", CleanStringType.Alias | CleanStringType.PascalCase));
    }

    // #region Cases
    // [TestCase("This is my_little_house so cute.", "thisIsMyLittleHouseSoCute", false)]
    // [TestCase("This is my_little_house so cute.", "thisIsMy_little_houseSoCute", true)]
    // [TestCase("This is my_Little_House so cute.", "thisIsMyLittleHouseSoCute", false)]
    // [TestCase("This is my_Little_House so cute.", "thisIsMy_Little_HouseSoCute", true)]
    // [TestCase("An UPPER_CASE_TEST to check", "anUpperCaseTestToCheck", false)]
    // [TestCase("An UPPER_CASE_TEST to check", "anUpper_case_testToCheck", true)]
    // [TestCase("Trailing_", "trailing", false)]
    // [TestCase("Trailing_", "trailing_", true)]
    // [TestCase("_Leading", "leading", false)]
    // [TestCase("_Leading", "leading", true)]
    // [TestCase("Repeat___Repeat", "repeatRepeat", false)]
    // [TestCase("Repeat___Repeat", "repeat___Repeat", true)]
    // [TestCase("Repeat___repeat", "repeatRepeat", false)]
    // [TestCase("Repeat___repeat", "repeat___repeat", true)]
    // #endregion
    // public void CleanStringWithUnderscore(string input, string expected, bool allowUnderscoreInTerm)
    // {
    //     var helper = new DefaultShortStringHelper(SettingsForTests.GetDefault())
    //         .WithConfig(allowUnderscoreInTerm: allowUnderscoreInTerm);
    //     var output = helper.CleanString(input, CleanStringType.Alias | CleanStringType.Ascii | CleanStringType.CamelCase);
    //     Assert.AreEqual(expected, output);
    // }
}
