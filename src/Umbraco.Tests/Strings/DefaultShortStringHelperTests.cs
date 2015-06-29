using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Strings
{
    [TestFixture]
    public class DefaultShortStringHelperTests : BaseUmbracoConfigurationTest
    {
        private DefaultShortStringHelper _helper;

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            // NOTE: it is not possible to configure the helper once it has been assigned
            // to the resolver and resolution has frozen. but, obviously, it is possible
            // to configure filters and then to alter these filters after resolution has
            // frozen. beware, nothing is thread-safe in-there!

            // NOTE pre-filters runs _before_ Recode takes place
            // so there still may be utf8 chars even though you want ascii

            _helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.FileName, new DefaultShortStringHelper.Config
                {
                    //PreFilter = ClearFileChars, // done in IsTerm
                    IsTerm = (c, leading) => (char.IsLetterOrDigit(c) || c == '_') && DefaultShortStringHelper.IsValidFileNameChar(c),
                    StringType = CleanStringType.LowerCase | CleanStringType.Ascii,
                    Separator = '-'
                })
                .WithConfig(CleanStringType.UrlSegment, new DefaultShortStringHelper.Config
                {
                    PreFilter = StripQuotes,
                    IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_',
                    StringType = CleanStringType.LowerCase | CleanStringType.Ascii,
                    Separator = '-'
                })
                .WithConfig(new CultureInfo("fr-FR"), CleanStringType.UrlSegment, new DefaultShortStringHelper.Config
                {
                    PreFilter = FilterFrenchElisions,
                    IsTerm = (c, leading) => leading ? char.IsLetter(c) : (char.IsLetterOrDigit(c) || c == '_'),
                    StringType = CleanStringType.LowerCase | CleanStringType.Ascii,
                    Separator = '-'
                })
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    PreFilter = StripQuotes,
                    IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c),
                    StringType = CleanStringType.UmbracoCase | CleanStringType.Ascii
                })
                .WithConfig(new CultureInfo("fr-FR"), CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    PreFilter = WhiteQuotes,
                    IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c),
                    StringType = CleanStringType.UmbracoCase | CleanStringType.Ascii
                })
                .WithConfig(CleanStringType.ConvertCase, new DefaultShortStringHelper.Config
                {
                    PreFilter = null,
                    IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
                    StringType = CleanStringType.Ascii,
                    BreakTermsOnUpper = true
                });

            ShortStringHelperResolver.Reset();
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(_helper);
            Resolution.Freeze();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            ShortStringHelperResolver.Reset();
        }

        static readonly Regex FrenchElisionsRegex = new Regex("\\b(c|d|j|l|m|n|qu|s|t)('|\u8217)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string FilterFrenchElisions(string s)
        {
            return FrenchElisionsRegex.Replace(s, "");
        }

        private static string StripQuotes(string s)
        {
            s = s.ReplaceMany(new Dictionary<string, string> {{"'", ""}, {"\u8217", ""}});
            return s;
        }

        private static string WhiteQuotes(string s)
        {
            s = s.ReplaceMany(new Dictionary<string, string> { { "'", " " }, { "\u8217", " " } });
            return s;
        }

        [Test]
        public void U4_4055_4056()
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.RequestHandler);
            contentMock.Setup(x => x.CharCollection).Returns(Enumerable.Empty<IChar>());
            contentMock.Setup(x => x.ConvertUrlsToAscii).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            const string input = "publishedVersion";

            Assert.AreEqual("PublishedVersion", input.ConvertCase(StringAliasCaseType.PascalCase)); // obsolete, use the one below
            Assert.AreEqual("PublishedVersion", input.ToCleanString(CleanStringType.ConvertCase | CleanStringType.PascalCase | CleanStringType.Ascii)); // role, case and code
        }

        [Test]
        public void U4_4056()
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.RequestHandler);
            contentMock.Setup(x => x.CharCollection).Returns(Enumerable.Empty<IChar>());
            contentMock.Setup(x => x.ConvertUrlsToAscii).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            const string input = "ÆØÅ and æøå and 中文测试 and  אודות האתר and größer БбДдЖж page";

            var helper = new DefaultShortStringHelper().WithDefaultConfig(); // unicode
            var output = helper.CleanStringForUrlSegment(input);
            Assert.AreEqual("æøå-and-æøå-and-中文测试-and-אודות-האתר-and-größer-ббдджж-page", output);

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.UrlSegment, new DefaultShortStringHelper.Config
                {
                    IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_',
                    StringType = CleanStringType.LowerCase | CleanStringType.Ascii, // ascii
                    Separator = '-'
                });
            output = helper.CleanStringForUrlSegment(input);
            Assert.AreEqual("aeoa-and-aeoa-and-and-and-grosser-bbddzhzh-page", output);
        }

        [Test]
        public void CleanStringUnderscoreInTerm()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    // underscore is accepted within terms
                    IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_',
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*'
                });
            Assert.AreEqual("foo_bar*nil", helper.CleanString("foo_bar nil", CleanStringType.Alias));

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    // underscore is not accepted within terms
                    IsTerm = (c, leading) => char.IsLetterOrDigit(c),
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*'
                });
            Assert.AreEqual("foo*bar*nil", helper.CleanString("foo_bar nil", CleanStringType.Alias));
        }

        [Test]
        public void CleanStringLeadingChars()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    // letters and digits are valid leading chars
                    IsTerm = (c, leading) => char.IsLetterOrDigit(c),
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*'
                });
            Assert.AreEqual("0123foo*bar*543*nil*321", helper.CleanString("0123foo_bar 543 nil 321", CleanStringType.Alias));

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    // only letters are valid leading chars
                    IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c),
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*'
                });
            Assert.AreEqual("foo*bar*543*nil*321", helper.CleanString("0123foo_bar 543 nil 321", CleanStringType.Alias));
            Assert.AreEqual("foo*bar*543*nil*321", helper.CleanString("0123 foo_bar 543 nil 321", CleanStringType.Alias));

            helper = new DefaultShortStringHelper().WithDefaultConfig();
            Assert.AreEqual("child2", helper.CleanStringForSafeAlias("1child2"));
        }

        [Test]
        public void CleanStringTermOnUpper()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    // uppercase letter means new term
                    BreakTermsOnUpper = true,
                    Separator = '*'
                });
            Assert.AreEqual("foo*Bar", helper.CleanString("fooBar", CleanStringType.Alias));

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    // uppercase letter is part of term
                    BreakTermsOnUpper = false,
                    Separator = '*'
                });
            Assert.AreEqual("fooBar", helper.CleanString("fooBar", CleanStringType.Alias));
        }

        [Test]
        public void CleanStringAcronymOnNonUpper()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    // non-uppercase letter means cut acronym
                    CutAcronymOnNonUpper = true,
                    Separator = '*'
                });
            Assert.AreEqual("foo*BAR*Rnil", helper.CleanString("foo BARRnil", CleanStringType.Alias));
            Assert.AreEqual("foo*BA*Rnil", helper.CleanString("foo BARnil", CleanStringType.Alias));
            Assert.AreEqual("foo*BAnil", helper.CleanString("foo BAnil", CleanStringType.Alias));
            Assert.AreEqual("foo*Bnil", helper.CleanString("foo Bnil", CleanStringType.Alias));

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    // non-uppercase letter means word
                    CutAcronymOnNonUpper = false,
                    Separator = '*'
                });
            Assert.AreEqual("foo*BARRnil", helper.CleanString("foo BARRnil", CleanStringType.Alias));
            Assert.AreEqual("foo*BARnil", helper.CleanString("foo BARnil", CleanStringType.Alias));
            Assert.AreEqual("foo*BAnil", helper.CleanString("foo BAnil", CleanStringType.Alias));
            Assert.AreEqual("foo*Bnil", helper.CleanString("foo Bnil", CleanStringType.Alias));
        }

        [Test]
        public void CleanStringGreedyAcronyms()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    CutAcronymOnNonUpper = true,
                    GreedyAcronyms = true,
                    Separator = '*'
                });
            Assert.AreEqual("foo*BARR*nil", helper.CleanString("foo BARRnil", CleanStringType.Alias));
            Assert.AreEqual("foo*BAR*nil", helper.CleanString("foo BARnil", CleanStringType.Alias));
            Assert.AreEqual("foo*BA*nil", helper.CleanString("foo BAnil", CleanStringType.Alias));
            Assert.AreEqual("foo*Bnil", helper.CleanString("foo Bnil", CleanStringType.Alias));

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    CutAcronymOnNonUpper = true,
                    GreedyAcronyms = false,
                    Separator = '*'
                });
            Assert.AreEqual("foo*BAR*Rnil", helper.CleanString("foo BARRnil", CleanStringType.Alias));
            Assert.AreEqual("foo*BA*Rnil", helper.CleanString("foo BARnil", CleanStringType.Alias));
            Assert.AreEqual("foo*BAnil", helper.CleanString("foo BAnil", CleanStringType.Alias));
            Assert.AreEqual("foo*Bnil", helper.CleanString("foo Bnil", CleanStringType.Alias));
        }

        [Test]
        public void CleanStringWhiteSpace()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*'
                });
            Assert.AreEqual("foo", helper.CleanString("   foo   ", CleanStringType.Alias));
            Assert.AreEqual("foo*bar", helper.CleanString("   foo   bar   ", CleanStringType.Alias));
        }

        [Test]
        public void CleanStringSeparator()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*'
                });
            Assert.AreEqual("foo*bar", helper.CleanString("foo bar", CleanStringType.Alias));

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = ' '
                });
            Assert.AreEqual("foo bar", helper.CleanString("foo bar", CleanStringType.Alias));

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged
                });
            Assert.AreEqual("foobar", helper.CleanString("foo bar", CleanStringType.Alias));

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '文'
                });
            Assert.AreEqual("foo文bar", helper.CleanString("foo bar", CleanStringType.Alias));
        }

        [Test]
        public void CleanStringSymbols()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*'
                });
            Assert.AreEqual("house*2", helper.CleanString("house (2)", CleanStringType.Alias));
            
            // FIXME but for a filename we want to keep them!
            // FIXME and what about a url?
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
            // then next string element is two chars (surrogate pair) or 4 bytes, 21 bits of code point
            Assert.AreEqual('z', bytes[5]);
            // then next string element is one char and 3 bytes, 16 bits of code point
            Assert.AreEqual('t', bytes[9]);
            //foreach (var b in bytes)
            //    Console.WriteLine("{0:X}", b);

            Console.WriteLine("\U00010B70");
        }

        [Test]
        public void Utf8ToAsciiConverter()
        {
            const string str = "a\U00010F00z\uA74Ftéô";
            var output = Core.Strings.Utf8ToAsciiConverter.ToAsciiString(str);
            Assert.AreEqual("a?zooteo", output);
        }

        [Test]
        public void CleanStringEncoding()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = '*'
                });
            Assert.AreEqual("中文测试", helper.CleanString("中文测试", CleanStringType.Alias));
            Assert.AreEqual("léger*中文测试*ZÔRG", helper.CleanString("léger 中文测试 ZÔRG", CleanStringType.Alias));

            helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Ascii | CleanStringType.Unchanged,
                    Separator = '*'
                });
            Assert.AreEqual("", helper.CleanString("中文测试", CleanStringType.Alias));
            Assert.AreEqual("leger*ZORG", helper.CleanString("léger 中文测试 ZÔRG", CleanStringType.Alias));
        }

        [Test]
        public void CleanStringDefaultConfig()
        {
            var settings = SettingsForTests.GenerateMockSettings();
            var contentMock = Mock.Get(settings.RequestHandler);
            contentMock.Setup(x => x.CharCollection).Returns(Enumerable.Empty<IChar>());
            contentMock.Setup(x => x.ConvertUrlsToAscii).Returns(false);
            SettingsForTests.ConfigureSettings(settings);

            var helper = new DefaultShortStringHelper().WithDefaultConfig();

            const string input = "0123 中文测试 中文测试 léger ZÔRG (2) a?? *x";

            var alias = helper.CleanStringForSafeAlias(input);
            var filename = helper.CleanStringForSafeFileName(input);
            var segment = helper.CleanStringForUrlSegment(input);

            // umbraco-cased ascii alias, must begin with a proper letter
            Assert.AreEqual("legerZORG2AX", alias, "alias");

            // lower-cased, utf8 filename, removing illegal filename chars, using dash-separator
            Assert.AreEqual("0123-中文测试-中文测试-léger-zôrg-2-a-x", filename, "filename");

            // lower-cased, utf8 url segment, only letters and digits, using dash-separator
            Assert.AreEqual("0123-中文测试-中文测试-léger-zôrg-2-a-x", segment, "segment");
        }

        [Test]
        public void CleanStringCasing()
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Alias, new DefaultShortStringHelper.Config
                {
                    StringType = CleanStringType.Utf8 | CleanStringType.Unchanged,
                    Separator = ' '
                });

            // BBB is an acronym
            // E is a word (too short to be an acronym)
            // FF is an acronym

            // FIXME "C" can't be an acronym
            // FIXME "DBXreview" = acronym?!

            Assert.AreEqual("aaa BBB CCc Ddd E FF", helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias)); // unchanged
            Assert.AreEqual("aaa Bbb Ccc Ddd E FF", helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.CamelCase));
            Assert.AreEqual("Aaa Bbb Ccc Ddd E FF", helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.PascalCase));
            Assert.AreEqual("aaa bbb ccc ddd e ff", helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.LowerCase));
            Assert.AreEqual("AAA BBB CCC DDD E FF", helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.UpperCase));
            Assert.AreEqual("aaa BBB CCc Ddd E FF", helper.CleanString("aaa BBB CCc Ddd E FF", CleanStringType.Alias | CleanStringType.UmbracoCase));

            // MS rules & guidelines:
            // - Do capitalize both characters of two-character acronyms, except the first word of a camel-cased identifier.
            //     eg "DBRate" (pascal) or "ioHelper" (camel) - "SpecialDBRate" (pascal) or "specialIOHelper" (camel)
            // - Do capitalize only the first character of acronyms with three or more characters, except the first word of a camel-cased identifier.
            //     eg "XmlWriter (pascal) or "htmlReader" (camel) - "SpecialXmlWriter" (pascal) or "specialHtmlReader" (camel)
            // - Do not capitalize any of the characters of any acronyms, whatever their length, at the beginning of a camel-cased identifier.
            //     eg "xmlWriter" or "dbWriter" (camel)

            Assert.AreEqual("aaa BB Ccc", helper.CleanString("aaa BB ccc", CleanStringType.Alias | CleanStringType.CamelCase));
            Assert.AreEqual("aa Bb Ccc", helper.CleanString("AA bb ccc", CleanStringType.Alias | CleanStringType.CamelCase));
            Assert.AreEqual("aaa Bb Ccc", helper.CleanString("AAA bb ccc", CleanStringType.Alias | CleanStringType.CamelCase));
            Assert.AreEqual("db Rate", helper.CleanString("DB rate", CleanStringType.Alias | CleanStringType.CamelCase));
            Assert.AreEqual("special DB Rate", helper.CleanString("special DB rate", CleanStringType.Alias | CleanStringType.CamelCase));
            Assert.AreEqual("xml Writer", helper.CleanString("XML writer", CleanStringType.Alias | CleanStringType.CamelCase));
            Assert.AreEqual("special Xml Writer", helper.CleanString("special XML writer", CleanStringType.Alias | CleanStringType.CamelCase));

            Assert.AreEqual("Aaa BB Ccc", helper.CleanString("aaa BB ccc", CleanStringType.Alias | CleanStringType.PascalCase));
            Assert.AreEqual("AA Bb Ccc", helper.CleanString("AA bb ccc", CleanStringType.Alias | CleanStringType.PascalCase));
            Assert.AreEqual("Aaa Bb Ccc", helper.CleanString("AAA bb ccc", CleanStringType.Alias | CleanStringType.PascalCase));
            Assert.AreEqual("DB Rate", helper.CleanString("DB rate", CleanStringType.Alias | CleanStringType.PascalCase));
            Assert.AreEqual("Special DB Rate", helper.CleanString("special DB rate", CleanStringType.Alias | CleanStringType.PascalCase));
            Assert.AreEqual("Xml Writer", helper.CleanString("XML writer", CleanStringType.Alias | CleanStringType.PascalCase));
            Assert.AreEqual("Special Xml Writer", helper.CleanString("special XML writer", CleanStringType.Alias | CleanStringType.PascalCase));
        }

        #region Cases
        [TestCase("foo", "foo")]
        [TestCase("    foo    ", "foo")]
        [TestCase("Foo", "Foo")]
        [TestCase("FoO", "FoO")]
        [TestCase("FoO bar", "FoOBar")]
        [TestCase("FoO bar NIL", "FoOBarNIL")]
        [TestCase("FoO 33bar 22NIL", "FoO33bar22NIL")]
        [TestCase("FoO 33bar 22NI", "FoO33bar22NI")]
        [TestCase("0foo", "foo")]
        [TestCase("2foo bar", "fooBar")]
        [TestCase("9FOO", "FOO")]
        [TestCase("foo-BAR", "fooBAR")]
        [TestCase("foo-BA-dang", "fooBADang")]
        [TestCase("foo_BAR", "fooBAR")]
        [TestCase("foo'BAR", "fooBAR")]
        [TestCase("sauté dans l'espace", "sauteDansLespace")]
        [TestCase("foo\"\"bar", "fooBar")]
        [TestCase("-foo-", "foo")]
        [TestCase("_foo_", "foo")]
        [TestCase("spécial", "special")]
        [TestCase("brô dëk ", "broDek")]
        [TestCase("1235brô dëk ", "broDek")]
        [TestCase("汉#字*/漢?字", "")]
        [TestCase("aa DB cd EFG X KLMN OP qrst", "aaDBCdEFGXKLMNOPQrst")]
        [TestCase("AA db cd EFG X KLMN OP qrst", "AADbCdEFGXKLMNOPQrst")]
        [TestCase("AAA db cd EFG X KLMN OP qrst", "AAADbCdEFGXKLMNOPQrst")]
        [TestCase("4 ways selector", "waysSelector")]
        [TestCase("WhatIfWeDoItAgain", "WhatIfWeDoItAgain")]
        [TestCase("whatIfWeDoItAgain", "whatIfWeDoItAgain")]
        [TestCase("WhatIfWEDOITAgain", "WhatIfWEDOITAgain")]
        [TestCase("WhatIfWe doItAgain", "WhatIfWeDoItAgain")]
        #endregion
        public void CleanStringForSafeAlias(string input, string expected)
        {
            var output = _helper.CleanStringForSafeAlias(input);
            Assert.AreEqual(expected, output);
        }

        //#region Cases
        //[TestCase("This is my_little_house so cute.", "thisIsMyLittleHouseSoCute", false)]
        //[TestCase("This is my_little_house so cute.", "thisIsMy_little_houseSoCute", true)]
        //[TestCase("This is my_Little_House so cute.", "thisIsMyLittleHouseSoCute", false)]
        //[TestCase("This is my_Little_House so cute.", "thisIsMy_Little_HouseSoCute", true)]
        //[TestCase("An UPPER_CASE_TEST to check", "anUpperCaseTestToCheck", false)]
        //[TestCase("An UPPER_CASE_TEST to check", "anUpper_case_testToCheck", true)]
        //[TestCase("Trailing_", "trailing", false)]
        //[TestCase("Trailing_", "trailing_", true)]
        //[TestCase("_Leading", "leading", false)]
        //[TestCase("_Leading", "leading", true)]
        //[TestCase("Repeat___Repeat", "repeatRepeat", false)]
        //[TestCase("Repeat___Repeat", "repeat___Repeat", true)]
        //[TestCase("Repeat___repeat", "repeatRepeat", false)]
        //[TestCase("Repeat___repeat", "repeat___repeat", true)]
        //#endregion
        //public void CleanStringWithUnderscore(string input, string expected, bool allowUnderscoreInTerm)
        //{
        //    var helper = new DefaultShortStringHelper()
        //        .WithConfig(allowUnderscoreInTerm: allowUnderscoreInTerm);
        //    var output = helper.CleanString(input, CleanStringType.Alias | CleanStringType.Ascii | CleanStringType.CamelCase);
        //    Assert.AreEqual(expected, output);
        //}

        #region Cases
        [TestCase("Home Page", "home-page")]
        [TestCase("Shannon's Home Page!", "shannons-home-page")]
        [TestCase("#Someones's Twitter $h1z%n", "someoness-twitter-h1z-n")]
        [TestCase("Räksmörgås", "raksmorgas")]
        [TestCase("'em guys-over there, are#goin' a \"little\"bit crazy eh!! :)", "em-guys-over-there-are-goin-a-little-bit-crazy-eh")]
        [TestCase("汉#字*/漢?字", "")]
        [TestCase("Réalösk fix bran#lo'sk", "realosk-fix-bran-losk")]
        [TestCase("200 ways to be happy", "200-ways-to-be-happy")]
        #endregion
        public void CleanStringForUrlSegment(string input, string expected)
        {
            var output = _helper.CleanStringForUrlSegment(input);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("ThisIsTheEndMyFriend", "This Is The End My Friend")]
        [TestCase("ThisIsTHEEndMyFriend", "This Is THE End My Friend")]
        [TestCase("THISIsTHEEndMyFriend", "THIS Is THE End My Friend")]
        [TestCase("This33I33sThe33EndMyFriend", "This33 I33s The33 End My Friend")] // works!
        [TestCase("ThisIsTHEEndMyFriendX", "This Is THE End My Friend X")]
        [TestCase("ThisIsTHEEndMyFriendXYZ", "This Is THE End My Friend XYZ")]
        [TestCase("ThisIsTHEEndMyFriendXYZt", "This Is THE End My Friend XY Zt")]
        [TestCase("UneÉlévationÀPartir", "Une Élévation À Partir")]
        #endregion
        public void SplitPascalCasing(string input, string expected)
        {
            var output = _helper.SplitPascalCasing(input, ' ');
            Assert.AreEqual(expected, output);
			
            output = _helper.SplitPascalCasing(input, '*');
            expected = expected.Replace(' ', '*');
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("sauté dans l'espace", "saute-dans-espace", "fr-FR", CleanStringType.UrlSegment | CleanStringType.Ascii | CleanStringType.LowerCase)]
        [TestCase("sauté dans l'espace", "sauté-dans-espace", "fr-FR", CleanStringType.UrlSegment | CleanStringType.Utf8 | CleanStringType.LowerCase)]
        [TestCase("sauté dans l'espace", "SauteDansLEspace", "fr-FR", CleanStringType.Alias | CleanStringType.Ascii | CleanStringType.PascalCase)]
        [TestCase("he doesn't want", "he-doesnt-want", null, CleanStringType.UrlSegment | CleanStringType.Ascii | CleanStringType.LowerCase)]
        [TestCase("he doesn't want", "heDoesntWant", null, CleanStringType.Alias | CleanStringType.Ascii | CleanStringType.CamelCase)]
        #endregion
        public void CleanStringWithTypeAndCulture(string input, string expected, string culture, CleanStringType stringType)
        {
            var cinfo = culture == null ? CultureInfo.InvariantCulture : new CultureInfo(culture);

            // picks the proper config per culture
            // and overrides some stringType params (ascii...)
            var output = _helper.CleanString(input, stringType, cinfo);
            Assert.AreEqual(expected, output);
        }

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
            var output = _helper.ReplaceMany(input, replacements);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("val$id!ate|this|str'ing", "$!'", '-', "val-id-ate|this|str-ing")]
        [TestCase("val$id!ate|this|str'ing", "$!'", '*', "val*id*ate|this|str*ing")]
        #endregion
        public void ReplaceManyByOneChar(string input, string toReplace, char replacement, string expected)
        {
            var output = _helper.ReplaceMany(input, toReplace.ToArray(), replacement);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("foo.txt", "foo.txt")]
        [TestCase("foo", "foo")]
        [TestCase(".txt", ".txt")]
        [TestCase("nag*dog/poo:xit.txt", "nag-dog-poo-xit.txt")]
        [TestCase("the dog is in the house.txt", "the-dog-is-in-the-house.txt")]
        [TestCase("nil.nil.nil.txt", "nil-nil-nil.txt")]
        [TestCase("taradabum", "taradabum")]
        [TestCase("tara$$da:b/u<m", "tara-da-b-u-m")]
        [TestCase("Straße Zvöskî.yop", "strasse-zvoski.yop")]
        [TestCase("yop.Straße Zvöskî", "yop.strasse-zvoski")]
        [TestCase("yop.Straße Zvös--kî", "yop.strasse-zvos-ki")]
        [TestCase("ma--ma---ma.ma-----ma", "ma-ma-ma.ma-ma")]
        #endregion
        public void CleanStringForSafeFileName(string input, string expected)
        {
            var output = _helper.CleanStringForSafeFileName(input);
            Assert.AreEqual(expected, output);
        }
    }
}
