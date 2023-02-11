// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

[TestFixture]
public class DefaultShortStringHelperTestsWithoutSetup
{
    [Test]
    public void U4_4056()
    {
        var requestHandlerSettings = new RequestHandlerSettings
        {
            CharCollection = Array.Empty<CharItem>(),
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

    [Test]
    public void U4_4056_TryAscii()
    {
        var requestHandlerSettings = new RequestHandlerSettings
        {
            CharCollection = Array.Empty<CharItem>(),
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

        // FIXME: but for a filename we want to keep them!
        // FIXME: and what about a URL?
    }

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

    [Test]
    public void Utf8ToAsciiConverter()
    {
        const string str = "a\U00010F00z\uA74Ftéô";
        var output = global::Umbraco.Cms.Core.Strings.Utf8ToAsciiConverter.ToAsciiString(str);
        Assert.AreEqual("a?zooteo", output);
    }

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

    [Test]
    public void CleanStringDefaultConfig()
    {
        var requestHandlerSettings = new RequestHandlerSettings
        {
            CharCollection = Array.Empty<CharItem>(),
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

        // FIXME: "C" can't be an acronym
        // FIXME: "DBXreview" = acronym?!
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
