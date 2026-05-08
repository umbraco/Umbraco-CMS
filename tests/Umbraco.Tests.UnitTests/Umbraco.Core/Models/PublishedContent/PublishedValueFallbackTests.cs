// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.PublishedContent;

[TestFixture]
public class PublishedValueFallbackTests
{
    // Regression test for https://github.com/umbraco/Umbraco-CMS/issues/22759.
    // PR #22219 caused PublishedContentExtensions.Value<T>(IPublishedContent, ...) to forward Fallback.ToAncestors into the property-level fallback
    // when the property exists with no value and no ancestor holds a value either. The property-level fallback would explicitly throw NotSupportedException
    // for the Ancestors code, breaking a common user scenario that worked silently in 17.3.x.
    [Test]
    public void TryGetValue_For_Property_With_Ancestors_Fallback_Does_Not_Throw_When_Property_Has_No_Value()
    {
        var fallback = CreateFallback();
        var property = CreateEmptyProperty();

        var result = fallback.TryGetValue<string>(property, culture: null, segment: null, Fallback.ToAncestors, defaultValue: null, out var value);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsNull(value);
        });
    }

    [Test]
    public void TryGetValue_For_Element_With_Ancestors_Fallback_Does_Not_Throw_When_Property_Has_No_Value()
    {
        var fallback = CreateFallback();
        var element = CreateElementWithEmptyProperty(out _);

        var result = fallback.TryGetValue<string>(element, "title", culture: null, segment: null, Fallback.ToAncestors, defaultValue: null, out var value);

        Assert.Multiple(() =>
        {
            Assert.IsFalse(result);
            Assert.IsNull(value);
        });
    }

    private static PublishedValueFallback CreateFallback() =>
        new(
            ServiceContext.CreatePartial(localizationService: Mock.Of<ILocalizationService>()),
            Mock.Of<IVariationContextAccessor>(),
            Mock.Of<IPropertyRenderingContextAccessor>());

    private static IPublishedProperty CreateEmptyProperty()
    {
        var propertyType = new Mock<IPublishedPropertyType>();
        propertyType.Setup(x => x.Variations).Returns(ContentVariation.Nothing);

        var property = new Mock<IPublishedProperty>();
        property.Setup(x => x.Alias).Returns("title");
        property.Setup(x => x.PropertyType).Returns(propertyType.Object);
        property.Setup(x => x.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(false);

        return property.Object;
    }

    private static IPublishedElement CreateElementWithEmptyProperty(out IPublishedProperty property)
    {
        property = CreateEmptyProperty();

        var contentType = new Mock<IPublishedContentType>();
        contentType.Setup(x => x.GetPropertyType("title")).Returns(property.PropertyType);

        var element = new Mock<IPublishedElement>();
        element.Setup(x => x.ContentType).Returns(contentType.Object);
        element.Setup(x => x.GetProperty("title")).Returns(property);

        return element.Object;
    }
}
