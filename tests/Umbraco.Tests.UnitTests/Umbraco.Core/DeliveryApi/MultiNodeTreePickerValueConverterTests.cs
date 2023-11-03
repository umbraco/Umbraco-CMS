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
        var umbracoContextAccessorMock = new Mock<IUmbracoContextAccessor>();
        var umbracoContext = Mock.Of<IUmbracoContext>();
        umbracoContextAccessorMock.Setup(m => m.TryGetUmbracoContext(out umbracoContext)).Returns(true);
        routeBuilder = routeBuilder ?? CreateContentRouteBuilder(PublishedUrlProvider, CreateGlobalSettings());
        return new MultiNodeTreePickerValueConverter(
            PublishedSnapshotAccessor,
            umbracoContextAccessorMock.Object,
            Mock.Of<IMemberService>(),
            new ApiContentBuilder(contentNameProvider, routeBuilder, expansionStrategyAccessor),
            new ApiMediaBuilder(contentNameProvider, apiUrProvider, Mock.Of<IPublishedValueFallback>(), expansionStrategyAccessor));
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

    private IPublishedPropertyType SetupMultiNodePickerPropertyType(bool multiSelect, string entityType)
    {
        var publishedDataType = MultiNodePickerPublishedDataType(multiSelect, entityType);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);
        publishedPropertyType.SetupGet(p => p.EditorAlias).Returns(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);
        return publishedPropertyType.Object;
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InSingleMode_ConvertsValueToListOfContent()
    {
        var publishedPropertyType = SetupMultiNodePickerPropertyType(false, Constants.UdiEntityType.Document);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var source = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key).ToString();
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
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
        var publishedPropertyType = SetupMultiNodePickerPropertyType(true, Constants.UdiEntityType.Document);

        var otherContentKey = Guid.NewGuid();
        var otherContent = SetupPublishedContent("The other page", otherContentKey, PublishedItemType.Content, PublishedContentType);
        RegisterContentWithProviders(otherContent.Object);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var source = $"{new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key)},{new GuidUdi(Constants.UdiEntityType.Document, otherContentKey)}";
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
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

        var prop1 = new PublishedElementPropertyBase(DeliveryApiPropertyType, content.Object, false, PropertyCacheLevel.None);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None);

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

        var publishedPropertyType = SetupMultiNodePickerPropertyType(false, entityType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var source = new GuidUdi(Constants.UdiEntityType.Document, key).ToString();
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
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
        var publishedPropertyType = SetupMultiNodePickerPropertyType(false, Constants.UdiEntityType.Media);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var source = new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key).ToString();
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;
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
        var publishedPropertyType = SetupMultiNodePickerPropertyType(true, Constants.UdiEntityType.Media);

        var otherMediaKey = Guid.NewGuid();
        var otherMedia = SetupPublishedContent("The other media", otherMediaKey, PublishedItemType.Media, PublishedMediaType);
        RegisterMediaWithProviders(otherMedia.Object);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var source = $"{new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key)},{new GuidUdi(Constants.UdiEntityType.Media, otherMediaKey)}";
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;
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
        var publishedPropertyType = SetupMultiNodePickerPropertyType(true, Constants.UdiEntityType.Document);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var source = $"{new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key)},{new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key)}";
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
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
        var publishedPropertyType = SetupMultiNodePickerPropertyType(true, Constants.UdiEntityType.Media);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var source = $"{new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key)},{new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key)}";
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;
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
        var publishedPropertyType = SetupMultiNodePickerPropertyType(multiSelect, Constants.UdiEntityType.Member);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(string), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var source = $"{new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key)},{new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key)}";
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as string;
        Assert.NotNull(result);
        Assert.AreEqual("(unsupported)", result);
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiNodeTreePickerValueConverter_InSingleMode_ConvertsInvalidValueToEmptyArray(object? source)
    {
        var publishedPropertyType = SetupMultiNodePickerPropertyType(false, Constants.UdiEntityType.Document);

        var valueConverter = MultiNodeTreePickerValueConverter();

        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }

    [TestCase(123)]
    [TestCase("123")]
    [TestCase(null)]
    public void MultiNodeTreePickerValueConverter_InMultiMode_ConvertsInvalidValueToEmptyArray(object? source)
    {
        var publishedPropertyType = SetupMultiNodePickerPropertyType(true, Constants.UdiEntityType.Document);

        var valueConverter = MultiNodeTreePickerValueConverter();

        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_YieldsNothingForUnRoutableContent()
    {
        var publishedPropertyType = SetupMultiNodePickerPropertyType(false, Constants.UdiEntityType.Document);

        // mocking the route builder will make it yield null values for all routes, so there is no need to setup anything on the mock
        var routeBuilder = new Mock<IApiContentRouteBuilder>();
        var valueConverter = MultiNodeTreePickerValueConverter(routeBuilder.Object);

        var source = new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key).ToString();
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }
}
