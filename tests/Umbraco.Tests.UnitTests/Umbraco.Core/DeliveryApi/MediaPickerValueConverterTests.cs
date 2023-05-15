using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class MediaPickerValueConverterTests : PropertyValueConverterTests
{
    [Test]
    public void MediaPickerValueConverter_InSingleMode_HasMultipleContentAsDeliveryApiType()
    {
        var publishedPropertyType = SetupMediaPropertyType(false);
        var valueConverter = CreateMediaPickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));
    }

    [Test]
    public void MediaPickerValueConverter_InSingleMode_ConvertsValueToDeliveryApiContent()
    {
        var publishedPropertyType = SetupMediaPropertyType(false);
        var valueConverter = CreateMediaPickerValueConverter();

        var inter = new[] {new GuidUdi(Constants.UdiEntityType.MediaType, PublishedMedia.Key)};

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiMedia>;

        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("The media", result.First().Name);
        Assert.AreEqual(PublishedMedia.Key, result.First().Id);
        Assert.AreEqual("the-media-url", result.First().Url);
        Assert.AreEqual("TheMediaType", result.First().MediaType);
    }

    [Test]
    public void MediaPickerValueConverter_InMultiMode_HasMultipleContentAsDeliveryApiType()
    {
        var publishedPropertyType = SetupMediaPropertyType(true);
        var valueConverter = CreateMediaPickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<IApiMedia>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));
    }

    [Test]
    public void MediaPickerValueConverter_InMultiMode_ConvertsValuesToDeliveryApiContent()
    {
        var publishedPropertyType = SetupMediaPropertyType(true);
        var valueConverter = CreateMediaPickerValueConverter();

        var otherMediaKey = Guid.NewGuid();
        var otherMedia = SetupPublishedContent("The other media", otherMediaKey, PublishedItemType.Media, PublishedMediaType);
        PublishedUrlProviderMock
            .Setup(p => p.GetMediaUrl(otherMedia.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns("the-other-media-url");
        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(otherMediaKey))
            .Returns(otherMedia.Object);

        var inter = new[] { new GuidUdi(Constants.UdiEntityType.MediaType, PublishedMedia.Key), new GuidUdi(Constants.UdiEntityType.MediaType, otherMediaKey) };

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false) as IEnumerable<IApiMedia>;

        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count());

        Assert.AreEqual("The media", result.First().Name);
        Assert.AreEqual(PublishedMedia.Key, result.First().Id);
        Assert.AreEqual("the-media-url", result.First().Url);
        Assert.AreEqual("TheMediaType", result.First().MediaType);

        Assert.AreEqual("The other media", result.Last().Name);
        Assert.AreEqual(otherMediaKey, result.Last().Id);
        Assert.AreEqual("the-other-media-url", result.Last().Url);
        Assert.AreEqual("TheMediaType", result.Last().MediaType);
    }

    private IPublishedPropertyType SetupMediaPropertyType(bool multiSelect)
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() =>
            new MediaPickerConfiguration {Multiple = multiSelect}
        ));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        return publishedPropertyType.Object;
    }

    private MediaPickerValueConverter CreateMediaPickerValueConverter() => new(
        PublishedSnapshotAccessor,
        Mock.Of<IPublishedModelFactory>(),
        new ApiMediaBuilder(
            new ApiContentNameProvider(),
            new ApiMediaUrlProvider(PublishedUrlProvider),
            Mock.Of<IPublishedValueFallback>(),
            CreateOutputExpansionStrategyAccessor()));
}
