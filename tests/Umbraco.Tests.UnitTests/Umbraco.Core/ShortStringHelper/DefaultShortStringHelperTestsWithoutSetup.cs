// Copyright (c) Umbraco.
// See LICENSE for more details.

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
            EnableDefaultCharReplacements = false,
            ConvertUrlsToAscii = "false",
        };

        const string input = "ÆØÅ and æøå and 中文测试 and  אודות האתר and größer БбДдЖж page";

        var helper =
            new DefaultShortStringHelper(
                new DefaultShortStringHelperConfig().WithDefault(requestHandlerSettings)); // unicode
        var output = helper.CleanStringForUrlSegment(input);
        Assert.That(output, Is.EqualTo("æøå-and-æøå-and-中文测试-and-אודות-האתר-and-größer-ббдджж-page"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(requestHandlerSettings)
            .WithConfig(CleanStringType.UrlSegment, new DefaultShortStringHelperConfig.Config
            {
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_',
                StringType = CleanStringType.LowerCase | CleanStringType.Ascii, // ascii
                Separator = '-',
            }));
        output = helper.CleanStringForUrlSegment(input);
        Assert.That(output, Is.EqualTo("aeoa-and-aeoa-and-and-and-grosser-bbddzhzh-page"));
    }

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
        Assert.That(helper.CleanStringForUrlSegment(input1), Is.EqualTo("æøå-and-æøå-and-中文测试-and-אודות-האתר-and-größer-ббдджж-page"));
        Assert.That(helper.CleanStringForUrlSegment(input2), Is.EqualTo("æøå-and-æøå-and-größer-ббдджж-page"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig().WithDefault(requestHandlerSettings)
            .WithConfig(CleanStringType.UrlSegment, new DefaultShortStringHelperConfig.Config
            {
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_',
                StringType = CleanStringType.LowerCase | CleanStringType.TryAscii, // try ascii
                Separator = '-',
            }));
        Assert.That(helper.CleanStringForUrlSegment(input1), Is.EqualTo("æøå-and-æøå-and-中文测试-and-אודות-האתר-and-größer-ббдджж-page"));
        Assert.That(helper.CleanStringForUrlSegment(input2), Is.EqualTo("aeoa-and-aeoa-and-grosser-bbddzhzh-page"));
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
        Assert.That(helper.CleanString("foo_bar nil", CleanStringType.Alias), Is.EqualTo("foo_bar*nil"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                // underscore is not accepted within terms
                IsTerm = (c, leading) => char.IsLetterOrDigit(c),
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                Separator = '*',
            }));
        Assert.That(helper.CleanString("foo_bar nil", CleanStringType.Alias), Is.EqualTo("foo*bar*nil"));
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
        Assert.That(helper.CleanString("0123foo_bar 543 nil 321", CleanStringType.Alias), Is.EqualTo("0123foo*bar*543*nil*321"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                // only letters are valid leading chars
                IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c),
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                Separator = '*',
            }));
        Assert.That(helper.CleanString("0123foo_bar 543 nil 321", CleanStringType.Alias), Is.EqualTo("foo*bar*543*nil*321"));
        Assert.That(helper.CleanString("0123 foo_bar 543 nil 321", CleanStringType.Alias), Is.EqualTo("foo*bar*543*nil*321"));

        helper = new DefaultShortStringHelper(
            new DefaultShortStringHelperConfig().WithDefault(new RequestHandlerSettings()));
        Assert.That(helper.CleanStringForSafeAlias("1child2"), Is.EqualTo("child2"));
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
        Assert.That(helper.CleanString("fooBar", CleanStringType.Alias), Is.EqualTo("foo*Bar"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,

                // uppercase letter is part of term
                BreakTermsOnUpper = false,
                Separator = '*',
            }));
        Assert.That(helper.CleanString("fooBar", CleanStringType.Alias), Is.EqualTo("fooBar"));
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
        Assert.That(helper.CleanString("foo BARRnil", CleanStringType.Alias), Is.EqualTo("foo*BAR*Rnil"));
        Assert.That(helper.CleanString("foo BARnil", CleanStringType.Alias), Is.EqualTo("foo*BA*Rnil"));
        Assert.That(helper.CleanString("foo BAnil", CleanStringType.Alias), Is.EqualTo("foo*BAnil"));
        Assert.That(helper.CleanString("foo Bnil", CleanStringType.Alias), Is.EqualTo("foo*Bnil"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.Alias, new DefaultShortStringHelperConfig.Config
            {
                StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,

                // non-uppercase letter means word
                CutAcronymOnNonUpper = false,
                Separator = '*',
            }));
        Assert.That(helper.CleanString("foo BARRnil", CleanStringType.Alias), Is.EqualTo("foo*BARRnil"));
        Assert.That(helper.CleanString("foo BARnil", CleanStringType.Alias), Is.EqualTo("foo*BARnil"));
        Assert.That(helper.CleanString("foo BAnil", CleanStringType.Alias), Is.EqualTo("foo*BAnil"));
        Assert.That(helper.CleanString("foo Bnil", CleanStringType.Alias), Is.EqualTo("foo*Bnil"));
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
        Assert.That(helper.CleanString("foo BARRnil", CleanStringType.Alias), Is.EqualTo("foo*BARR*nil"));
        Assert.That(helper.CleanString("foo BARnil", CleanStringType.Alias), Is.EqualTo("foo*BAR*nil"));
        Assert.That(helper.CleanString("foo BAnil", CleanStringType.Alias), Is.EqualTo("foo*BA*nil"));
        Assert.That(helper.CleanString("foo Bnil", CleanStringType.Alias), Is.EqualTo("foo*Bnil"));

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
        Assert.That(helper.CleanString("foo BARRnil", CleanStringType.Alias), Is.EqualTo("foo*BAR*Rnil"));
        Assert.That(helper.CleanString("foo BARnil", CleanStringType.Alias), Is.EqualTo("foo*BA*Rnil"));
        Assert.That(helper.CleanString("foo BAnil", CleanStringType.Alias), Is.EqualTo("foo*BAnil"));
        Assert.That(helper.CleanString("foo Bnil", CleanStringType.Alias), Is.EqualTo("foo*Bnil"));
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
        Assert.That(helper.CleanString("   foo   ", CleanStringType.Alias), Is.EqualTo("foo"));
        Assert.That(helper.CleanString("   foo   bar   ", CleanStringType.Alias), Is.EqualTo("foo*bar"));
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
        Assert.That(helper.CleanString("foo bar", CleanStringType.Alias), Is.EqualTo("foo*bar"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = ' ',
                }));
        Assert.That(helper.CleanString("foo bar", CleanStringType.Alias), Is.EqualTo("foo bar"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                }));
        Assert.That(helper.CleanString("foo bar", CleanStringType.Alias), Is.EqualTo("foobar"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '文',
                }));
        Assert.That(helper.CleanString("foo bar", CleanStringType.Alias), Is.EqualTo("foo文bar"));
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
        Assert.That(helper.CleanString("house (2)", CleanStringType.Alias), Is.EqualTo("house*2"));

        // TODO: but for a filename we want to keep them!
        // TODO: and what about a URL?
    }

    [Test]
    public void Utf8Surrogates()
    {
        // Unicode values between 0x10000 and 0x10FFF are represented by two 16-bit "surrogate" characters
        const string str = "a\U00010F00z\uA74Ft";
        Assert.That(str.Length, Is.EqualTo(6));
        Assert.That(char.IsSurrogate(str[1]), Is.True);
        Assert.That(char.IsHighSurrogate(str[1]), Is.True);
        Assert.That(char.IsSurrogate(str[2]), Is.True);
        Assert.That(char.IsLowSurrogate(str[2]), Is.True);
        Assert.That(str[3], Is.EqualTo('z'));
        Assert.That(char.IsSurrogate(str[4]), Is.False);
        Assert.That(str[4], Is.EqualTo('\uA74F'));
        Assert.That(str[5], Is.EqualTo('t'));

        Assert.That(str.Substring(3, 1), Is.EqualTo("z"));
        Assert.That(str.Substring(1, 2), Is.EqualTo("\U00010F00"));

        var bytes = Encoding.UTF8.GetBytes(str);
        Assert.That(bytes.Length, Is.EqualTo(10));
        Assert.That(bytes[0], Is.EqualTo('a'));

        // Then next string element is two chars (surrogate pair) or 4 bytes, 21 bits of code point.
        Assert.That(bytes[5], Is.EqualTo('z'));

        // Then next string element is one char and 3 bytes, 16 bits of code point.
        Assert.That(bytes[9], Is.EqualTo('t'));

        //// foreach (var b in bytes)
        ////    Debug.Print("{0:X}", b);

        Debug.Print("\U00010B70");
    }

    [Test]
    public void Utf8ToAsciiConverter()
    {
        const string str = "a\U00010F00z\uA74Ftéô";
        var output = global::Umbraco.Cms.Core.Strings.Utf8ToAsciiConverter.ToAsciiString(str);
        Assert.That(output, Is.EqualTo("a?zooteo"));
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
        Assert.That(helper.CleanString("中文测试", CleanStringType.Alias), Is.EqualTo("中文测试"));
        Assert.That(helper.CleanString("léger 中文测试 ZÔRG", CleanStringType.Alias), Is.EqualTo("léger*中文测试*ZÔRG"));

        helper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    StringType = CleanStringType.Ascii | CleanStringType.Unchanged,
                    Separator = '*',
                }));
        Assert.That(helper.CleanString("中文测试", CleanStringType.Alias), Is.EqualTo(string.Empty));
        Assert.That(helper.CleanString("léger 中文测试 ZÔRG", CleanStringType.Alias), Is.EqualTo("leger*ZORG"));
    }

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
        Assert.That(alias, Is.EqualTo("legerZORG2AX"), "alias");

        // lower-cased, utf8 filename, removing illegal filename chars, using dash-separator
        Assert.That(filename, Is.EqualTo("0123-中文测试-中文测试-léger-zôrg-2-a-x"), "filename");

        // lower-cased, utf8 URL segment, only letters and digits, using dash-separator
        Assert.That(segment, Is.EqualTo("0123-中文测试-中文测试-léger-zôrg-2-a-x"), "segment");
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

        // TODO: "C" can't be an acronym
        // TODO: "DBXreview" = acronym?!
        Assert.That(
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias), Is.EqualTo("aaa BBB CCc Ddd E FF")); // unchanged
        Assert.That(
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.CamelCase), Is.EqualTo("aaa Bbb Ccc Ddd E FF"));
        Assert.That(helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.PascalCase), Is.EqualTo("Aaa Bbb Ccc Ddd E FF"));
        Assert.That(
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.LowerCase), Is.EqualTo("aaa bbb ccc ddd e ff"));
        Assert.That(
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.UpperCase), Is.EqualTo("AAA BBB CCC DDD E FF"));
        Assert.That(
            helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.UmbracoCase), Is.EqualTo("aaa BBB CCc Ddd E FF"));

        // MS rules & guidelines:
        // - Do capitalize both characters of two-character acronyms, except the first word of a camel-cased identifier.
        //     eg "DBRate" (pascal) or "ioHelper" (camel) - "SpecialDBRate" (pascal) or "specialIOHelper" (camel)
        // - Do capitalize only the first character of acronyms with three or more characters, except the first word of a camel-cased identifier.
        //     eg "XmlWriter (pascal) or "htmlReader" (camel) - "SpecialXmlWriter" (pascal) or "specialHtmlReader" (camel)
        // - Do not capitalize any of the characters of any acronyms, whatever their length, at the beginning of a camel-cased identifier.
        //     eg "xmlWriter" or "dbWriter" (camel)
        Assert.That(
            helper.CleanString("aaa BB ccc", CleanStringType.Alias | CleanStringType.CamelCase), Is.EqualTo("aaa BB Ccc"));
        Assert.That(
            helper.CleanString("AA bb ccc", CleanStringType.Alias | CleanStringType.CamelCase), Is.EqualTo("aa Bb Ccc"));
        Assert.That(
            helper.CleanString("AAA bb ccc", CleanStringType.Alias | CleanStringType.CamelCase), Is.EqualTo("aaa Bb Ccc"));
        Assert.That(helper.CleanString("DB rate", CleanStringType.Alias | CleanStringType.CamelCase), Is.EqualTo("db Rate"));
        Assert.That(
            helper.CleanString("special DB rate", CleanStringType.Alias | CleanStringType.CamelCase), Is.EqualTo("special DB Rate"));
        Assert.That(
            helper.CleanString("XML writer", CleanStringType.Alias | CleanStringType.CamelCase), Is.EqualTo("xml Writer"));
        Assert.That(
            helper.CleanString("special XML writer", CleanStringType.Alias | CleanStringType.CamelCase), Is.EqualTo("special Xml Writer"));

        Assert.That(
            helper.CleanString("aaa BB ccc", CleanStringType.Alias | CleanStringType.PascalCase), Is.EqualTo("Aaa BB Ccc"));
        Assert.That(
            helper.CleanString("AA bb ccc", CleanStringType.Alias | CleanStringType.PascalCase), Is.EqualTo("AA Bb Ccc"));
        Assert.That(
            helper.CleanString("AAA bb ccc", CleanStringType.Alias | CleanStringType.PascalCase), Is.EqualTo("Aaa Bb Ccc"));
        Assert.That(helper.CleanString("DB rate", CleanStringType.Alias | CleanStringType.PascalCase), Is.EqualTo("DB Rate"));
        Assert.That(
            helper.CleanString("special DB rate", CleanStringType.Alias | CleanStringType.PascalCase), Is.EqualTo("Special DB Rate"));
        Assert.That(
            helper.CleanString("XML writer", CleanStringType.Alias | CleanStringType.PascalCase), Is.EqualTo("Xml Writer"));
        Assert.That(
            helper.CleanString("special XML writer", CleanStringType.Alias | CleanStringType.PascalCase), Is.EqualTo("Special Xml Writer"));
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
