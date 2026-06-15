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
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IApiContent)));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key),
            false,
            false) as IApiContent;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("The page"));
        Assert.That(result.Id, Is.EqualTo(PublishedContent.Key));
        Assert.That(result.Route.Path, Is.EqualTo("/the-page-url/"));
        Assert.That(result.ContentType, Is.EqualTo("TheContentType"));
        Assert.That(result.Properties, Is.Empty);
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Custom name for: The page"));
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

        var prop1 = new PublishedProperty(DeliveryApiPropertyType, content.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);
        var prop2 = new PublishedProperty(DefaultPropertyType, content.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);

        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var key = Guid.NewGuid();
        var urlSegment = "page-url-segment";
        var name = "The page";
        ConfigurePublishedContentMock(content, key, name, PublishedContentType, new[] { prop1, prop2 });

        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(content.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(urlSegment);
        PublishedContentCacheMock
            .Setup(pcc => pcc.GetById(false, key))
            .Returns(content.Object);

        var valueConverter = CreateValueConverter();
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IApiContent)));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            new GuidUdi(Constants.UdiEntityType.Document, key),
            false,
            false) as IApiContent;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("The page"));
        Assert.That(result.Id, Is.EqualTo(content.Object.Key));
        Assert.That(result.Route.Path, Is.EqualTo("/page-url-segment/"));
        Assert.That(result.ContentType, Is.EqualTo("TheContentType"));
        Assert.That(result.Properties, Has.Count.EqualTo(2));
        Assert.That(result.Properties[DeliveryApiPropertyType.Alias], Is.EqualTo("Delivery API value"));
        Assert.That(result.Properties[DefaultPropertyType.Alias], Is.EqualTo("Default value"));
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("The page (draft)"));
        Assert.That(result.Id, Is.EqualTo(DraftContent.Key));
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

        Assert.That(result, Is.Null);
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<IPublishedContent>());
        Assert.That(((IPublishedContent)result).Key, Is.EqualTo(DraftContent.Key));
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

        Assert.That(result, Is.Null);
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

        Assert.That(result, Is.EqualTo(inter));
    }
}
