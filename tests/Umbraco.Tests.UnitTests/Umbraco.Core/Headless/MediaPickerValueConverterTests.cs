using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Headless;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Headless;

[TestFixture]
public class MediaPickerValueConverterTests : PropertyValueConverterTests
{
    [Test]
    public void MediaPickerValueConverter_InSingleMode_HasSingleContentAsHeadlessType()
    {
        var publishedPropertyType = SetupMediaPropertyType(false);
        var valueConverter = CreateMediaPickerValueConverter();

        Assert.AreEqual(typeof(HeadlessMedia), valueConverter.GetHeadlessPropertyValueType(publishedPropertyType));
    }

    [Test]
    public void MediaPickerValueConverter_InSingleMode_ConvertsValueToHeadlessContent()
    {
        var publishedPropertyType = SetupMediaPropertyType(false);
        var valueConverter = CreateMediaPickerValueConverter();

        var inter = new[] {new GuidUdi(Constants.UdiEntityType.MediaType, PublishedMedia.Key)};

        var result = valueConverter.ConvertIntermediateToHeadlessObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false) as HeadlessMedia;

        Assert.NotNull(result);
        Assert.AreEqual("The media", result.Name);
        Assert.AreEqual(PublishedMedia.Key, result.Key);
        Assert.AreEqual("the-media-url", result.Url);
        Assert.AreEqual("TheMediaType", result.MediaType);
    }

    [Test]
    public void MediaPickerValueConverter_InMultiMode_HasMultipleContentAsHeadlessType()
    {
        var publishedPropertyType = SetupMediaPropertyType(true);
        var valueConverter = CreateMediaPickerValueConverter();

        Assert.AreEqual(typeof(IEnumerable<HeadlessMedia>), valueConverter.GetHeadlessPropertyValueType(publishedPropertyType));
    }

    [Test]
    public void MediaPickerValueConverter_InMultiMode_ConvertsValuesToHeadlessContent()
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

        var result = valueConverter.ConvertIntermediateToHeadlessObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false) as IEnumerable<HeadlessMedia>;

        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count());

        Assert.AreEqual("The media", result.First().Name);
        Assert.AreEqual(PublishedMedia.Key, result.First().Key);
        Assert.AreEqual("the-media-url", result.First().Url);
        Assert.AreEqual("TheMediaType", result.First().MediaType);

        Assert.AreEqual("The other media", result.Last().Name);
        Assert.AreEqual(otherMediaKey, result.Last().Key);
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
        PublishedUrlProvider,
        Mock.Of<IPublishedValueFallback>(),
        new HeadlessContentNameProvider());
}
