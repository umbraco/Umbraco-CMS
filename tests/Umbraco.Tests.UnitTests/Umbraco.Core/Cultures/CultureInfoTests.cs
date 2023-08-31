using System.Globalization;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cultures;

[TestFixture]
public class CultureInfoTests
{
    /// <summary>
    /// This tests how custom cultures merge their data from the language and country cultures.
    /// We can use this test to ensure that the custom cultures merge thier data in the same manner on all platforms.
    /// </summary>
    [TestCase("en-CZ", "English (Czechia)", "en", "cs-CZ")]
    [TestCase("en-DZ", "English (Algeria)", "en", "fr-DZ")]
    [TestCase("en-BA", "English (Bosnia & Herzegovina)", "en", "hr-BA")]
    [TestCase("en-HR", "English (Croatia)", "en", "hr-HR")]
    [TestCase("en-GL", "English (Greenland)", "en", "da-GL")]
    [TestCase("en-AW", "English (Aruba)", "en", "nl-AW")]
    [TestCase("en-BQ", "English (Bonaire, Sint Eustatius and Saba)", "en", "nl-BQ")]
    [TestCase("ar-IN", "العربية (الهند)", "ar", "hi-IN")]
    public void Can_Create_Custom_CultureInfo(string isoCode, string nativeName, string languageIsoCode, string countryNativeIsoCode)
    {
        var customCultureInfo = new CultureInfo(isoCode);
        var languageCultureInfo = new CultureInfo(languageIsoCode);
        var countryCultureInfo = new CultureInfo(countryNativeIsoCode);

        Assert.AreEqual(isoCode, customCultureInfo.Name);
        Assert.AreEqual(countryCultureInfo.Calendar.GetType(), customCultureInfo.Calendar.GetType());
        Assert.AreEqual(nativeName, customCultureInfo.NativeName);
        Assert.AreEqual(4096, customCultureInfo.LCID);

        if (languageIsoCode == "en")
        {
            Assert.AreEqual("eng", customCultureInfo.ThreeLetterISOLanguageName);
        }

        Assert.AreEqual("ZZZ", customCultureInfo.ThreeLetterWindowsLanguageName);
        Assert.AreEqual(languageIsoCode, customCultureInfo.TwoLetterISOLanguageName);

        Assert.AreEqual(languageCultureInfo.DateTimeFormat.DayNames, customCultureInfo.DateTimeFormat.DayNames);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.MonthNames, customCultureInfo.DateTimeFormat.MonthNames);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.AbbreviatedDayNames, customCultureInfo.DateTimeFormat.AbbreviatedDayNames);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.AbbreviatedMonthNames, customCultureInfo.DateTimeFormat.AbbreviatedMonthNames);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.ShortestDayNames, customCultureInfo.DateTimeFormat.ShortestDayNames);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.AMDesignator, customCultureInfo.DateTimeFormat.AMDesignator);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.PMDesignator, customCultureInfo.DateTimeFormat.PMDesignator);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.AbbreviatedMonthGenitiveNames, customCultureInfo.DateTimeFormat.AbbreviatedMonthGenitiveNames);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.MonthGenitiveNames, customCultureInfo.DateTimeFormat.MonthGenitiveNames);
        Assert.AreEqual(countryCultureInfo.DateTimeFormat.CalendarWeekRule, customCultureInfo.DateTimeFormat.CalendarWeekRule);
        Assert.AreEqual(countryCultureInfo.DateTimeFormat.FirstDayOfWeek, customCultureInfo.DateTimeFormat.FirstDayOfWeek);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.LongDatePattern, customCultureInfo.DateTimeFormat.LongDatePattern);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.MonthDayPattern, customCultureInfo.DateTimeFormat.MonthDayPattern);
        Assert.AreEqual(countryCultureInfo.DateTimeFormat.RFC1123Pattern, customCultureInfo.DateTimeFormat.RFC1123Pattern);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.ShortDatePattern, customCultureInfo.DateTimeFormat.ShortDatePattern);
        Assert.AreEqual(countryCultureInfo.DateTimeFormat.SortableDateTimePattern, customCultureInfo.DateTimeFormat.SortableDateTimePattern);
        Assert.AreEqual(countryCultureInfo.DateTimeFormat.UniversalSortableDateTimePattern, customCultureInfo.DateTimeFormat.UniversalSortableDateTimePattern);
        Assert.AreEqual(languageCultureInfo.DateTimeFormat.YearMonthPattern, customCultureInfo.DateTimeFormat.YearMonthPattern);

        if (languageIsoCode == "ar")
        {
            Assert.AreEqual(",", customCultureInfo.TextInfo.ListSeparator);
        }
        else
        {
            Assert.AreEqual(languageCultureInfo.TextInfo.ListSeparator, customCultureInfo.TextInfo.ListSeparator);
        }
        Assert.AreEqual(languageCultureInfo.TextInfo.ANSICodePage, customCultureInfo.TextInfo.ANSICodePage);
        Assert.AreEqual(languageCultureInfo.TextInfo.EBCDICCodePage, customCultureInfo.TextInfo.EBCDICCodePage);
        Assert.AreEqual(languageCultureInfo.TextInfo.MacCodePage, customCultureInfo.TextInfo.MacCodePage);
        Assert.AreEqual(languageCultureInfo.TextInfo.OEMCodePage, customCultureInfo.TextInfo.OEMCodePage);
        Assert.AreEqual(languageCultureInfo.TextInfo.IsReadOnly, customCultureInfo.TextInfo.IsReadOnly);
        Assert.AreEqual(languageCultureInfo.TextInfo.IsRightToLeft, customCultureInfo.TextInfo.IsRightToLeft);

        Assert.AreEqual(languageCultureInfo.NumberFormat.CurrencyDecimalDigits, customCultureInfo.NumberFormat.CurrencyDecimalDigits);
        Assert.AreEqual(languageCultureInfo.NumberFormat.CurrencyDecimalSeparator, customCultureInfo.NumberFormat.CurrencyDecimalSeparator);
        Assert.AreEqual(languageCultureInfo.NumberFormat.CurrencyGroupSeparator, customCultureInfo.NumberFormat.CurrencyGroupSeparator);
        Assert.AreEqual(languageCultureInfo.NumberFormat.CurrencyGroupSizes, customCultureInfo.NumberFormat.CurrencyGroupSizes);
        Assert.AreEqual(languageCultureInfo.NumberFormat.CurrencyNegativePattern, customCultureInfo.NumberFormat.CurrencyNegativePattern);
        Assert.AreEqual(languageCultureInfo.NumberFormat.CurrencyPositivePattern, customCultureInfo.NumberFormat.CurrencyPositivePattern);
        Assert.AreEqual(countryCultureInfo.NumberFormat.DigitSubstitution, customCultureInfo.NumberFormat.DigitSubstitution);
        Assert.AreEqual(languageCultureInfo.NumberFormat.IsReadOnly, customCultureInfo.NumberFormat.IsReadOnly);
        Assert.AreEqual(languageCultureInfo.NumberFormat.NativeDigits, customCultureInfo.NumberFormat.NativeDigits);
        Assert.AreEqual(languageCultureInfo.NumberFormat.NegativeInfinitySymbol, customCultureInfo.NumberFormat.NegativeInfinitySymbol);
        Assert.AreEqual(languageCultureInfo.NumberFormat.NegativeSign, customCultureInfo.NumberFormat.NegativeSign);
        Assert.AreEqual(languageCultureInfo.NumberFormat.NumberDecimalDigits, customCultureInfo.NumberFormat.NumberDecimalDigits);
        Assert.AreEqual(languageCultureInfo.NumberFormat.NumberDecimalSeparator, customCultureInfo.NumberFormat.NumberDecimalSeparator);
        Assert.AreEqual(languageCultureInfo.NumberFormat.NumberGroupSeparator, customCultureInfo.NumberFormat.NumberGroupSeparator);
        Assert.AreEqual(languageCultureInfo.NumberFormat.NumberGroupSizes, customCultureInfo.NumberFormat.NumberGroupSizes);
        Assert.AreEqual(languageCultureInfo.NumberFormat.NumberNegativePattern, customCultureInfo.NumberFormat.NumberNegativePattern);
        Assert.AreEqual(languageCultureInfo.NumberFormat.PercentDecimalDigits, customCultureInfo.NumberFormat.PercentDecimalDigits);
        Assert.AreEqual(languageCultureInfo.NumberFormat.PercentDecimalSeparator, customCultureInfo.NumberFormat.PercentDecimalSeparator);
        Assert.AreEqual(languageCultureInfo.NumberFormat.PercentGroupSeparator, customCultureInfo.NumberFormat.PercentGroupSeparator);
    }
}
