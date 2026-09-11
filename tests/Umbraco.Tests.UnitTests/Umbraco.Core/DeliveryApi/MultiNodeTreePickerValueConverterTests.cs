using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HybridCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class MultiNodeTreePickerValueConverterTests : PropertyValueConverterTests
{
    private MultiNodeTreePickerValueConverter MultiNodeTreePickerValueConverter(IApiContentRouteBuilder? routeBuilder = null)
    {
        var expansionStrategyAccessor = CreateOutputExpansionStrategyAccessor();

        var contentNameProvider = new ApiContentNameProvider();
        var apiUrProvider = new ApiMediaUrlProvider(PublishedUrlProvider);
        routeBuilder ??= CreateContentRouteBuilder(ApiContentPathProvider, CreateGlobalSettings());
        return new MultiNodeTreePickerValueConverter(
            Mock.Of<IUmbracoContextAccessor>(),
            Mock.Of<IMemberService>(),
            new ApiContentBuilder(contentNameProvider, routeBuilder, expansionStrategyAccessor, CreateVariationContextAccessor()),
            new ApiMediaBuilder(contentNameProvider, apiUrProvider, Mock.Of<IPublishedValueFallback>(), expansionStrategyAccessor),
            CacheManager.Content,
            CacheManager.Media,
            CacheManager.Members);
    }

    private PublishedDataType MultiNodePickerPublishedDataType(bool multiSelect, string entityType) =>
        new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MultiNodePickerConfiguration
        {
            MaxNumber = multiSelect ? 10 : 1,
            TreeSource = new MultiNodePickerConfigurationTreeSource
            {
                ObjectType = entityType
            }
        }));

    [Test]
    public void MultiNodeTreePickerValueConverter_InSingleMode_ConvertsValueToListOfContent()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(false, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiContent>)));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo(PublishedContent.Name));
        Assert.That(result.First().Id, Is.EqualTo(PublishedContent.Key));
        Assert.That(result.First().Route.Path, Is.EqualTo("/the-page-url/"));
        Assert.That(result.First().ContentType, Is.EqualTo("TheContentType"));
        Assert.That(result.First().Properties, Is.Empty);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMode_ConvertsValueToListOfContent()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var otherContentKey = Guid.NewGuid();
        var otherContent = SetupPublishedContent("The other page", otherContentKey, PublishedItemType.Content, PublishedContentType);
        RegisterContentWithProviders(otherContent.Object, false);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiContent>)));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key), new GuidUdi(Constants.UdiEntityType.Document, otherContentKey) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));

        Assert.That(result.First().Name, Is.EqualTo(PublishedContent.Name));
        Assert.That(result.First().Id, Is.EqualTo(PublishedContent.Key));
        Assert.That(result.First().Route.Path, Is.EqualTo("/the-page-url/"));
        Assert.That(result.First().ContentType, Is.EqualTo("TheContentType"));

        Assert.That(result.Last().Name, Is.EqualTo("The other page"));
        Assert.That(result.Last().Id, Is.EqualTo(otherContentKey));
        Assert.That(result.Last().ContentType, Is.EqualTo("TheContentType"));
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InSingleMode_WithPreview_ConvertsValueToListOfContent()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(false, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiContent>)));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, DraftContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, true, false) as IEnumerable<IApiContent>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo(DraftContent.Name));
        Assert.That(result.First().Id, Is.EqualTo(DraftContent.Key));
        Assert.That(result.First().Route.Path, Is.EqualTo("/the-draft-page-url/"));
        Assert.That(result.First().ContentType, Is.EqualTo("TheContentType"));
        Assert.That(result.First().Properties, Is.Empty);
    }

    [Test]
    [TestCase(Constants.UdiEntityType.Document)]
    [TestCase("content")]
    public void MultiNodeTreePickerValueConverter_RendersContentProperties(string entityType)
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");
        contentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.ContentType).Returns(contentType.Object);

        var propertyData = new PropertyData { Value = "n/a", Culture = "abc", Segment = string.Empty };

        var prop1 = new PublishedProperty(DeliveryApiPropertyType, content.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);
        var prop2 = new PublishedProperty(DefaultPropertyType, content.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);

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

        var publishedDataType = MultiNodePickerPublishedDataType(false, entityType);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiContent>)));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo("The page"));
        Assert.That(result.First().Id, Is.EqualTo(key));
        Assert.That(result.First().Route.Path, Is.EqualTo("/page-url-segment/"));
        Assert.That(result.First().ContentType, Is.EqualTo("TheContentType"));
        Assert.That(result.First().Properties, Has.Count.EqualTo(2));
        Assert.That(result.First().Properties[DeliveryApiPropertyType.Alias], Is.EqualTo("Delivery API value"));
        Assert.That(result.First().Properties[DefaultPropertyType.Alias], Is.EqualTo("Default value"));
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InSingleMediaMode_ConvertsValueToListOfMedia()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(false, Constants.UdiEntityType.Media);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiMedia>)));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo(PublishedMedia.Name));
        Assert.That(result.First().Id, Is.EqualTo(PublishedMedia.Key));
        Assert.That(result.First().Url, Is.EqualTo("the-media-url"));
        Assert.That(result.First().MediaType, Is.EqualTo("TheMediaType"));
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMediaMode_ConvertsValueToListOfMedia()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Media);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var otherMediaKey = Guid.NewGuid();
        var otherMedia = SetupPublishedContent("The other media", otherMediaKey, PublishedItemType.Media, PublishedMediaType);
        RegisterMediaWithProviders(otherMedia.Object);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiMedia>)));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Media, otherMediaKey) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.First().Name, Is.EqualTo(PublishedMedia.Name));
        Assert.That(result.First().Id, Is.EqualTo(PublishedMedia.Key));
        Assert.That(result.First().Url, Is.EqualTo("the-media-url"));
        Assert.That(result.First().MediaType, Is.EqualTo("TheMediaType"));

        Assert.That(result.Last().Name, Is.EqualTo("The other media"));
        Assert.That(result.Last().Id, Is.EqualTo(otherMediaKey));
        Assert.That(result.Last().MediaType, Is.EqualTo("TheMediaType"));
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMode_WithMixedEntityTypes_OnlyConvertsConfiguredEntityType()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiContent>)));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo(PublishedContent.Name));
        Assert.That(result.First().Id, Is.EqualTo(PublishedContent.Key));
        Assert.That(result.First().Route.Path, Is.EqualTo("/the-page-url/"));
        Assert.That(result.First().ContentType, Is.EqualTo("TheContentType"));
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMediaMode_WithMixedEntityTypes_OnlyConvertsConfiguredEntityType()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Media);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiMedia>)));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        Assert.That(result.First().Name, Is.EqualTo(PublishedMedia.Name));
        Assert.That(result.First().Id, Is.EqualTo(PublishedMedia.Key));
        Assert.That(result.First().Url, Is.EqualTo("the-media-url"));
        Assert.That(result.First().MediaType, Is.EqualTo("TheMediaType"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MultiNodeTreePickerValueConverter_InMemberMode_IsUnsupported(bool multiSelect)
    {
        var publishedDataType = MultiNodePickerPublishedDataType(multiSelect, Constants.UdiEntityType.Member);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(string)));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as string;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo("(unsupported)"));
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiNodeTreePickerValueConverter_InSingleMode_ConvertsInvalidValueToEmptyArray(object? inter)
    {
        var publishedDataType = MultiNodePickerPublishedDataType(false, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiNodeTreePickerValueConverter_InMultiMode_ConvertsInvalidValueToEmptyArray(object? inter)
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_YieldsNothingForUnRoutableContent()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(false, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        // mocking the route builder will make it yield null values for all routes, so there is no need to setup anything on the mock
        var routeBuilder = new Mock<IApiContentRouteBuilder>();
        var valueConverter = MultiNodeTreePickerValueConverter(routeBuilder.Object);

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }
}
