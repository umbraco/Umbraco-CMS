using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DeliveryApi.Accessors;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class MediaPickerWithCropsValueConverterTests : PropertyValueConverterTests
{
    private MediaPickerWithCropsValueConverter MediaPickerWithCropsValueConverter()
    {
        var serializer = new JsonNetSerializer();
        var publishedValueFallback = Mock.Of<IPublishedValueFallback>();
        var apiUrlProvider = new ApiMediaUrlProvider(PublishedUrlProvider);
        return new MediaPickerWithCropsValueConverter(
            PublishedSnapshotAccessor,
            PublishedUrlProvider,
            publishedValueFallback,
            serializer,
            new ApiMediaBuilder(
                new ApiContentNameProvider(),
                apiUrlProvider,
                Mock.Of<IPublishedValueFallback>(),
                CreateOutputExpansionStrategyAccessor()));
    }

    [Test]
    public void MediaPickerWithCropsValueConverter_InSingleMode_ConvertsValueToCollectionOfApiMedia()
    {
        var publishedPropertyType = SetupMediaPropertyType(false);
        var mediaKey = SetupMedia("My media", ".jpg", 200, 400, "My alt text", 800);

        var serializer = new JsonNetSerializer();

        var valueConverter = MediaPickerWithCropsValueConverter();
        Assert.AreEqual(typeof(IEnumerable<ApiMediaWithCrops>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var inter = serializer.Serialize(new[]
        {
            new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
            {
                Key = Guid.NewGuid(),
                MediaKey = mediaKey,
                Crops = new []
                {
                    new ImageCropperValue.ImageCropperCrop
                    {
                        Alias = "one",
                        Coordinates = new ImageCropperValue.ImageCropperCropCoordinates { X1 = 1m, X2 = 2m, Y1 = 10m, Y2 = 20m }
                    }
                },
                FocalPoint = new ImageCropperValue.ImageCropperFocalPoint { Left = .2m, Top = .4m }
            }
        });

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false) as IEnumerable<ApiMediaWithCrops>;
        Assert.NotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual("My media", result.First().Name);
        Assert.AreEqual("my-media", result.First().Url);
        Assert.AreEqual(".jpg", result.First().Extension);
        Assert.AreEqual(200, result.First().Width);
        Assert.AreEqual(400, result.First().Height);
        Assert.AreEqual(800, result.First().Bytes);
        Assert.NotNull(result.First().FocalPoint);
        Assert.AreEqual(".jpg", result.First().Extension);
        Assert.AreEqual(200, result.First().Width);
        Assert.AreEqual(400, result.First().Height);
        Assert.AreEqual(800, result.First().Bytes);
        Assert.AreEqual(.2m, result.First().FocalPoint.Left);
        Assert.AreEqual(.4m, result.First().FocalPoint.Top);
        Assert.NotNull(result.First().Crops);
        Assert.AreEqual(1, result.First().Crops.Count());
        Assert.AreEqual("one", result.First().Crops.First().Alias);
        Assert.AreEqual(100, result.First().Crops.First().Height);
        Assert.AreEqual(200, result.First().Crops.First().Width);
        Assert.NotNull(result.First().Crops.First().Coordinates);
        Assert.AreEqual(1m, result.First().Crops.First().Coordinates.X1);
        Assert.AreEqual(2m, result.First().Crops.First().Coordinates.X2);
        Assert.AreEqual(10m, result.First().Crops.First().Coordinates.Y1);
        Assert.AreEqual(20m, result.First().Crops.First().Coordinates.Y2);
        Assert.NotNull(result.First().Properties);
        Assert.AreEqual(1, result.First().Properties.Count);
        Assert.AreEqual("My alt text", result.First().Properties["altText"]);
    }

    [Test]
    public void MediaPickerWithCropsValueConverter_InMultiMode_ConvertsValueToMedias()
    {
        var publishedPropertyType = SetupMediaPropertyType(true);
        var mediaKey1 = SetupMedia("My media", ".jpg", 200, 400, "My alt text", 800);
        var mediaKey2 = SetupMedia("My other media", ".png", 800, 600, "My other alt text", 200);

        var serializer = new JsonNetSerializer();

        var valueConverter = MediaPickerWithCropsValueConverter();
        Assert.AreEqual(typeof(IEnumerable<ApiMediaWithCrops>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType));

        var inter = serializer.Serialize(new[]
        {
            new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
            {
                Key = Guid.NewGuid(),
                MediaKey = mediaKey1,
                Crops = new []
                {
                    new ImageCropperValue.ImageCropperCrop
                    {
                        Alias = "one",
                        Coordinates = new ImageCropperValue.ImageCropperCropCoordinates { X1 = 1m, X2 = 2m, Y1 = 10m, Y2 = 20m }
                    }
                },
                FocalPoint = new ImageCropperValue.ImageCropperFocalPoint { Left = .2m, Top = .4m }
            },
            new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
            {
                Key = Guid.NewGuid(),
                MediaKey = mediaKey2,
                Crops = new []
                {
                    new ImageCropperValue.ImageCropperCrop
                    {
                        Alias = "one",
                        Coordinates = new ImageCropperValue.ImageCropperCropCoordinates { X1 = 40m, X2 = 20m, Y1 = 2m, Y2 = 1m }
                    }
                },
                FocalPoint = new ImageCropperValue.ImageCropperFocalPoint { Left = .8m, Top = .6m }
            }
        });

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false) as IEnumerable<ApiMediaWithCrops>;
        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count());

        Assert.AreEqual("My media", result.First().Name);
        Assert.AreEqual("my-media", result.First().Url);
        Assert.AreEqual(".jpg", result.First().Extension);
        Assert.AreEqual(200, result.First().Width);
        Assert.AreEqual(400, result.First().Height);
        Assert.AreEqual(800, result.First().Bytes);
        Assert.NotNull(result.First().FocalPoint);
        Assert.AreEqual(.2m, result.First().FocalPoint.Left);
        Assert.AreEqual(.4m, result.First().FocalPoint.Top);
        Assert.NotNull(result.First().Crops);
        Assert.AreEqual(1, result.First().Crops.Count());
        Assert.AreEqual("one", result.First().Crops.First().Alias);
        Assert.AreEqual(100, result.First().Crops.First().Height);
        Assert.AreEqual(200, result.First().Crops.First().Width);
        Assert.NotNull(result.First().Crops.First().Coordinates);
        Assert.AreEqual(1m, result.First().Crops.First().Coordinates.X1);
        Assert.AreEqual(2m, result.First().Crops.First().Coordinates.X2);
        Assert.AreEqual(10m, result.First().Crops.First().Coordinates.Y1);
        Assert.AreEqual(20m, result.First().Crops.First().Coordinates.Y2);
        Assert.NotNull(result.First().Properties);
        Assert.AreEqual(1, result.First().Properties.Count);
        Assert.AreEqual("My alt text", result.First().Properties["altText"]);

        Assert.AreEqual("My other media", result.Last().Name);
        Assert.AreEqual("my-other-media", result.Last().Url);
        Assert.AreEqual(".png", result.Last().Extension);
        Assert.AreEqual(800, result.Last().Width);
        Assert.AreEqual(600, result.Last().Height);
        Assert.AreEqual(200, result.Last().Bytes);
        Assert.NotNull(result.Last().FocalPoint);
        Assert.AreEqual(.8m, result.Last().FocalPoint.Left);
        Assert.AreEqual(.6m, result.Last().FocalPoint.Top);
        Assert.NotNull(result.Last().Crops);
        Assert.AreEqual(1, result.Last().Crops.Count());
        Assert.AreEqual("one", result.Last().Crops.Last().Alias);
        Assert.AreEqual(100, result.Last().Crops.Last().Height);
        Assert.AreEqual(200, result.Last().Crops.Last().Width);
        Assert.NotNull(result.Last().Crops.Last().Coordinates);
        Assert.AreEqual(40m, result.Last().Crops.Last().Coordinates.X1);
        Assert.AreEqual(20m, result.Last().Crops.Last().Coordinates.X2);
        Assert.AreEqual(2m, result.Last().Crops.Last().Coordinates.Y1);
        Assert.AreEqual(1m, result.Last().Crops.Last().Coordinates.Y2);
        Assert.NotNull(result.Last().Properties);
        Assert.AreEqual(1, result.Last().Properties.Count);
        Assert.AreEqual("My other alt text", result.Last().Properties["altText"]);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase(123)]
    [TestCase("123")]
    public void MediaPickerWithCropsValueConverter_InSingleMode_ConvertsInvalidValueToEmptyCollection(object inter)
    {
        var publishedPropertyType = SetupMediaPropertyType(false);

        var serializer = new JsonNetSerializer();

        var valueConverter = MediaPickerWithCropsValueConverter();

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false) as IEnumerable<ApiMediaWithCrops>;
        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase(123)]
    [TestCase("123")]
    public void MediaPickerWithCropsValueConverter_InMultiMode_ConvertsInvalidValueToEmptyCollection(object inter)
    {
        var publishedPropertyType = SetupMediaPropertyType(true);

        var serializer = new JsonNetSerializer();

        var valueConverter = MediaPickerWithCropsValueConverter();

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false) as IEnumerable<ApiMediaWithCrops>;
        Assert.NotNull(result);
        Assert.IsEmpty(result);
    }


    private IPublishedPropertyType SetupMediaPropertyType(bool multiSelect)
    {
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new MediaPicker3Configuration
        {
            Multiple = multiSelect,
            EnableLocalFocalPoint = true,
            Crops = new MediaPicker3Configuration.CropConfiguration[]
            {
                new MediaPicker3Configuration.CropConfiguration
                {
                    Alias = "one", Width = 200, Height = 100
                }
            }
        }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        return publishedPropertyType.Object;
    }

    private Guid SetupMedia(string name, string extension, int width, int height, string altText, int bytes)
    {
        var publishedMediaType = new Mock<IPublishedContentType>();
        publishedMediaType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);

        var mediaKey = Guid.NewGuid();
        var media = SetupPublishedContent(name, mediaKey, PublishedItemType.Media, publishedMediaType.Object);
        var mediaProperties = new List<IPublishedProperty>();
        media.SetupGet(m => m.Properties).Returns(mediaProperties);

        void AddProperty(string alias, object value)
        {
            var property = new Mock<IPublishedProperty>();
            property.SetupGet(p => p.Alias).Returns(alias);
            property.Setup(p => p.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(true);
            property.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(value);
            property.Setup(p => p.GetDeliveryApiValue(It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<string?>())).Returns(value);
            media.Setup(m => m.GetProperty(alias)).Returns(property.Object);
            mediaProperties.Add(property.Object);
        }

        AddProperty(Constants.Conventions.Media.Extension, extension);
        AddProperty(Constants.Conventions.Media.Width, width);
        AddProperty(Constants.Conventions.Media.Height, height);
        AddProperty(Constants.Conventions.Media.Bytes, bytes);
        AddProperty("altText", altText);

        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(mediaKey))
            .Returns(media.Object);
        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(It.IsAny<bool>(), mediaKey))
            .Returns(media.Object);

        PublishedUrlProviderMock
            .Setup(p => p.GetMediaUrl(media.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(name.ToLowerInvariant().Replace(" ", "-"));

        return mediaKey;
    }
}
