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

/// <summary>
/// Unit tests for the MultiNodeTreePickerValueConverter class.
/// </summary>
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

    /// <summary>
    /// Verifies that the <see cref="MultiNodeTreePickerValueConverter"/> correctly converts a value in single mode
    /// to a list containing a single <see cref="IApiContent"/> instance, and that the resulting content has the expected properties.
    /// </summary>
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
        Assert.AreEqual("/the-page-url/", result.First().Route.Path);
        Assert.AreEqual("TheContentType", result.First().ContentType);
        Assert.IsEmpty(result.First().Properties);
    }

    /// <summary>
    /// Tests that the MultiNodeTreePickerValueConverter correctly converts a value to a list of content items when in multi mode.
    /// </summary>
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

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key), new GuidUdi(Constants.UdiEntityType.Document, otherContentKey) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count());

        Assert.AreEqual(PublishedContent.Name, result.First().Name);
        Assert.AreEqual(PublishedContent.Key, result.First().Id);
        Assert.AreEqual("/the-page-url/", result.First().Route.Path);
        Assert.AreEqual("TheContentType", result.First().ContentType);

        Assert.AreEqual("The other page", result.Last().Name);
        Assert.AreEqual(otherContentKey, result.Last().Id);
        Assert.AreEqual("TheContentType", result.Last().ContentType);
    }

    /// <summary>
    /// Verifies that when the MultiNodeTreePickerValueConverter is used in single mode with preview enabled,
    /// it converts the intermediate value (a single Udi) into a list containing one <see cref="IApiContent"/> object
    /// with the expected properties (name, id, route, content type, and empty properties collection).
    /// </summary>
    [Test]
    public void MultiNodeTreePickerValueConverter_InSingleMode_WithPreview_ConvertsValueToListOfContent()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(false, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, DraftContent.Key) };
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, true, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(DraftContent.Name, result.First().Name);
        Assert.AreEqual(DraftContent.Key, result.First().Id);
        Assert.AreEqual("/the-draft-page-url/", result.First().Route.Path);
        Assert.AreEqual("TheContentType", result.First().ContentType);
        Assert.IsEmpty(result.First().Properties);
    }

    /// <summary>
    /// Verifies that the <see cref="MultiNodeTreePickerValueConverter"/> correctly renders content properties
    /// for a given entity type when used in the Delivery API context.
    /// </summary>
    /// <param name="entityType">The entity type to test, such as <c>document</c> or <c>content</c>.</param>
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
            .Setup(pcc => pcc.GetById(false, key))
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
        Assert.AreEqual("/page-url-segment/", result.First().Route.Path);
        Assert.AreEqual("TheContentType", result.First().ContentType);
        Assert.AreEqual(2, result.First().Properties.Count);
        Assert.AreEqual("Delivery API value", result.First().Properties[DeliveryApiPropertyType.Alias]);
        Assert.AreEqual("Default value", result.First().Properties[DefaultPropertyType.Alias]);
    }

    /// <summary>
    /// Tests that the MultiNodeTreePickerValueConverter correctly converts a value to a list of media items when in single media mode.
    /// </summary>
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

    /// <summary>
    /// Tests that the MultiNodeTreePickerValueConverter correctly converts values to a list of media items when in multi-media mode.
    /// </summary>
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

    /// <summary>
    /// Tests that the MultiNodeTreePickerValueConverter in multi mode with mixed entity types
    /// only converts the configured entity type correctly.
    /// </summary>
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
        Assert.AreEqual("/the-page-url/", result.First().Route.Path);
        Assert.AreEqual("TheContentType", result.First().ContentType);
    }

    /// <summary>
    /// Tests that in multi-media mode with mixed entity types, the converter only converts entities of the configured type.
    /// </summary>
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

    /// <summary>
    /// Verifies that the <see cref="MultiNodeTreePickerValueConverter"/> returns an unsupported string ("(unsupported)")
    /// when used in member mode, regardless of whether multiple selection is enabled.
    /// </summary>
    /// <param name="multiSelect">If set to <c>true</c>, multiple selection is enabled; otherwise, single selection is used.</param>
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

    /// <summary>
    /// Verifies that when the MultiNodeTreePickerValueConverter is in single mode, passing an invalid intermediate value results in an empty array being returned.
    /// </summary>
    /// <param name="inter">The intermediate value to be converted, which is expected to be invalid for the converter (e.g., non-UDI values or null).</param>
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

    /// <summary>
    /// Tests that the MultiNodeTreePickerValueConverter converts invalid values to an empty array when in multi mode.
    /// </summary>
    /// <param name="inter">The intermediate value to convert.</param>
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

    /// <summary>
    /// Tests that the MultiNodeTreePickerValueConverter yields no results for content that is unroutable.
    /// </summary>
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
