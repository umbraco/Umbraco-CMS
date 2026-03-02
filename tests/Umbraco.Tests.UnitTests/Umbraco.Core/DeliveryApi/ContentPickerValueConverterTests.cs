using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ContentPickerValueConverterTests : PropertyValueConverterTests
{
    private ContentPickerValueConverter CreateValueConverter(IApiContentNameProvider? nameProvider = null)
        => new ContentPickerValueConverter(
            PublishedContentCacheMock.Object,
            new ApiContentBuilder(
                nameProvider ?? new ApiContentNameProvider(),
                CreateContentRouteBuilder(ApiContentPathProvider, CreateGlobalSettings()),
                CreateOutputExpansionStrategyAccessor(),
                CreateVariationContextAccessor()));

    [Test]
    public void ContentPickerValueConverter_BuildsDeliveryApiOutput()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var valueConverter = CreateValueConverter();
        Assert.AreEqual(typeof(IApiContent), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
            false,
            false) as IApiContent;

        Assert.NotNull(result);
        Assert.AreEqual("The page", result.Name);
        Assert.AreEqual(PublishedContent.Key, result.Id);
        Assert.AreEqual("/the-page-url/", result.Route.Path);
        Assert.AreEqual("TheContentType", result.ContentType);
        Assert.IsEmpty(result.Properties);
    }

    [Test]
    public void ContentPickerValueConverter_CanCustomizeContentNameInDeliveryApiOutput()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var customNameProvider = new Mock<IApiContentNameProvider>();
        customNameProvider.Setup(n => n.GetName(PublishedContent)).Returns($"Custom name for: {PublishedContent.Name}");

        var valueConverter = CreateValueConverter(customNameProvider.Object);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
            false,
            false) as IApiContent;

        Assert.NotNull(result);
        Assert.AreEqual("Custom name for: The page", result.Name);
    }

    [Test]
    public void ContentPickerValueConverter_RendersContentProperties()
    {
        var content = new Mock<IPublishedContent>();

        var prop1 = new PublishedElementPropertyBase(DeliveryApiPropertyType, content.Object, false, PropertyCacheLevel.None, new VariationContext(), Mock.Of<ICacheManager>());
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None, new VariationContext(), Mock.Of<ICacheManager>());

        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var key = Guid.NewGuid();
        var urlSegment = "page-url-segment";
        var name = "The page";
        ConfigurePublishedContentMock(content, key, name, urlSegment, PublishedContentType, new[] { prop1, prop2 });

        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(content.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(content.Object.UrlSegment);
        PublishedContentCacheMock
            .Setup(pcc => pcc.GetById(false, key))
            .Returns(content.Object);

        var valueConverter = CreateValueConverter();
        Assert.AreEqual(typeof(IApiContent), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, key),
            false,
            false) as IApiContent;

        Assert.NotNull(result);
        Assert.AreEqual("The page", result.Name);
        Assert.AreEqual(content.Object.Key, result.Id);
        Assert.AreEqual("/page-url-segment/", result.Route.Path);
        Assert.AreEqual("TheContentType", result.ContentType);
        Assert.AreEqual(2, result.Properties.Count);
        Assert.AreEqual("Delivery API value", result.Properties[DeliveryApiPropertyType.Alias]);
        Assert.AreEqual("Default value", result.Properties[DefaultPropertyType.Alias]);
    }

    [Test]
    public void ContentPickerValueConverter_WithPreview_BuildsDeliveryApiOutputForDraftContent()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var valueConverter = CreateValueConverter();
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, DraftContent.Key),
            true,
            false) as IApiContent;

        Assert.NotNull(result);
        Assert.AreEqual("The page (draft)", result.Name);
        Assert.AreEqual(DraftContent.Key, result.Id);
    }

    [Test]
    public void ContentPickerValueConverter_WithoutPreview_ReturnsNullForDraftOnlyContent()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var valueConverter = CreateValueConverter();
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, DraftContent.Key),
            false,
            false);

        Assert.Null(result);
    }

    [Test]
    public void ContentPickerValueConverter_WithPreview_ConvertIntermediateToObject_ReturnsDraftContent()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var valueConverter = CreateValueConverter();
        var result = valueConverter.ConvertIntermediateToObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, DraftContent.Key),
            true);

        Assert.NotNull(result);
        Assert.IsInstanceOf<IPublishedContent>(result);
        Assert.AreEqual(DraftContent.Key, ((IPublishedContent)result).Key);
    }

    [Test]
    public void ContentPickerValueConverter_WithoutPreview_ConvertIntermediateToObject_ReturnsNullForDraftOnlyContent()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var valueConverter = CreateValueConverter();
        var result = valueConverter.ConvertIntermediateToObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, DraftContent.Key),
            false);

        Assert.Null(result);
    }

    [TestCase(Constants.Conventions.Content.InternalRedirectId)]
    [TestCase(Constants.Conventions.Content.Redirect)]
    public void ContentPickerValueConverter_ExcludedRoutingProperty_ReturnsRawIntermediateValue(string alias)
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns(alias);

        var inter = new GuidUdi(Constants.UdiEntityType.Document, Guid.NewGuid());

        var valueConverter = CreateValueConverter();
        var result = valueConverter.ConvertIntermediateToObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            inter,
            false);

        Assert.AreEqual(inter, result);
    }
}
