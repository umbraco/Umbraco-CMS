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
    /// <summary>
    /// Regression test for <see href="https://github.com/umbraco/Umbraco-CMS/issues/22759">issue #22759</see>.
    /// PR #22219 caused <c>PublishedContentExtensions.Value&lt;T&gt;(IPublishedContent, ...)</c> to forward
    /// <see cref="Fallback.ToAncestors"/> into the property-level fallback when the property exists with no value
    /// and no ancestor holds a value either. The property-level fallback would explicitly throw
    /// <see cref="System.NotSupportedException"/> for the Ancestors code, breaking a common user scenario that
    /// worked silently in 17.3.x.
    /// </summary>
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

    /// <summary>
    /// Element-level counterpart to the property-level regression test for
    /// <see href="https://github.com/umbraco/Umbraco-CMS/issues/22759">issue #22759</see>: directly invoking the
    /// <see cref="IPublishedElement"/> overload with <see cref="Fallback.ToAncestors"/> must not throw when the
    /// property has no value, since the Ancestors policy is tree-aware and only meaningful at
    /// <see cref="IPublishedContent"/> level.
    /// </summary>
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

    /// <summary>
    /// The property/element-level handling of <see cref="Fallback.Ancestors"/> is a <c>continue</c> rather than a
    /// short-circuit return. This test locks that in: a chained policy of Ancestors then DefaultValue should skip
    /// Ancestors at the property level and resolve via DefaultValue.
    /// </summary>
    [Test]
    public void TryGetValue_For_Property_With_Chained_Ancestors_Then_DefaultValue_Resolves_Via_Default_Value()
    {
        var fallback = CreateFallback();
        var property = CreateEmptyProperty();

        var result = fallback.TryGetValue<string>(property, culture: null, segment: null, Fallback.To(Fallback.Ancestors, Fallback.DefaultValue), defaultValue: "fallback-default", out var value);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual("fallback-default", value);
        });
    }

    /// <summary>
    /// Element-level counterpart to the property-level chained-fallback test: confirms that
    /// <see cref="Fallback.Ancestors"/> falls through to a subsequent <see cref="Fallback.DefaultValue"/> rather
    /// than short-circuiting the chain at element level.
    /// </summary>
    [Test]
    public void TryGetValue_For_Element_With_Chained_Ancestors_Then_DefaultValue_Resolves_Via_Default_Value()
    {
        var fallback = CreateFallback();
        var element = CreateElementWithEmptyProperty(out _);

        var result = fallback.TryGetValue<string>(element, "title", culture: null, segment: null, Fallback.To(Fallback.Ancestors, Fallback.DefaultValue), defaultValue: "fallback-default", out var value);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result);
            Assert.AreEqual("fallback-default", value);
        });
    }

    private static PublishedValueFallback CreateFallback() =>
        new(
ServiceContext.CreatePartial(languageService: Mock.Of<ILanguageService>()),
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
