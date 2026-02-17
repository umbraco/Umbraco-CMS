using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.HybridCache;

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
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");
        contentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.ContentType).Returns(contentType.Object);

        var propertyData = new PropertyData { Value = "n/a", Culture = "abc", Segment = string.Empty };

        var prop1 = new PublishedProperty(DeliveryApiPropertyType, content.Object, CreateVariationContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);
        var prop2 = new PublishedProperty(DefaultPropertyType, content.Object, CreateVariationContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);

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
            .Setup(pcc => pcc.GetById(key))
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
}
