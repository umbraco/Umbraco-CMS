// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

[TestFixture]
public class DefaultShortStringHelperTests
{
    [SetUp]
    public void SetUp() =>

        // NOTE pre-filters runs _before_ Recode takes place
        // so there still may be utf8 chars even though you want ascii
        ShortStringHelper = new DefaultShortStringHelper(new DefaultShortStringHelperConfig()
            .WithDefault(new RequestHandlerSettings())
            .WithConfig(CleanStringType.FileName, new DefaultShortStringHelperConfig.Config
            {
                // PreFilter = ClearFileChars, // done in IsTerm
                IsTerm = (c, leading) =>
                    (char.IsLetterOrDigit(c) || c == '_') && DefaultShortStringHelper.IsValidFileNameChar(c),
                StringType = CleanStringType.LowerCase | CleanStringType.Ascii,
                Separator = '-',
            })
            .WithConfig(
                CleanStringType.UrlSegment,
                new DefaultShortStringHelperConfig.Config
                {
                    PreFilter = StripQuotes,
                    IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_',
                    StringType = CleanStringType.LowerCase | CleanStringType.Ascii,
                    Separator = '-',
                })
            .WithConfig(
                "fr-FR",
                CleanStringType.UrlSegment,
                new DefaultShortStringHelperConfig.Config
                {
                    PreFilter = FilterFrenchElisions,
                    IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c) || c == '_',
                    StringType = CleanStringType.LowerCase | CleanStringType.Ascii,
                    Separator = '-',
                })
            .WithConfig(
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    PreFilter = StripQuotes,
                    IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c),
                    StringType = CleanStringType.UmbracoCase | CleanStringType.Ascii,
                })
            .WithConfig(
                "fr-FR",
                CleanStringType.Alias,
                new DefaultShortStringHelperConfig.Config
                {
                    PreFilter = WhiteQuotes,
                    IsTerm = (c, leading) => leading ? char.IsLetter(c) : char.IsLetterOrDigit(c),
                    StringType = CleanStringType.UmbracoCase | CleanStringType.Ascii,
                })
            .WithConfig(CleanStringType.ConvertCase, new DefaultShortStringHelperConfig.Config
            {
                PreFilter = null,
                IsTerm = (c, leading) => char.IsLetterOrDigit(c) || c == '_', // letter, digit or underscore
                StringType = CleanStringType.Ascii,
                BreakTermsOnUpper = true,
            }));

    private IShortStringHelper ShortStringHelper { get; set; }

    private static readonly Regex s_frenchElisionsRegex =
        new("\\b(c|d|j|l|m|n|qu|s|t)('|\u8217)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static string FilterFrenchElisions(string s) => s_frenchElisionsRegex.Replace(s, string.Empty);

    private static string StripQuotes(string s)
    {
        s = s.ReplaceMany(new Dictionary<string, string> { { "'", string.Empty }, { "\u8217", string.Empty } });
        return s;
    }

    private static string WhiteQuotes(string s)
    {
        s = s.ReplaceMany(new Dictionary<string, string> { { "'", " " }, { "\u8217", " " } });
        return s;
    }

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
    [TestCase("What if I have emojis 🎈", "WhatIfIHaveEmojis")]
    public void CleanStringForSafeAlias(string input, string expected)
    {
        var output = ShortStringHelper.CleanStringForSafeAlias(input);
        Assert.AreEqual(expected, output);
    }

    [TestCase("Home Page", "home-page")]
    [TestCase("Shannon's Home Page!", "shannons-home-page")]
    [TestCase("#Someones's Twitter $h1z%n", "someoness-twitter-h1z-n")]
    [TestCase("Räksmörgås", "raksmorgas")]
    [TestCase("'em guys-over there, are#goin' a \"little\"bit crazy eh!! :)", "em-guys-over-there-are-goin-a-little-bit-crazy-eh")]
    [TestCase("汉#字*/漢?字", "")]
    [TestCase("Réalösk fix bran#lo'sk", "realosk-fix-bran-losk")]
    [TestCase("200 ways to be happy", "200-ways-to-be-happy")]
    [TestCase("What if I have emojis 🎈", "what-if-i-have-emojis")]
    public void CleanStringForUrlSegment(string input, string expected)
    {
        var output = ShortStringHelper.CleanStringForUrlSegment(input);
        Assert.AreEqual(expected, output);
    }

    [TestCase("ThisIsTheEndMyFriend", "This Is The End My Friend")]
    [TestCase("ThisIsTHEEndMyFriend", "This Is THE End My Friend")]
    [TestCase("THISIsTHEEndMyFriend", "THIS Is THE End My Friend")]
    [TestCase("This33I33sThe33EndMyFriend", "This33 I33s The33 End My Friend")] // works!
    [TestCase("ThisIsTHEEndMyFriendX", "This Is THE End My Friend X")]
    [TestCase("ThisIsTHEEndMyFriendXYZ", "This Is THE End My Friend XYZ")]
    [TestCase("ThisIsTHEEndMyFriendXYZt", "This Is THE End My Friend XY Zt")]
    [TestCase("UneÉlévationÀPartir", "Une Élévation À Partir")]
    [TestCase("WhatIfIHaveEmojis🎈", "What If I Have Emojis🎈")]
    public void SplitPascalCasing(string input, string expected)
    {
        var output = ShortStringHelper.SplitPascalCasing(input, ' ');
        Assert.AreEqual(expected, output);

        output = ShortStringHelper.SplitPascalCasing(input, '*');
        expected = expected.Replace(' ', '*');
        Assert.AreEqual(expected, output);
    }

    [TestCase(
        "sauté dans l'espace",
        "saute-dans-espace",
        "fr-FR",
        CleanStringType.UrlSegment | CleanStringType.Ascii | CleanStringType.LowerCase)]
    [TestCase("sauté dans l'espace", "sauté-dans-espace", "fr-FR", CleanStringType.UrlSegment | CleanStringType.Utf8 | CleanStringType.LowerCase)]
    [TestCase("sauté dans l'espace", "SauteDansLEspace", "fr-FR", CleanStringType.Alias | CleanStringType.Ascii | CleanStringType.PascalCase)]
    [TestCase("he doesn't want", "he-doesnt-want", null, CleanStringType.UrlSegment | CleanStringType.Ascii | CleanStringType.LowerCase)]
    [TestCase("he doesn't want", "heDoesntWant", null, CleanStringType.Alias | CleanStringType.Ascii | CleanStringType.CamelCase)]
    public void CleanStringWithTypeAndCulture(string input, string expected, string culture, CleanStringType stringType)
    {
        // picks the proper config per culture
        // and overrides some stringType params (ascii...)
        var output = ShortStringHelper.CleanString(input, stringType, culture);
        Assert.AreEqual(expected, output);
    }

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
    [TestCase("What if I have emojis 🎈", "what-if-i-have-emojis")]
    public void CleanStringForSafeFileName(string input, string expected)
    {
        var output = ShortStringHelper.CleanStringForSafeFileName(input);
        Assert.AreEqual(expected, output);
    }
}
