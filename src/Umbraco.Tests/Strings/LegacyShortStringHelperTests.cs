using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Strings
{
    [TestFixture]
    public class LegacyShortStringHelperTests
    {
        private LegacyShortStringHelper _helper;

        [SetUp]
        public void Setup()
        {
            var config = SettingsForTests.GetDefault();
            SettingsForTests.ConfigureSettings(config);
            _helper = new LegacyShortStringHelper();
        }

        [TearDown]
        public void TearDown()
        {
        }


        #region Cases
        [TestCase("foo", "foo")]
        [TestCase("    foo    ", "Foo")]
        [TestCase("Foo", "Foo")]
        [TestCase("FoO", "FoO")]
        [TestCase("FoO bar", "FoOBar")]
        [TestCase("FoO bar NIL", "FoOBarNIL")]
        [TestCase("FoO 33bar 22NIL", "FoO33bar22NIL")]
        [TestCase("FoO 33bar 22NI", "FoO33bar22NI")]
        [TestCase("0foo", "foo")]
        [TestCase("2foo bar", "fooBar")]
        [TestCase("9FOO", "FOO")]
        [TestCase("foo-BAR", "foo-BAR")]
        [TestCase("foo-BA-dang", "foo-BA-dang")]
        [TestCase("foo_BAR", "foo_BAR")]
        [TestCase("foo'BAR", "fooBAR")]
        [TestCase("sauté dans l'espace", "sauteDansLespace", IgnoreReason = "non-supported non-ascii chars")]
        [TestCase("foo\"\"bar", "foobar")]
        [TestCase("-foo-", "-foo-")]
        [TestCase("_foo_", "_foo_")]
        [TestCase("spécial", "special", IgnoreReason = "non-supported non-ascii chars")]
        [TestCase("brô dëk ", "broDek", IgnoreReason = "non-supported non-ascii chars")]
        [TestCase("1235brô dëk ", "broDek", IgnoreReason = "non-supported non-ascii chars")]
        [TestCase("汉#字*/漢?字", "")]
        [TestCase("aa DB cd EFG X KLMN OP qrst", "aaDBCdEFGXKLMNOPQrst")]
        [TestCase("AA db cd EFG X KLMN OP qrst", "AADbCdEFGXKLMNOPQrst")]
        [TestCase("AAA db cd EFG X KLMN OP qrst", "AAADbCdEFGXKLMNOPQrst")]
        [TestCase("4 ways selector", "WaysSelector")]
        [TestCase("WhatIfWeDoItAgain", "WhatIfWeDoItAgain")]
        [TestCase("whatIfWeDoItAgain", "whatIfWeDoItAgain")]
        [TestCase("WhatIfWEDOITAgain", "WhatIfWEDOITAgain")]
        [TestCase("WhatIfWe doItAgain", "WhatIfWeDoItAgain")]
        #endregion
        public void CleanStringForSafeAlias(string input, string expected)
        {
            // NOTE legacy CleanStringForSafeAlias has issues w/some cases
            // -> ignore test cases
            // also, some aliases are strange... how can "-foo-" be a valid alias?
            var output = _helper.CleanStringForSafeAlias(input);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("Tab 1", "tab1")]
        [TestCase("Home - Page", "homePage")]
        [TestCase("Home.Page", "homePage")]
        [TestCase("Shannon's Document Type", "shannonsDocumentType")] // look, lowercase s and the end of shannons
        [TestCase("!BADDLY nam-ed Document Type", "baddlyNamEdDocumentType")]
        [TestCase("i %Want!thisTo end up In Proper@case", "iWantThisToEndUpInProperCase")]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "raksmorgasKeKe", IgnoreReason = "non-supported non-ascii chars")]
        [TestCase("TRii", "tRii")]
        [TestCase("**TRii", "tRii")]
        [TestCase("trII", "trII")]
        [TestCase("**trII", "trII")]
        [TestCase("trIIX", "trIIX")]
        [TestCase("**trIIX", "trIIX")]
        #endregion
        public void LegacyCleanStringForUmbracoAlias(string input, string expected)
        {
            // NOTE ToUmbracoAlias has issues w/non-ascii, and a few other things
            // -> ignore test cases
            // also all those tests should, in theory, fail because removeSpaces is false by default
            var output = _helper.LegacyCleanStringForUmbracoAlias(input);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("Home Page", "home-page")]
        [TestCase("Shannon's Home Page!", "shannons-home-page!")]
        [TestCase("#Someones's Twitter $h1z%n", "someoness-twitter-$h1zn")]
        [TestCase("Räksmörgås", "raeksmoergaas")]
        [TestCase("'em guys-over there, are#goin' a \"little\"bit crazy eh!! :)", "em-guys-over-there,-aregoin-a-littlebit-crazy-eh!!-)")]
        [TestCase("汉#字*/漢?字", "汉字star漢字")]
        [TestCase("Réalösk fix bran#lo'sk", "realosk-fix-bran-lo-sk", IgnoreReason = "cannot handle it")]
        #endregion
        public void LegacyFormatUrl(string input, string expected)
        {
            // NOTE CleanStringForUrlSegment has issues with a few cases
            // -> ignore test cases
            // also some results are a bit strange...
            var output = _helper.LegacyFormatUrl(input);
            Assert.AreEqual(expected, output);

            // NOTE: not testing the overload with culture
            // in legacy, they are the same
        }

        #region Cases
        [TestCase("Home Page", "home-page", true, true, false)]
        [TestCase("Shannon's Home Page!", "shannons-home-page", true, true, false)]
        [TestCase("#Someones's Twitter $h1z%n", "someoness-twitter-h1zn", true, true, false)]
        [TestCase("Räksmörgås", "rksmrgs", true, true, false)]
        [TestCase("'em guys-over there, are#goin' a \"little\"bit crazy eh!! :)", "em-guys-over-there-aregoin-a-littlebit-crazy-eh", true, true, false)]
        [TestCase("汉#字*/漢?字", "", true, true, false)]
        [TestCase("汉#字*/漢?字", "汉字漢字", true, false, false)]
        [TestCase("汉#字*/漢?字", "%e6%b1%89%e5%ad%97%e6%bc%a2%e5%ad%97", true, false, true)]
        [TestCase("Réalösk fix bran#lo'sk", "realosk-fix-bran-lo-sk", true, true, false, IgnoreReason = "cannot handle it")]
        #endregion
        public void LegacyToUrlAlias(string input, string expected, bool replaceDoubleDashes, bool stripNonAscii, bool urlEncode)
        {
            var replacements = new Dictionary<string, string>
            {
                {" ", "-"},
                {"\"", ""},
                {"&quot;", ""},
                {"@", ""},
                {"%", ""},
                {".", ""},
                {";", ""},
                {"/", ""},
                {":", ""},
                {"#", ""},
                {"+", ""},
                {"*", ""},
                {"&amp;", ""},
                {"?", ""}
            };

            // NOTE CleanStringForUrlSegment has issues with a few cases
            // -> ignore test cases
            // also some results are a bit strange...
            var output = _helper.LegacyToUrlAlias(input, replacements, replaceDoubleDashes, stripNonAscii, urlEncode);
            Assert.AreEqual(expected, output);

            // NOTE: not testing the overload with culture
            // in legacy, they are the same
        }

        #region Cases
        [TestCase("Tab 1", "tab1", CleanStringType.CamelCase)]
        [TestCase("Home - Page", "homePage", CleanStringType.CamelCase)]
        [TestCase("Shannon's document type", "shannon'sDocumentType", CleanStringType.CamelCase)]
        [TestCase("This is the FIRSTTIME of TheDay.", "ThisistheFIRSTTIMEofTheDay", CleanStringType.Unchanged)]
        [TestCase("Sépàyô lüx.", "Sepayolux", CleanStringType.Unchanged, IgnoreReason = "non-supported non-ascii chars")]
        [TestCase("This is the FIRSTTIME of TheDay.", "ThisIsTheFIRSTTIMEOfTheDay", CleanStringType.PascalCase)]
        [TestCase("This is the FIRSTTIME of TheDay.", "thisIsTheFIRSTTIMEOfTheDay", CleanStringType.CamelCase)]
        #endregion
        public void LegacyConvertStringCase(string input, string expected, CleanStringType caseType)
        {
            // NOTE LegacyConvertStringCase has issues with a few cases
            // -> ignore test cases
            // also it removes symbols, etc... except the quote?
            var output = _helper.LegacyConvertStringCase(input, caseType);
            Assert.AreEqual(expected, output);
        }

        #region Cases
        [TestCase("ThisIsTheEndMyFriend", "This Is The End My Friend")]
        [TestCase("ThisIsTHEEndMyFriend", "This Is THE End My Friend")]
        [TestCase("THISIsTHEEndMyFriend", "THIS Is THE End My Friend")]
        [TestCase("This33I33sThe33EndMyFriend", "This33 I33s The33 End My Friend", IgnoreReason = "fails")]
        [TestCase("ThisIsTHEEndMyFriendX", "This Is THE End My Friend X")]
        [TestCase("ThisIsTHEEndMyFriendXYZ", "This Is THE End My Friend XYZ")]
        [TestCase("ThisIsTHEEndMyFriendXYZt", "This Is THE End My Friend XY Zt")]
        [TestCase("UneÉlévationÀPartir", "Une Élévation À Partir", IgnoreReason = "non-supported non-ascii chars")]
        #endregion
        public void SplitPascalCasing(string input, string expected)
        {
            // NOTE legacy SplitPascalCasing has issues w/some cases
            // -> ignore test cases
            var output = _helper.SplitPascalCasing(input, ' ');
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
            // legacy does nothing
            Assert.AreEqual(input, output);
        }

        #region Cases
        [TestCase("1235brô dëK tzARlan ban123!pOo", "brodeKtzARlanban123pOo", CleanStringType.Unchanged)]
        [TestCase("    1235brô dëK tzARlan ban123!pOo    ", "brodeKtzARlanban123pOo", CleanStringType.Unchanged)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "BroDeKTzARLanBan123POo", CleanStringType.PascalCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "broDeKTzARLanBan123POo", CleanStringType.CamelCase)]
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
        [TestCase("Shannon's Document Type", "shannonsDocumentType", CleanStringType.CamelCase)]
        [TestCase("!BADDLY nam-ed Document Type", "baddlyNamEdDocumentType", CleanStringType.CamelCase)]
        [TestCase("  !BADDLY nam-ed Document Type", "BADDLYnamedDocumentType", CleanStringType.Unchanged)]
        [TestCase("!BADDLY nam-ed   Document Type", "BaddlyNamEdDocumentType", CleanStringType.PascalCase)]
        [TestCase("i %Want!thisTo end up In Proper@case", "IWantThisToEndUpInProperCase", CleanStringType.PascalCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "raksmorgasKeKe", CleanStringType.CamelCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "RaksmorgasKeKe", CleanStringType.PascalCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "RaksmorgaskeKe", CleanStringType.Unchanged)]
        [TestCase("TRii", "TRii", CleanStringType.Unchanged)]
        [TestCase("**TRii", "TRii", CleanStringType.Unchanged)]
        [TestCase("TRii", "trIi", CleanStringType.CamelCase)]
        [TestCase("**TRii", "trIi", CleanStringType.CamelCase)]
        [TestCase("TRii", "TRIi", CleanStringType.PascalCase)]
        [TestCase("**TRii", "TRIi", CleanStringType.PascalCase)]
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
        public void CleanStringToAsciiWithCase(string input, string expected, CleanStringType caseType)
        {
            var output = _helper.CleanString(input, caseType | CleanStringType.Ascii);
            // legacy does nothing
            Assert.AreEqual(input, output);
        }

        #region Cases
        [TestCase("1235brô dëK tzARlan ban123!pOo", "bro de K tz AR lan ban123 p Oo", ' ', CleanStringType.Unchanged)]
        [TestCase("    1235brô dëK tzARlan ban123!pOo    ", "bro de K tz AR lan ban123 p Oo", ' ', CleanStringType.Unchanged)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "Bro De K Tz AR Lan Ban123 P Oo", ' ', CleanStringType.PascalCase)]
        [TestCase("1235brô dëK     tzARlan ban123!pOo", "Bro De K Tz AR Lan Ban123 P Oo", ' ', CleanStringType.PascalCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "bro De K Tz AR Lan Ban123 P Oo", ' ', CleanStringType.CamelCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "bro-De-K-Tz-AR-Lan-Ban123-P-Oo", '-', CleanStringType.CamelCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "BRO-DE-K-TZ-AR-LAN-BAN123-P-OO", '-', CleanStringType.UpperCase)]
        [TestCase("1235brô dëK tzARlan ban123!pOo", "bro-de-k-tz-ar-lan-ban123-p-oo", '-', CleanStringType.LowerCase)]
        [TestCase("Tab 1", "tab 1", ' ', CleanStringType.CamelCase)]
        [TestCase("Home - Page", "home Page", ' ', CleanStringType.CamelCase)]
        [TestCase("Shannon's Document Type", "shannons Document Type", ' ', CleanStringType.CamelCase)]
        [TestCase("!BADDLY nam-ed Document Type", "baddly Nam Ed Document Type", ' ', CleanStringType.CamelCase)]
        [TestCase("  !BADDLY nam-ed Document Type", "BADDLY nam ed Document Type", ' ', CleanStringType.Unchanged)]
        [TestCase("!BADDLY nam-ed   Document Type", "Baddly Nam Ed Document Type", ' ', CleanStringType.PascalCase)]
        [TestCase("i %Want!thisTo end up In Proper@case", "I Want This To End Up In Proper Case", ' ', CleanStringType.PascalCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "raksmorgas Ke Ke", ' ', CleanStringType.CamelCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "Raksmorgas Ke Ke", ' ', CleanStringType.PascalCase)]
        [TestCase("Räksmörgås %%$£¤¤¤§ kéKé", "Raksmorgas ke Ke", ' ', CleanStringType.Unchanged)]
        #endregion
        public void CleanStringToAsciiWithCaseAndSeparator(string input, string expected, char separator, CleanStringType caseType)
        {
            var output = _helper.CleanString(input, caseType | CleanStringType.Ascii, separator);
            // legacy does nothing
            Assert.AreEqual(input, output);
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
        [TestCase("foo", "foo", IgnoreReason = "fails when no extension")]
        [TestCase(".txt", ".txt")]
        [TestCase("nag*dog/poo:xit.txt", "nag-dog-poo-xit.txt")]
        [TestCase("the dog is in the house.txt", "the-dog-is-in-the-house.txt")]
        [TestCase("nil.nil.nil.txt", "nilnilnil.txt")] // because of chars map
        [TestCase("taradabum", "taradabum", IgnoreReason = "fails when no extension")]
        [TestCase("tara$$da:b/u<m", "tara-da-b-u-m", IgnoreReason = "fails when no extension")]
        [TestCase("Straße Zvöskî.yop", "Strasse-Zvoeskî.yop")] // because of chars map + does not lowercase
        [TestCase("yop.Straße Zvöskî", "yop.Straße-Zvöskî")] // also note that neither î nor ß are removed, not in the map
        [TestCase("yop.Straße Zvös--kî", "yop.Straße-Zvös-kî")] // and finaly, not the same rule for ext eg ö...
        [TestCase("ma--ma---ma.ma-----ma", "ma-ma-ma.ma-ma")]
        #endregion
        public void CleanStringForSafeFileName(string input, string expected)
        {
            var output = _helper.CleanStringForSafeFileName(input);
            Assert.AreEqual(expected, output);
        }
    }
}
