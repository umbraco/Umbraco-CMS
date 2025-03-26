using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

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
            new ApiContentBuilder(contentNameProvider, routeBuilder, expansionStrategyAccessor),
            new ApiMediaBuilder(contentNameProvider, apiUrProvider, Mock.Of<IPublishedValueFallback>(), expansionStrategyAccessor),
            CacheManager.Content,
            CacheManager.Media,
            CacheManager.Members);
    }

    private PublishedDataType MultiNodePickerPublishedDataType(bool multiSelect, string entityType) =>
        new PublishedDataType(123, "test", new Lazy<object>(() => new MultiNodePickerConfiguration
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

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(PublishedContent.Name, result.First().Name);
        Assert.AreEqual(PublishedContent.Key, result.First().Id);
        Assert.AreEqual("/the-page-url", result.First().Route.Path);
        Assert.AreEqual("TheContentType", result.First().ContentType);
        Assert.IsEmpty(result.First().Properties);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMode_ConvertsValueToListOfContent()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var otherContentKey = Guid.NewGuid();
        var otherContent = SetupPublishedContent("The other page", otherContentKey, PublishedItemType.Content, PublishedContentType);
        RegisterContentWithProviders(otherContent.Object);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key), new GuidUdi(Constants.UdiEntityType.Document, otherContentKey) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count());

        Assert.AreEqual(PublishedContent.Name, result.First().Name);
        Assert.AreEqual(PublishedContent.Key, result.First().Id);
        Assert.AreEqual("/the-page-url", result.First().Route.Path);
        Assert.AreEqual("TheContentType", result.First().ContentType);

        Assert.AreEqual("The other page", result.Last().Name);
        Assert.AreEqual(otherContentKey, result.Last().Id);
        Assert.AreEqual("TheContentType", result.Last().ContentType);
    }

    [Test]
    [TestCase(Constants.UdiEntityType.Document)]
    [TestCase("content")]
    public void MultiNodeTreePickerValueConverter_RendersContentProperties(string entityType)
    {
        var content = new Mock<IPublishedContent>();

        var prop1 = new PublishedElementPropertyBase(DeliveryApiPropertyType, content.Object, false, PropertyCacheLevel.None, new VariationContext(), CacheManager);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None, new VariationContext(), CacheManager);

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

        var publishedDataType = MultiNodePickerPublishedDataType(false, entityType);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("The page", result.First().Name);
        Assert.AreEqual(key, result.First().Id);
        Assert.AreEqual("/page-url-segment", result.First().Route.Path);
        Assert.AreEqual("TheContentType", result.First().ContentType);
        Assert.AreEqual(2, result.First().Properties.Count);
        Assert.AreEqual("Delivery API value", result.First().Properties[DeliveryApiPropertyType.Alias]);
        Assert.AreEqual("Default value", result.First().Properties[DefaultPropertyType.Alias]);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InSingleMediaMode_ConvertsValueToListOfMedia()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(false, Constants.UdiEntityType.Media);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(PublishedMedia.Name, result.First().Name);
        Assert.AreEqual(PublishedMedia.Key, result.First().Id);
        Assert.AreEqual("the-media-url", result.First().Url);
        Assert.AreEqual("TheMediaType", result.First().MediaType);
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

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Media, otherMediaKey) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;
        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual(PublishedMedia.Name, result.First().Name);
        Assert.AreEqual(PublishedMedia.Key, result.First().Id);
        Assert.AreEqual("the-media-url", result.First().Url);
        Assert.AreEqual("TheMediaType", result.First().MediaType);

        Assert.AreEqual("The other media", result.Last().Name);
        Assert.AreEqual(otherMediaKey, result.Last().Id);
        Assert.AreEqual("TheMediaType", result.Last().MediaType);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMode_WithMixedEntityTypes_OnlyConvertsConfiguredEntityType()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(PublishedContent.Name, result.First().Name);
        Assert.AreEqual(PublishedContent.Key, result.First().Id);
        Assert.AreEqual("/the-page-url", result.First().Route.Path);
        Assert.AreEqual("TheContentType", result.First().ContentType);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMediaMode_WithMixedEntityTypes_OnlyConvertsConfiguredEntityType()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Media);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(PublishedMedia.Name, result.First().Name);
        Assert.AreEqual(PublishedMedia.Key, result.First().Id);
        Assert.AreEqual("the-media-url", result.First().Url);
        Assert.AreEqual("TheMediaType", result.First().MediaType);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MultiNodeTreePickerValueConverter_InMemberMode_IsUnsupported(bool multiSelect)
    {
        var publishedDataType = MultiNodePickerPublishedDataType(multiSelect, Constants.UdiEntityType.Member);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(string), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as string;
        Assert.NotNull(result);
        Assert.AreEqual("(unsupported)", result);
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
        Assert.NotNull(result);
        Assert.IsEmpty(result);
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
        Assert.NotNull(result);
        Assert.IsEmpty(result);
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
        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }
}
