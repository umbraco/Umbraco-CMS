// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

[TestFixture]
internal sealed class PropertyDataCultureResolverTests
{
    [Test]
    public void InvariantPropertyType_NullLanguageId_ReturnsNullCulture()
    {
        var propertyType = CreatePropertyType(ContentVariation.Nothing);
        var languages = new Dictionary<int, ILanguage>();

        var result = PropertyDataCultureResolver.ResolveCulture(propertyType, languageId: null, languages);

        Assert.That(result.Culture, Is.Null);
        Assert.That(result.ShouldSkip, Is.False);
    }

    [Test]
    public void InvariantPropertyType_WithLanguageId_ReturnsNullCulture_DoesNotSkip()
    {
        var propertyType = CreatePropertyType(ContentVariation.Nothing);
        var languages = new Dictionary<int, ILanguage>();

        var result = PropertyDataCultureResolver.ResolveCulture(propertyType, languageId: 5, languages);

        Assert.That(result.Culture, Is.Null);
        Assert.That(result.ShouldSkip, Is.False);
    }

    [Test]
    public void CultureVaryingPropertyType_NullLanguageId_ReturnsNullCulture_DoesNotSkip()
    {
        // This is the bug scenario: an invariant content type uses a composition
        // with a culture-varying property. The property data has languageId = NULL
        // (correct for invariant content). The migration must NOT skip this row.
        var propertyType = CreatePropertyType(ContentVariation.Culture);
        var languages = new Dictionary<int, ILanguage> { { 1, CreateLanguage("en-US") } };

        var result = PropertyDataCultureResolver.ResolveCulture(propertyType, languageId: null, languages);

        Assert.That(result.Culture, Is.Null);
        Assert.That(result.ShouldSkip, Is.False);
    }

    [Test]
    public void CultureVaryingPropertyType_ValidLanguageId_ReturnsCulture()
    {
        var propertyType = CreatePropertyType(ContentVariation.Culture);
        var languages = new Dictionary<int, ILanguage> { { 1, CreateLanguage("en-US") } };

        var result = PropertyDataCultureResolver.ResolveCulture(propertyType, languageId: 1, languages);

        Assert.That(result.Culture, Is.EqualTo("en-US"));
        Assert.That(result.ShouldSkip, Is.False);
    }

    [Test]
    public void CultureVaryingPropertyType_OrphanedLanguageId_ShouldSkip()
    {
        // The language was deleted — languageId is non-null but not in the lookup.
        var propertyType = CreatePropertyType(ContentVariation.Culture);
        var languages = new Dictionary<int, ILanguage> { { 1, CreateLanguage("en-US") } };

        var result = PropertyDataCultureResolver.ResolveCulture(propertyType, languageId: 99, languages);

        Assert.That(result.Culture, Is.Null);
        Assert.That(result.ShouldSkip, Is.True);
        Assert.That(result.OrphanedLanguageId, Is.EqualTo(99));
    }

    [Test]
    public void CultureVaryingPropertyType_EmptyDictionary_WithLanguageId_ShouldSkip()
    {
        var propertyType = CreatePropertyType(ContentVariation.Culture);
        var languages = new Dictionary<int, ILanguage>();

        var result = PropertyDataCultureResolver.ResolveCulture(propertyType, languageId: 1, languages);

        Assert.That(result.Culture, Is.Null);
        Assert.That(result.ShouldSkip, Is.True);
        Assert.That(result.OrphanedLanguageId, Is.EqualTo(1));
    }

    [Test]
    public void CultureAndSegmentVaryingPropertyType_NullLanguageId_ReturnsNullCulture_DoesNotSkip()
    {
        // CultureAndSegment also has VariesByCulture() == true, so the same
        // composition scenario applies.
        var propertyType = CreatePropertyType(ContentVariation.CultureAndSegment);
        var languages = new Dictionary<int, ILanguage> { { 1, CreateLanguage("en-US") } };

        var result = PropertyDataCultureResolver.ResolveCulture(propertyType, languageId: null, languages);

        Assert.That(result.Culture, Is.Null);
        Assert.That(result.ShouldSkip, Is.False);
    }

    private static IPropertyType CreatePropertyType(ContentVariation variation)
    {
        var mock = new Mock<IPropertyType>();
        mock.Setup(pt => pt.Variations).Returns(variation);
        return mock.Object;
    }

    private static ILanguage CreateLanguage(string isoCode)
    {
        var mock = new Mock<ILanguage>();
        mock.Setup(l => l.IsoCode).Returns(isoCode);
        return mock.Object;
    }
}
