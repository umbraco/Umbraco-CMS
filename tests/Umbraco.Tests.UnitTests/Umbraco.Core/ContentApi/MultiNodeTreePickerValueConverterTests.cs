using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class MultiNodeTreePickerValueConverterTests : PropertyValueConverterTests
{
    private MultiNodeTreePickerValueConverter MultiNodeTreePickerValueConverter(bool shouldExpand)
    {
        var expansionStrategy = new Mock<IOutputExpansionStrategy>();
        expansionStrategy.Setup(e => e.ShouldExpand(It.IsAny<IPublishedPropertyType>())).Returns(shouldExpand);

        var contentNameProvider = new PublishedContentNameProvider();
        var propertyMapper = new PropertyMapper();
        return new MultiNodeTreePickerValueConverter(
            PublishedSnapshotAccessor,
            Mock.Of<IUmbracoContextAccessor>(),
            Mock.Of<IMemberService>(),
            new ApiContentBuilder(propertyMapper, contentNameProvider, PublishedUrlProvider),
            new ApiMediaBuilder(propertyMapper, contentNameProvider, PublishedUrlProvider, Mock.Of<IPublishedValueFallback>()),
            expansionStrategy.Object);
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

        var valueConverter = MultiNodeTreePickerValueConverter(false);

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(PublishedContent.Name, result.First().Name);
        Assert.AreEqual(PublishedContent.Key, result.First().Id);
        Assert.AreEqual("the-page-url", result.First().Url);
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
        PublishedContentCacheMock
            .Setup(pcc => pcc.GetById(otherContentKey))
            .Returns(otherContent.Object);

        var valueConverter = MultiNodeTreePickerValueConverter(false);

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key), new GuidUdi(Constants.UdiEntityType.Document, otherContentKey) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count());

        Assert.AreEqual(PublishedContent.Name, result.First().Name);
        Assert.AreEqual(PublishedContent.Key, result.First().Id);
        Assert.AreEqual("the-page-url", result.First().Url);
        Assert.AreEqual("TheContentType", result.First().ContentType);

        Assert.AreEqual("The other page", result.Last().Name);
        Assert.AreEqual(otherContentKey, result.Last().Id);
        Assert.AreEqual("TheContentType", result.Last().ContentType);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MultiNodeTreePickerValueConverter_HandlesContentExpansion(bool shouldExpand)
    {
        var content = new Mock<IPublishedContent>();

        var prop1 = new PublishedElementPropertyBase(ContentApiPropertyType, content.Object, false, PropertyCacheLevel.None);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None);

        var key = Guid.NewGuid();
        content.SetupGet(c => c.Properties).Returns(new[] { prop1, prop2 });
        content.SetupGet(c => c.UrlSegment).Returns("page-url-segment");
        content.SetupGet(c => c.Name).Returns("The page");
        content.SetupGet(c => c.Key).Returns(key);
        content.SetupGet(c => c.ContentType).Returns(PublishedContentType);
        content.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(content.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(content.Object.UrlSegment);
        PublishedContentCacheMock
            .Setup(pcc => pcc.GetById(key))
            .Returns(content.Object);

        var publishedDataType = MultiNodePickerPublishedDataType(false, Constants.UdiEntityType.Document);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter(shouldExpand);

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Document, key) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("The page", result.First().Name);
        Assert.AreEqual(key, result.First().Id);
        Assert.AreEqual("page-url-segment", result.First().Url);
        Assert.AreEqual("TheContentType", result.First().ContentType);
        if (shouldExpand)
        {
            Assert.AreEqual(2, result.First().Properties.Count);
        }
        else
        {
            Assert.IsEmpty(result.First().Properties);
        }
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InSingleMediaMode_ConvertsValueToListOfMedia()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(false, Constants.UdiEntityType.Media);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter(false);

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiMedia>;
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
        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(otherMediaKey))
            .Returns(otherMedia.Object);

        var valueConverter = MultiNodeTreePickerValueConverter(false);

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Media, otherMediaKey) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiMedia>;
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

        var valueConverter = MultiNodeTreePickerValueConverter(false);

        Assert.AreEqual(typeof(IEnumerable<IApiContent>), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(PublishedContent.Name, result.First().Name);
        Assert.AreEqual(PublishedContent.Key, result.First().Id);
        Assert.AreEqual("the-page-url", result.First().Url);
        Assert.AreEqual("TheContentType", result.First().ContentType);
    }

    [Test]
    public void MultiNodeTreePickerValueConverter_InMultiMediaMode_WithMixedEntityTypes_OnlyConvertsConfiguredEntityType()
    {
        var publishedDataType = MultiNodePickerPublishedDataType(true, Constants.UdiEntityType.Media);
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = MultiNodeTreePickerValueConverter(false);

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiMedia>;
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

        var valueConverter = MultiNodeTreePickerValueConverter(false);

        Assert.AreEqual(typeof(string), valueConverter.GetContentApiPropertyValueType(publishedPropertyType.Object));

        var inter = new Udi[] { new GuidUdi(Constants.UdiEntityType.Media, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.Document, PublishedContent.Key) };
        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as string;
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

        var valueConverter = MultiNodeTreePickerValueConverter(false);

        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiContent>;
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

        var valueConverter = MultiNodeTreePickerValueConverter(false);

        var result = valueConverter.ConvertIntermediateToContentApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiContent>;
        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }
}
