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
    private MultiNodeTreePickerValueConverter MultiNodeTreePickerValueConverter(IApiContentRouteBuilder? routeBuilder = null, IUmbracoContextAccessor? umbracoContextAccessor = null)
    {
        var expansionStrategyAccessor = CreateOutputExpansionStrategyAccessor();

        var contentNameProvider = new ApiContentNameProvider();
        var apiUrProvider = new ApiMediaUrlProvider(PublishedUrlProvider);
        routeBuilder ??= CreateContentRouteBuilder(ApiContentPathProvider, CreateGlobalSettings());
        return new MultiNodeTreePickerValueConverter(
            umbracoContextAccessor ?? Mock.Of<IUmbracoContextAccessor>(),
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
        Assert.AreEqual("/the-page-url/", result.First().Route.Path);
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

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMode_ReadsContentForTheDeliveryApiInOneBatch()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var otherContentKey = Guid.NewGuid();
        var otherContent = SetupPublishedContent("The other page", otherContentKey, PublishedItemType.Content, PublishedContentType);
        RegisterContentWithProviders(otherContent.Object, false);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Guid[] keys = [PublishedContent.Key, otherContentKey];
        PublishedContentCacheMock
            .Setup(c => c.GetByIds(false, It.IsAny<IReadOnlyCollection<Guid>>()))
            .Returns([PublishedContent, otherContent.Object]);
        var inter = keys.Select(key => new GuidUdi(Constants.UdiEntityType.Document, key)).ToArray<Udi>();
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiContent>;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, result?.Count());
            PublishedContentCacheMock.Verify(c => c.GetByIds(false, It.Is<IReadOnlyCollection<Guid>>(k => k.SequenceEqual(keys))), Times.Once);
            PublishedContentCacheMock.Verify(c => c.GetById(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never);
        });
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMediaMode_ReadsMediaForTheDeliveryApiInOneBatch()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Media);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter();

        Guid[] keys = [PublishedMedia.Key, PublishedMedia.Key];
        PublishedMediaCacheMock
            .Setup(c => c.GetByIds(It.IsAny<IReadOnlyCollection<Guid>>()))
            .Returns([PublishedMedia, PublishedMedia]);
        var inter = keys.Select(key => new GuidUdi(Constants.UdiEntityType.Media, key)).ToArray<Udi>();
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMedia>;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, result?.Count());
            PublishedMediaCacheMock.Verify(c => c.GetByIds(It.Is<IReadOnlyCollection<Guid>>(k => k.SequenceEqual(keys))), Times.Once);
            PublishedMediaCacheMock.Verify(c => c.GetById(It.IsAny<Guid>()), Times.Never);
        });
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_ReadsDocumentsInOneBatch()
    {
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        IPublishedContent[] documents = keys.Select(key => CreatePublishedItem(key, PublishedItemType.Content)).ToArray();
        PublishedContentCacheMock
            .Setup(c => c.GetByIds(false, It.IsAny<IReadOnlyCollection<Guid>>()))
            .Returns(documents);

        var result = ConvertToObject(Constants.UdiEntityType.Document, true, keys.Select(key => new GuidUdi(Constants.UdiEntityType.Document, key))) as IEnumerable<IPublishedContent>;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(documents));
            PublishedContentCacheMock.Verify(c => c.GetByIds(false, It.Is<IReadOnlyCollection<Guid>>(k => k.SequenceEqual(keys))), Times.Once);
            PublishedContentCacheMock.Verify(c => c.GetById(It.IsAny<bool>(), It.IsAny<Guid>()), Times.Never);
        });
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_ReadsMediaInOneBatch()
    {
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        IPublishedContent[] media = keys.Select(key => CreatePublishedItem(key, PublishedItemType.Media)).ToArray();
        PublishedMediaCacheMock
            .Setup(c => c.GetByIds(It.IsAny<IReadOnlyCollection<Guid>>()))
            .Returns(media);

        var result = ConvertToObject(Constants.UdiEntityType.Media, true, keys.Select(key => new GuidUdi(Constants.UdiEntityType.Media, key))) as IEnumerable<IPublishedContent>;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(media));
            PublishedMediaCacheMock.Verify(c => c.GetByIds(It.Is<IReadOnlyCollection<Guid>>(k => k.SequenceEqual(keys))), Times.Once);
            PublishedMediaCacheMock.Verify(c => c.GetById(It.IsAny<Guid>()), Times.Never);
        });
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_ConvertsDocumentsInTheOrderOfTheValueAndSkipsMissingOnes()
    {
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        IPublishedContent first = CreatePublishedItem(keys[0], PublishedItemType.Content);
        IPublishedContent third = CreatePublishedItem(keys[2], PublishedItemType.Content);
        PublishedContentCacheMock.Setup(c => c.GetById(false, keys[0])).Returns(first);
        PublishedContentCacheMock.Setup(c => c.GetById(false, keys[2])).Returns(third);

        var result = ConvertToObject(Constants.UdiEntityType.Document, true, keys.Select(key => new GuidUdi(Constants.UdiEntityType.Document, key))) as IEnumerable<IPublishedContent>;

        Assert.That(result, Is.EqualTo(new[] { first, third }));
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InSingleMode_ConvertsTheFirstDocumentThatExists()
    {
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        IPublishedContent second = CreatePublishedItem(keys[1], PublishedItemType.Content);
        IPublishedContent third = CreatePublishedItem(keys[2], PublishedItemType.Content);
        PublishedContentCacheMock.Setup(c => c.GetById(false, keys[1])).Returns(second);
        PublishedContentCacheMock.Setup(c => c.GetById(false, keys[2])).Returns(third);

        var result = ConvertToObject(Constants.UdiEntityType.Document, false, keys.Select(key => new GuidUdi(Constants.UdiEntityType.Document, key)));

        Assert.AreSame(second, result);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_ConvertsOnlyTheEntityTypeOfTheFirstItemThatExists()
    {
        // The first document does not exist, so the media is the first item found and decides the type; the document
        // that does exist after it is then left out.
        var missingDocumentKey = Guid.NewGuid();
        var mediaKey = Guid.NewGuid();
        var documentKey = Guid.NewGuid();
        IPublishedContent media = CreatePublishedItem(mediaKey, PublishedItemType.Media);
        PublishedMediaCacheMock.Setup(c => c.GetById(mediaKey)).Returns(media);
        PublishedContentCacheMock.Setup(c => c.GetById(false, documentKey)).Returns(CreatePublishedItem(documentKey, PublishedItemType.Content));

        GuidUdi[] udis =
        [
            new(Constants.UdiEntityType.Document, missingDocumentKey),
            new(Constants.UdiEntityType.Media, mediaKey),
            new(Constants.UdiEntityType.Document, documentKey),
        ];
        var result = ConvertToObject(Constants.UdiEntityType.Document, true, udis) as IEnumerable<IPublishedContent>;

        Assert.That(result, Is.EqualTo(new[] { media }));
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_DoesNotReadMediaWhenTheDocumentsDecideTheEntityType()
    {
        var documentKey = Guid.NewGuid();
        var mediaKey = Guid.NewGuid();
        IPublishedContent document = CreatePublishedItem(documentKey, PublishedItemType.Content);
        PublishedContentCacheMock.Setup(c => c.GetById(false, documentKey)).Returns(document);

        GuidUdi[] udis = [new(Constants.UdiEntityType.Document, documentKey), new(Constants.UdiEntityType.Media, mediaKey)];
        var result = ConvertToObject(Constants.UdiEntityType.Document, true, udis) as IEnumerable<IPublishedContent>;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(new[] { document }));
            PublishedMediaCacheMock.Verify(c => c.GetByIds(It.IsAny<IReadOnlyCollection<Guid>>()), Times.Never);
            PublishedMediaCacheMock.Verify(c => c.GetById(It.IsAny<Guid>()), Times.Never);
        });
    }

    private object? ConvertToObject(string entityType, bool multiSelect, IEnumerable<GuidUdi> udis)
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(MultiNodePickerPublishedDataType(multiSelect, entityType));
        publishedPropertyType.SetupGet(p => p.EditorAlias).Returns(Constants.PropertyEditors.Aliases.MultiNodeTreePicker);
        publishedPropertyType.SetupGet(p => p.Alias).Returns("pickedPages");

        IUmbracoContext? umbracoContext = Mock.Of<IUmbracoContext>();
        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out umbracoContext)).Returns(true);

        return MultiNodeTreePickerValueConverter(umbracoContextAccessor: umbracoContextAccessor.Object).ConvertIntermediateToObject(
            Mock.Of<IPublishedElement>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Snapshot,
            udis.ToArray<Udi>(),
            false);
    }

    private static IPublishedContent CreatePublishedItem(Guid key, PublishedItemType itemType)
    {
        var contentType = Mock.Of<IPublishedContentType>(x => x.ItemType == itemType);
        return Mock.Of<IPublishedContent>(x => x.Key == key && x.ContentType == contentType);
    }
}
