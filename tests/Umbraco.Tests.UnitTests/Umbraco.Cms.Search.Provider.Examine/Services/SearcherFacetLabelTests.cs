using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Search.Provider.Examine.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Search.Provider.Examine.Services;

[TestFixture]
public class SearcherFacetLabelTests
{
    // The index is written by a background thread running under the process culture, but read on a request thread
    // running under the culture of the authenticated back office user, so the two routinely disagree.
    [TestCase("1.55", "en-US")]
    [TestCase("1.55", "da-DK")]
    [TestCase("1,55", "en-US")]
    [TestCase("1,55", "da-DK")]
    public void FacetLabel_IsParsedIndependentlyOfTheReadingCulture(string label, string readingCulture)
    {
        using var cultureScope = new CultureScope(readingCulture);

        Assert.That(Searcher.TryParseFacetLabelAsDecimal(label, out var value), Is.True);
        Assert.That(value, Is.EqualTo(1.55m));
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    public void NonNumericFacetLabel_IsNotParsed(string readingCulture)
    {
        using var cultureScope = new CultureScope(readingCulture);

        Assert.That(Searcher.TryParseFacetLabelAsDecimal("not a number", out _), Is.False);
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _original = CultureInfo.CurrentCulture;

        public CultureScope(string culture) => CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

        public void Dispose() => CultureInfo.CurrentCulture = _original;
    }
}
