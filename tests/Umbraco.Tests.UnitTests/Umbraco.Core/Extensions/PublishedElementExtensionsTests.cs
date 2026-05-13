// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class PublishedElementExtensionsTests
{
    [Test]
    public void HasCulture_Returns_True_When_Culture_Exists()
    {
        var element = CreateElement(cultureCodes: ["en-US", "da-DK"]);

        Assert.IsTrue(element.Object.HasCulture("en-US"));
    }

    [Test]
    public void HasCulture_Returns_False_When_Culture_Missing()
    {
        var element = CreateElement(cultureCodes: ["en-US"]);

        Assert.IsFalse(element.Object.HasCulture("da-DK"));
    }

    [Test]
    public void HasCulture_Is_Case_Insensitive()
    {
        var element = CreateElement(cultureCodes: ["en-US"]);

        Assert.IsTrue(element.Object.HasCulture("EN-us"));
    }

    [Test]
    public void HasCulture_Null_Checks_Invariant()
    {
        var element = CreateElement(cultureCodes: [string.Empty]);

        Assert.IsTrue(element.Object.HasCulture(null));
    }

    [Test]
    public void IsInvariantOrHasCulture_Returns_True_When_Invariant()
    {
        var element = CreateElement(variation: ContentVariation.Nothing);

        Assert.IsTrue(element.Object.IsInvariantOrHasCulture("en-US"));
    }

    [Test]
    public void IsInvariantOrHasCulture_Returns_True_When_Variant_And_Has_Culture()
    {
        var element = CreateElement(variation: ContentVariation.Culture, cultureCodes: ["en-US", "da-DK"]);

        Assert.IsTrue(element.Object.IsInvariantOrHasCulture("en-US"));
    }

    [Test]
    public void IsInvariantOrHasCulture_Returns_False_When_Variant_And_Missing_Culture()
    {
        var element = CreateElement(variation: ContentVariation.Culture, cultureCodes: ["en-US"]);

        Assert.IsFalse(element.Object.IsInvariantOrHasCulture("da-DK"));
    }

    [Test]
    public void CultureDate_Returns_UpdateDate_When_Invariant()
    {
        var updateDate = new DateTime(2025, 6, 15);
        var element = CreateElement(variation: ContentVariation.Nothing);
        element.Setup(x => x.UpdateDate).Returns(updateDate);

        var variationContextAccessor = new TestVariationContextAccessor();

        Assert.AreEqual(updateDate, element.Object.CultureDate(variationContextAccessor));
    }

    [Test]
    public void CultureDate_Returns_Culture_Date_When_Variant()
    {
        var cultureDate = new DateTime(2025, 7, 20);
        var element = CreateElement(variation: ContentVariation.Culture);
        element.Setup(x => x.Cultures).Returns(
            new Dictionary<string, PublishedCultureInfo>
            {
                { "en-US", new PublishedCultureInfo("en-US", "Home", "home", cultureDate) },
            });

        var variationContextAccessor = new TestVariationContextAccessor();

        Assert.AreEqual(cultureDate, element.Object.CultureDate(variationContextAccessor, "en-US"));
    }

    [Test]
    public void CultureDate_Returns_MinValue_When_Culture_Missing()
    {
        var element = CreateElement(variation: ContentVariation.Culture, cultureCodes: ["en-US"]);

        var variationContextAccessor = new TestVariationContextAccessor();

        Assert.AreEqual(DateTime.MinValue, element.Object.CultureDate(variationContextAccessor, "da-DK"));
    }

    [Test]
    public void CultureDate_Uses_Context_Culture_When_Null()
    {
        var expectedDate = new DateTime(2025, 8, 10);
        var otherDate = new DateTime(2020, 1, 1);
        var element = CreateElement(variation: ContentVariation.Culture);
        element.Setup(x => x.Cultures).Returns(
            new Dictionary<string, PublishedCultureInfo>
            {
                { "en-US", new PublishedCultureInfo("en-US", "Home", "home", expectedDate) },
                { "da-DK", new PublishedCultureInfo("da-DK", "Hjem", "hjem", otherDate) },
            });

        var variationContextAccessor = new TestVariationContextAccessor
        {
            VariationContext = new VariationContext("en-US"),
        };

        Assert.AreEqual(expectedDate, element.Object.CultureDate(variationContextAccessor));
    }

    [Test]
    public void IsDocumentType_Returns_True_When_Alias_Matches()
    {
        var element = CreateElement(contentTypeAlias: "myElement");

        Assert.IsTrue(element.Object.IsDocumentType("myElement"));
    }

    [Test]
    public void IsDocumentType_Is_Case_Insensitive()
    {
        var element = CreateElement(contentTypeAlias: "myElement");

        Assert.IsTrue(element.Object.IsDocumentType("MYELEMENT"));
    }

    [Test]
    public void IsDocumentType_Returns_False_When_Alias_Does_Not_Match()
    {
        var element = CreateElement(contentTypeAlias: "myElement");

        Assert.IsFalse(element.Object.IsDocumentType("otherElement"));
    }

    [Test]
    public void IsDocumentType_Recursive_Returns_True_When_Composed_Of()
    {
        var element = CreateElement(contentTypeAlias: "myElement", compositionAliases: ["baseElement"]);

        Assert.IsTrue(element.Object.IsDocumentType("baseElement", true));
    }

    [Test]
    public void IsDocumentType_Recursive_False_Does_Not_Check_Composition()
    {
        var element = CreateElement(contentTypeAlias: "myElement", compositionAliases: ["baseElement"]);

        Assert.IsFalse(element.Object.IsDocumentType("baseElement", false));
    }

    [Test]
    public void IsEqual_Returns_True_When_Same_Id()
    {
        var element1 = CreateElement();
        element1.Setup(x => x.Id).Returns(123);
        var element2 = CreateElement();
        element2.Setup(x => x.Id).Returns(123);

        Assert.IsTrue(element1.Object.IsEqual(element2.Object));
    }

    [Test]
    public void IsEqual_Returns_False_When_Different_Id()
    {
        var element1 = CreateElement();
        element1.Setup(x => x.Id).Returns(123);
        var element2 = CreateElement();
        element2.Setup(x => x.Id).Returns(456);

        Assert.IsFalse(element1.Object.IsEqual(element2.Object));
    }

    [Test]
    public void IsNotEqual_Returns_True_When_Different_Id()
    {
        var element1 = CreateElement();
        element1.Setup(x => x.Id).Returns(123);
        var element2 = CreateElement();
        element2.Setup(x => x.Id).Returns(456);

        Assert.IsTrue(element1.Object.IsNotEqual(element2.Object));
    }

    [Test]
    public void IsNotEqual_Returns_False_When_Same_Id()
    {
        var element1 = CreateElement();
        element1.Setup(x => x.Id).Returns(123);
        var element2 = CreateElement();
        element2.Setup(x => x.Id).Returns(123);

        Assert.IsFalse(element1.Object.IsNotEqual(element2.Object));
    }

    [Test]
    public void GetCreatorName_Returns_Name_When_User_Found()
    {
        var element = CreateElement();
        element.Setup(x => x.CreatorId).Returns(10);

        var profile = new Mock<IProfile>(MockBehavior.Strict);
        profile.Setup(x => x.Name).Returns("Admin");

        var userService = new Mock<IUserService>(MockBehavior.Strict);
        userService.Setup(x => x.GetProfileById(10)).Returns(profile.Object);

        Assert.AreEqual("Admin", element.Object.GetCreatorName(userService.Object));
    }

    [Test]
    public void GetCreatorName_Returns_Empty_When_User_Not_Found()
    {
        var element = CreateElement();
        element.Setup(x => x.CreatorId).Returns(99);

        var userService = new Mock<IUserService>(MockBehavior.Strict);
        userService.Setup(x => x.GetProfileById(99)).Returns((IProfile?)null);

        Assert.AreEqual(string.Empty, element.Object.GetCreatorName(userService.Object));
    }

    [Test]
    public void GetWriterName_Returns_Name_When_User_Found()
    {
        var element = CreateElement();
        element.Setup(x => x.WriterId).Returns(20);

        var profile = new Mock<IProfile>(MockBehavior.Strict);
        profile.Setup(x => x.Name).Returns("Editor");

        var userService = new Mock<IUserService>(MockBehavior.Strict);
        userService.Setup(x => x.GetProfileById(20)).Returns(profile.Object);

        Assert.AreEqual("Editor", element.Object.GetWriterName(userService.Object));
    }

    [Test]
    public void GetWriterName_Returns_Empty_When_User_Not_Found()
    {
        var element = CreateElement();
        element.Setup(x => x.WriterId).Returns(99);

        var userService = new Mock<IUserService>(MockBehavior.Strict);
        userService.Setup(x => x.GetProfileById(99)).Returns((IProfile?)null);

        Assert.AreEqual(string.Empty, element.Object.GetWriterName(userService.Object));
    }

    [Test]
    public void HasValue_With_Fallback_Returns_True_When_Property_Has_Value()
    {
        var property = new Mock<IPublishedProperty>(MockBehavior.Strict);
        property.Setup(x => x.HasValue(null, null)).Returns(true);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.GetProperty("prop1")).Returns(property.Object);

        var fallback = new Mock<IPublishedValueFallback>(MockBehavior.Strict);

        Assert.IsTrue(element.Object.HasValue(fallback.Object, "prop1"));
    }

    [Test]
    public void HasValue_With_Fallback_Returns_True_When_Fallback_Provides_Value()
    {
        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.GetProperty("prop1")).Returns((IPublishedProperty?)null);

        var fallback = new Mock<IPublishedValueFallback>(MockBehavior.Strict);
        object? outValue;
        fallback
            .Setup(x => x.TryGetValue(element.Object, "prop1", null, null, It.IsAny<Fallback>(), null, out outValue))
            .Returns(true);

        Assert.IsTrue(element.Object.HasValue(fallback.Object, "prop1"));
    }

    [Test]
    public void HasValue_With_Fallback_Returns_False_When_No_Value_And_No_Fallback()
    {
        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.GetProperty("prop1")).Returns((IPublishedProperty?)null);

        var fallback = new Mock<IPublishedValueFallback>(MockBehavior.Strict);
        object? outValue;
        fallback
            .Setup(x => x.TryGetValue(element.Object, "prop1", null, null, It.IsAny<Fallback>(), null, out outValue))
            .Returns(false);

        Assert.IsFalse(element.Object.HasValue(fallback.Object, "prop1"));
    }

    private static Mock<IPublishedElement> CreateElement(
        string? contentTypeAlias = null,
        ContentVariation variation = ContentVariation.Nothing,
        string[]? cultureCodes = null,
        HashSet<string>? compositionAliases = null)
    {
        var contentType = new Mock<IPublishedContentType>(MockBehavior.Strict);
        contentType.Setup(x => x.Variations).Returns(variation);

        if (contentTypeAlias != null)
        {
            contentType.Setup(x => x.Alias).Returns(contentTypeAlias);
        }

        if (compositionAliases != null)
        {
            contentType.Setup(x => x.CompositionAliases).Returns(compositionAliases);
        }

        var cultures = cultureCodes?.ToDictionary(
            c => c,
            c => new PublishedCultureInfo(c, "Name", "name", DateTime.Now),
            StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, PublishedCultureInfo>(StringComparer.OrdinalIgnoreCase);

        var element = new Mock<IPublishedElement>(MockBehavior.Strict);
        element.Setup(x => x.ContentType).Returns(contentType.Object);
        element.Setup(x => x.Cultures).Returns(cultures);

        return element;
    }
}
