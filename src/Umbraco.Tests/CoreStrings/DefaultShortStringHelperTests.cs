using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Strings;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Tests.CoreStrings
{
    [TestFixture]
    public class DefaultShortStringHelperTests
    {
        private DefaultShortStringHelper _helper;

        [SetUp]
        public void Setup()
        {
            // NOTE: it is not possible to configure the helper once it has been assigned
            // to the resolver and resolution has frozen. but, obviously, it is possible
            // to configure filters and then to alter these filters after resolution has
            // frozen. beware, nothing is thread-safe in-there!

            // NOTE pre-filters runs _before_ Recode takes place
            // so there still may be utf8 chars even though you want ascii

            _helper = new DefaultShortStringHelper()
                .WithConfig(CleanStringType.Url, StripQuotes, allowLeadingDigits: true)
                .WithConfig(new CultureInfo("fr-FR"), CleanStringType.Url, FilterFrenchElisions, allowLeadingDigits: true)
                .WithConfig(CleanStringType.Alias, StripQuotes)
                .WithConfig(new CultureInfo("fr-FR"), CleanStringType.Alias, WhiteQuotes);

            ShortStringHelperResolver.Reset();
            ShortStringHelperResolver.Current = new ShortStringHelperResolver(_helper);
            Resolution.Freeze();
        }

        [TearDown]
        public void TearDown()
        {
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

        #region Cases
        [TestCase("This is my_little_house so cute.", "thisIsMyLittleHouseSoCute", false)]
        [TestCase("This is my_little_house so cute.", "thisIsMy_little_houseSoCute", true)]
        [TestCase("This is my_Little_House so cute.", "thisIsMyLittleHouseSoCute", false)]
        [TestCase("This is my_Little_House so cute.", "thisIsMy_Little_HouseSoCute", true)]
        [TestCase("An UPPER_CASE_TEST to check", "anUpperCaseTestToCheck", false)]
        [TestCase("An UPPER_CASE_TEST to check", "anUpper_case_testToCheck", true)]
        [TestCase("Trailing_", "trailing", false)]
        [TestCase("Trailing_", "trailing_", true)]
        [TestCase("_Leading", "leading", false)]
        [TestCase("_Leading", "leading", true)]
        [TestCase("Repeat___Repeat", "repeatRepeat", false)]
        [TestCase("Repeat___Repeat", "repeat___Repeat", true)]
        [TestCase("Repeat___repeat", "repeatRepeat", false)]
        [TestCase("Repeat___repeat", "repeat___repeat", true)]
        #endregion
        public void CleanStringWithUnderscore(string input, string expected, bool allowUnderscoreInTerm)
        {
            var helper = new DefaultShortStringHelper()
                .WithConfig(allowUnderscoreInTerm: allowUnderscoreInTerm);
            var output = helper.CleanString(input, CleanStringType.Alias | CleanStringType.Ascii | CleanStringType.CamelCase);
            Assert.AreEqual(expected, output);
        }

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
        [TestCase("foo", "foo")]
        [TestCase("    foo    ", "foo")]
        [TestCase("Foo", "foo")]
        [TestCase("FoO", "foO")]
        [TestCase("FoO bar", "foOBar")]
        [TestCase("FoO bar NIL", "foOBarNil")]
        [TestCase("FoO 33bar 22NIL", "foO33bar22Nil")]
        [TestCase("FoO 33bar 22NI", "foO33bar22NI")]
        [TestCase("0foo", "foo")]
        [TestCase("2foo bar", "fooBar")]
        [TestCase("9FOO", "foo")]
        [TestCase("foo-BAR", "fooBar")]
        [TestCase("foo-BA-dang", "fooBADang")]
        [TestCase("foo_BAR", "fooBar")]
        [TestCase("foo'BAR", "fooBar")]
        [TestCase("sauté dans l'espace", "sautéDansLEspace")]
        [TestCase("foo\"\"bar", "fooBar")]
        [TestCase("-foo-", "foo")]
        [TestCase("_foo_", "foo")]
        [TestCase("spécial", "spécial")]
        [TestCase("brô dëk ", "brôDëk")]
        [TestCase("1235brô dëk ", "brôDëk")]
        [TestCase("汉#字*/漢?字", "汉字漢字")]
        [TestCase("aa DB cd EFG X KLMN OP qrst", "aaDBCdEfgXKlmnOPQrst")]
        [TestCase("AA db cd EFG X KLMN OP qrst", "aaDbCdEfgXKlmnOPQrst")]
        [TestCase("AAA db cd EFG X KLMN OP qrst", "aaaDbCdEfgXKlmnOPQrst")]
        [TestCase("quelle élévation à partir", "quelleÉlévationÀPartir")]
        #endregion
        public void CleanUtf8String(string input, string expected)
        {
            input = _helper.Recode(input, CleanStringType.Utf8);
            var output = _helper.CleanUtf8String(input);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("sauté dans l'espace", "saute-dans-espace", "fr-FR", CleanStringType.Url | CleanStringType.Ascii | CleanStringType.LowerCase)]
        [TestCase("sauté dans l'espace", "sauté-dans-espace", "fr-FR", CleanStringType.Url | CleanStringType.Utf8 | CleanStringType.LowerCase)]
        [TestCase("sauté dans l'espace", "SauteDansLEspace", "fr-FR", CleanStringType.Alias | CleanStringType.Ascii | CleanStringType.PascalCase)]
        [TestCase("he doesn't want", "he-doesnt-want", null, CleanStringType.Url | CleanStringType.Ascii | CleanStringType.LowerCase)]
        [TestCase("he doesn't want", "heDoesntWant", null, CleanStringType.Alias | CleanStringType.Ascii | CleanStringType.CamelCase)]
        #endregion
        public void CleanStringWithTypeAndCulture(string input, string expected, string culture, CleanStringType stringType)
        {
            var cinfo = culture == null ? CultureInfo.InvariantCulture : new CultureInfo(culture);
            var separator = (stringType & CleanStringType.Url) == CleanStringType.Url ? '-' : char.MinValue;
            var output = _helper.CleanString(input, stringType, separator, cinfo);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("foo", "foo")]
        [TestCase("    foo    ", "foo")]
        [TestCase("Foo", "foo")]
        [TestCase("FoO", "foO")]
        [TestCase("FoO bar", "foOBar")]
        [TestCase("FoO bar NIL", "foOBarNil")]
        [TestCase("FoO 33bar 22NIL", "foO33bar22Nil")]
        [TestCase("FoO 33bar 22NI", "foO33bar22NI")]
        [TestCase("0foo", "foo")]
        [TestCase("2foo bar", "fooBar")]
        [TestCase("9FOO", "foo")]
        [TestCase("foo-BAR", "fooBar")]
        [TestCase("foo-BA-dang", "fooBADang")]
        [TestCase("foo_BAR", "fooBar")]
        [TestCase("foo'BAR", "fooBar")]
        [TestCase("sauté dans l'espace", "sauteDansLEspace")]
        [TestCase("foo\"\"bar", "fooBar")]
        [TestCase("-foo-", "foo")]
        [TestCase("_foo_", "foo")]
        [TestCase("spécial", "special")]
        [TestCase("brô dëk ", "broDek")]
        [TestCase("1235brô dëk ", "broDek")]
        [TestCase("汉#字*/漢?字", "")]
        [TestCase("aa DB cd EFG X KLMN OP qrst", "aaDBCdEfgXKlmnOPQrst")]
        [TestCase("AA db cd EFG X KLMN OP qrst", "aaDbCdEfgXKlmnOPQrst")]
        [TestCase("AAA db cd EFG X KLMN OP qrst", "aaaDbCdEfgXKlmnOPQrst")]
        #endregion
        public void CleanStringToAscii(string input, string expected)
        {
            var output = _helper.CleanString(input, CleanStringType.Ascii | CleanStringType.CamelCase);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("1235brô dëK tzARlan ban123!pOo", "brodeKtzARlanban123pOo", CleanStringType.Unchanged)]
        [TestCase("    1235brô dëK tzARlan ban123!pOo    ", "brodeKtzARlanban123pOo", CleanStringType.Unchanged)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "BroDeKTzARlanBan123POo", CleanStringType.PascalCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "broDeKTzARlanBan123POo", CleanStringType.CamelCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "BRODEKTZARLANBAN123POO", CleanStringType.UpperCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "brodektzarlanban123poo", CleanStringType.LowerCase)]
        [TestCase("aa DB cd EFG X KLMN OP qrst", "aaDBCdEfgXKlmnOPQrst", CleanStringType.CamelCase)]
        [TestCase("aaa DB cd EFG X KLMN OP qrst", "aaaDBCdEfgXKlmnOPQrst", CleanStringType.CamelCase)]
        [TestCase("aa DB cd EFG X KLMN OP qrst", "AaDBCdEfgXKlmnOPQrst", CleanStringType.PascalCase)]
        [TestCase("aaa DB cd EFG X KLMN OP qrst", "AaaDBCdEfgXKlmnOPQrst", CleanStringType.PascalCase)]
        [TestCase("AA db cd EFG X KLMN OP qrst", "aaDbCdEfgXKlmnOPQrst", CleanStringType.CamelCase)]
        [TestCase("AAA db cd EFG X KLMN OP qrst", "aaaDbCdEfgXKlmnOPQrst", CleanStringType.CamelCase)]
        [TestCase("AA db cd EFG X KLMN OP qrst", "AADbCdEfgXKlmnOPQrst", CleanStringType.PascalCase)]
        [TestCase("AAA db cd EFG X KLMN OP qrst", "AaaDbCdEfgXKlmnOPQrst", CleanStringType.PascalCase)]
        [TestCase("We store some HTML in the DB for performance", "WeStoreSomeHtmlInTheDBForPerformance", CleanStringType.PascalCase)]
        [TestCase("We store some HTML in the DB for performance", "weStoreSomeHtmlInTheDBForPerformance", CleanStringType.CamelCase)]
        [TestCase("X is true", "XIsTrue", CleanStringType.PascalCase)]
        [TestCase("X is true", "xIsTrue", CleanStringType.CamelCase)]
        [TestCase("IO are slow", "IOAreSlow", CleanStringType.PascalCase)]
        [TestCase("IO are slow", "ioAreSlow", CleanStringType.CamelCase)]
        [TestCase("RAM is fast", "RamIsFast", CleanStringType.PascalCase)]
        [TestCase("RAM is fast", "ramIsFast", CleanStringType.CamelCase)]
        [TestCase("Tab 1", "tab1", CleanStringType.CamelCase)]
        [TestCase("Home - Page", "homePage", CleanStringType.CamelCase)]
        [TestCase("Shannon's Document Type", "shannonSDocumentType", CleanStringType.CamelCase)]
        [TestCase("Shannon's Document Type", "shannonsDocumentType", CleanStringType.CamelCase | CleanStringType.Alias)]
        [TestCase("!BADDLY nam-ed Document Type", "baddlyNamEdDocumentType", CleanStringType.CamelCase)]
        [TestCase("  !BADDLY nam-ed Document Type", "BADDLYnamedDocumentType", CleanStringType.Unchanged)]
        [TestCase("!BADDLY nam-ed   Document Type", "BaddlyNamEdDocumentType", CleanStringType.PascalCase)]
        [TestCase("i %Want!thisTo end up In Proper@case", "IWantThisToEndUpInProperCase", CleanStringType.PascalCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "raksmorgasKeKe", CleanStringType.CamelCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "RaksmorgasKeKe", CleanStringType.PascalCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "RaksmorgaskeKe", CleanStringType.Unchanged)]
        [TestCase("TRii", "TRii", CleanStringType.Unchanged)]
        [TestCase("**TRii", "TRii", CleanStringType.Unchanged)]
        [TestCase("TRii", "tRii", CleanStringType.CamelCase)]
        [TestCase("TRXii", "trXii", CleanStringType.CamelCase)]
        [TestCase("**TRii", "tRii", CleanStringType.CamelCase)]
        [TestCase("TRii", "TRii", CleanStringType.PascalCase)]
        [TestCase("TRXii", "TRXii", CleanStringType.PascalCase)]
        [TestCase("**TRii", "TRii", CleanStringType.PascalCase)]
        [TestCase("trII", "trII", CleanStringType.Unchanged)]
        [TestCase("**trII", "trII", CleanStringType.Unchanged)]
        [TestCase("trII", "trII", CleanStringType.CamelCase)]
        [TestCase("**trII", "trII", CleanStringType.CamelCase)]
        [TestCase("trII", "TrII", CleanStringType.PascalCase)]
        [TestCase("**trII", "TrII", CleanStringType.PascalCase)]
        [TestCase("trIIX", "trIix", CleanStringType.CamelCase)]
        [TestCase("**trIIX", "trIix", CleanStringType.CamelCase)]
        [TestCase("trIIX", "TrIix", CleanStringType.PascalCase)]
        [TestCase("**trIIX", "TrIix", CleanStringType.PascalCase)]
        #endregion
        public void CleanStringToAsciiWithType(string input, string expected, CleanStringType caseType)
        {
            var output = _helper.CleanString(input, caseType | CleanStringType.Ascii);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("1235brô dëK tzARlan ban123!pOo", "bro de K tz A Rlan ban123 p Oo", ' ', CleanStringType.Unchanged)]
        [TestCase("    1235brô dëK tzARlan ban123!pOo    ", "bro de K tz A Rlan ban123 p Oo", ' ', CleanStringType.Unchanged)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "Bro De K Tz A Rlan Ban123 P Oo", ' ', CleanStringType.PascalCase)]
        [TestCase("1235brô dëK     tzARlan ban123!pOo", "Bro De K Tz A Rlan Ban123 P Oo", ' ', CleanStringType.PascalCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "bro De K Tz A Rlan Ban123 P Oo", ' ', CleanStringType.CamelCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "bro-De-K-Tz-A-Rlan-Ban123-P-Oo", '-', CleanStringType.CamelCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "BRO-DE-K-TZ-A-RLAN-BAN123-P-OO", '-', CleanStringType.UpperCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "bro-de-k-tz-a-rlan-ban123-p-oo", '-', CleanStringType.LowerCase)]
        [TestCase("Tab 1", "tab 1", ' ', CleanStringType.CamelCase)]
        [TestCase("Home - Page", "home Page", ' ', CleanStringType.CamelCase)]
        [TestCase("Shannon's Document Type", "shannon S Document Type", ' ', CleanStringType.CamelCase)]
        [TestCase("Shannon's Document Type", "shannons Document Type", ' ', CleanStringType.CamelCase | CleanStringType.Alias)]
        [TestCase("!BADDLY nam-ed Document Type", "baddly Nam Ed Document Type", ' ', CleanStringType.CamelCase)]
        [TestCase("  !BADDLY nam-ed Document Type", "BADDLY nam ed Document Type", ' ', CleanStringType.Unchanged)]
        [TestCase("!BADDLY nam-ed   Document Type", "Baddly Nam Ed Document Type", ' ', CleanStringType.PascalCase)]
        [TestCase("i %Want!thisTo end up In Proper@case", "I Want This To End Up In Proper Case", ' ', CleanStringType.PascalCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "raksmorgas Ke Ke", ' ', CleanStringType.CamelCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "Raksmorgas Ke Ke", ' ', CleanStringType.PascalCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "Raksmorgas ke Ke", ' ', CleanStringType.Unchanged)]
        #endregion
        public void CleanStringToAsciiWithTypeAndSeparator(string input, string expected, char separator, CleanStringType caseType)
        {
            var output = _helper.CleanString(input, caseType | CleanStringType.Ascii, separator);
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
