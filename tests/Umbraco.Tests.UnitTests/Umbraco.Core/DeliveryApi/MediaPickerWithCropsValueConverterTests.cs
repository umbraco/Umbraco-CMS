using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.DeliveryApi;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class MediaPickerWithCropsValueConverterTests : PropertyValueConverterTests
{
    private MediaPickerWithCropsValueConverter MediaPickerWithCropsValueConverter()
    {
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var publishedValueFallback = Mock.Of<IPublishedValueFallback>();
        var apiUrlProvider = new ApiMediaUrlProvider(PublishedUrlProvider);
        var apiMediaWithCropsBuilder = new ApiMediaWithCropsBuilder(
            new ApiMediaBuilder(
                new ApiContentNameProvider(),
                apiUrlProvider,
                publishedValueFallback,
                CreateOutputExpansionStrategyAccessor()),
            publishedValueFallback);
        return new MediaPickerWithCropsValueConverter(
            CacheManager.Media,
            PublishedUrlProvider,
            publishedValueFallback,
            serializer,
            apiMediaWithCropsBuilder);
    }

    [Test]
    public void MediaPickerWithCropsValueConverter_InSingleMode_ConvertsValueToCollectionOfApiMedia()
    {
        var publishedPropertyType = SetupMediaPropertyType(false);
        var mediaKey = SetupMedia("My media", ".jpg", 200, 400, "My alt text", 800);

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        var valueConverter = MediaPickerWithCropsValueConverter();
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType), Is.EqualTo(typeof(IEnumerable<IApiMediaWithCrops>)));

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

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMediaWithCrops>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var first = result.Single();
        ValidateMedia(first, "My media", "my-media", ".jpg", 200, 400, 800);
        ValidateFocalPoint(first.FocalPoint, .2m, .4m);
        Assert.That(first.Crops, Is.Not.Null);
        Assert.That(first.Crops.Count(), Is.EqualTo(1));
        ValidateCrop(first.Crops.First(), "one", 200, 100, 1m, 2m, 10m, 20m);
        Assert.That(first.Properties, Is.Not.Null);
        Assert.That(first.Properties, Has.Count.EqualTo(1));
        Assert.That(first.Properties["altText"], Is.EqualTo("My alt text"));
    }

    [Test]
    public void MediaPickerWithCropsValueConverter_InMultiMode_ConvertsValueToMedias()
    {
        var publishedPropertyType = SetupMediaPropertyType(true);
        var mediaKey1 = SetupMedia("My media", ".jpg", 200, 400, "My alt text", 800);
        var mediaKey2 = SetupMedia("My other media", ".png", 800, 600, "My other alt text", 200);

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        var valueConverter = MediaPickerWithCropsValueConverter();
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType), Is.EqualTo(typeof(IEnumerable<IApiMediaWithCrops>)));

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

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMediaWithCrops>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
        var first = result.First();
        var last = result.Last();

        ValidateMedia(first, "My media", "my-media", ".jpg", 200, 400, 800);
        ValidateFocalPoint(first.FocalPoint, .2m, .4m);
        Assert.That(first.Crops, Is.Not.Null);
        Assert.That(first.Crops.Count(), Is.EqualTo(1));
        ValidateCrop(first.Crops.First(), "one", 200, 100, 1m, 2m, 10m, 20m);
        Assert.That(first.Properties, Is.Not.Null);
        Assert.That(first.Properties, Has.Count.EqualTo(1));
        Assert.That(first.Properties["altText"], Is.EqualTo("My alt text"));

        ValidateMedia(last, "My other media", "my-other-media", ".png", 800, 600, 200);
        ValidateFocalPoint(last.FocalPoint, .8m, .6m);
        Assert.That(last.Crops, Is.Not.Null);
        Assert.That(last.Crops.Count(), Is.EqualTo(1));
        ValidateCrop(last.Crops.First(), "one", 200, 100, 40m, 20m, 2m, 1m);
        Assert.That(last.Properties, Is.Not.Null);
        Assert.That(last.Properties, Has.Count.EqualTo(1));
        Assert.That(last.Properties["altText"], Is.EqualTo("My other alt text"));
    }

    [Test]
    public void MediaPickerWithCropsValueConverter_MergesMediaCropsWithLocalCrops()
    {
        var publishedPropertyType = SetupMediaPropertyType(false);
        var mediaCrops = new ImageCropperValue
        {
            Crops = new[]
            {
                new ImageCropperValue.ImageCropperCrop
                {
                    Alias = "mediaOne",
                    Width = 111,
                    Height = 222,
                    Coordinates = new ImageCropperValue.ImageCropperCropCoordinates { X1 = 2m, X2 = 4m, Y1 = 20m, Y2 = 40m }
                }
            },
            FocalPoint = new ImageCropperValue.ImageCropperFocalPoint { Left = .9m, Top = .1m }
        };
        var mediaKey = SetupMedia("Some media", ".123", 123, 456, "My alt text", 789, mediaCrops);

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        var valueConverter = MediaPickerWithCropsValueConverter();
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType), Is.EqualTo(typeof(IEnumerable<IApiMediaWithCrops>)));

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
                }
            }
        });

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMediaWithCrops>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var mediaWithCrops = result.Single();
        ValidateMedia(mediaWithCrops, "Some media", "some-media", ".123", 123, 456, 789);

        // no local focal point, should revert to media focal point
        ValidateFocalPoint(mediaWithCrops.FocalPoint, .9m, .1m);

        // media crops should be merged with local crops
        Assert.That(mediaWithCrops.Crops, Is.Not.Null);
        Assert.That(mediaWithCrops.Crops.Count(), Is.EqualTo(2));

        // local crops should be first, media crops should be last
        ValidateCrop(mediaWithCrops.Crops.First(), "one", 200, 100, 1m, 2m, 10m, 20m);
        ValidateCrop(mediaWithCrops.Crops.Last(), "mediaOne", 111, 222, 2m, 4m, 20m, 40m);
    }

    [Test]
    public void MediaPickerWithCropsValueConverter_LocalCropsAndFocalPointTakesPrecedenceOverMediaCropsAndFocalPoint()
    {
        var publishedPropertyType = SetupMediaPropertyType(false);
        var mediaCrops = new ImageCropperValue
        {
            Crops = new[]
            {
                new ImageCropperValue.ImageCropperCrop
                {
                    Alias = "one",
                    Width = 111,
                    Height = 222,
                    Coordinates = new ImageCropperValue.ImageCropperCropCoordinates { X1 = 2m, X2 = 4m, Y1 = 20m, Y2 = 40m }
                }
            },
            FocalPoint = new ImageCropperValue.ImageCropperFocalPoint { Left = .9m, Top = .1m }
        };
        var mediaKey = SetupMedia("Some media", ".123", 123, 456, "My alt text", 789, mediaCrops);

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        var valueConverter = MediaPickerWithCropsValueConverter();
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType), Is.EqualTo(typeof(IEnumerable<IApiMediaWithCrops>)));

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
                FocalPoint = new ImageCropperValue.ImageCropperFocalPoint { Left = .2m, Top = .3m }
            }
        });

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMediaWithCrops>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(1));
        var mediaWithCrops = result.Single();
        ValidateMedia(mediaWithCrops, "Some media", "some-media", ".123", 123, 456, 789);

        // local focal point should take precedence over media focal point
        ValidateFocalPoint(mediaWithCrops.FocalPoint, .2m, .3m);

        // media crops should be discarded when merging with local crops (matching aliases, local ones take precedence)
        Assert.That(mediaWithCrops.Crops, Is.Not.Null);
        Assert.That(mediaWithCrops.Crops.Count(), Is.EqualTo(1));

        // local crops should be first, media crops should be last
        ValidateCrop(mediaWithCrops.Crops.First(), "one", 200, 100, 1m, 2m, 10m, 20m);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase(123)]
    [TestCase("123")]
    public void MediaPickerWithCropsValueConverter_InSingleMode_ConvertsInvalidValueToEmptyCollection(object inter)
    {
        var publishedPropertyType = SetupMediaPropertyType(false);

        var valueConverter = MediaPickerWithCropsValueConverter();

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMediaWithCrops>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase(123)]
    [TestCase("123")]
    public void MediaPickerWithCropsValueConverter_InMultiMode_ConvertsInvalidValueToEmptyCollection(object inter)
    {
        var publishedPropertyType = SetupMediaPropertyType(true);

        var valueConverter = MediaPickerWithCropsValueConverter();

        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false, false) as IEnumerable<IApiMediaWithCrops>;
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void MediaPickerWithCropsValueConverter_InSingleMode_ConvertsValueToStronglyTypedMediaWithCrops()
    {
        var publishedPropertyType = SetupMediaPropertyType(false);

        TestMediaModelOne? media = null;
        var mediaKey = SetupMedia("My media", ".jpg", 200, 400, "My alt text", 800, asModel: inner => media = new TestMediaModelOne(inner));

        var valueConverter = MediaPickerWithCropsValueConverter();
        var inter = SerializeMediaWithCropsDtos(mediaKey);

        var result = valueConverter.ConvertIntermediateToObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false);

        Assert.That(result.GetType(), Is.EqualTo(typeof(MediaWithCrops<TestMediaModelOne>)));
        Assert.That(((MediaWithCrops<TestMediaModelOne>)result).Content, Is.SameAs(media));
    }

    [Test]
    public void MediaPickerWithCropsValueConverter_InMultiMode_ConvertsEachValueToItsOwnStronglyTypedMediaWithCrops()
    {
        var publishedPropertyType = SetupMediaPropertyType(true);

        TestMediaModelOne? firstMedia = null;
        TestMediaModelTwo? secondMedia = null;
        var firstMediaKey = SetupMedia("First media", ".jpg", 200, 400, "First alt text", 800, asModel: inner => firstMedia = new TestMediaModelOne(inner));
        var secondMediaKey = SetupMedia("Second media", ".png", 300, 600, "Second alt text", 900, asModel: inner => secondMedia = new TestMediaModelTwo(inner));

        var valueConverter = MediaPickerWithCropsValueConverter();
        var inter = SerializeMediaWithCropsDtos(firstMediaKey, secondMediaKey);

        // convert twice; the first pass populates the constructor cache, the second one exercises it
        for (var iteration = 0; iteration < 2; iteration++)
        {
            var result = valueConverter.ConvertIntermediateToObject(Mock.Of<IPublishedElement>(), publishedPropertyType, PropertyCacheLevel.Element, inter, false) as IEnumerable<MediaWithCrops>;
            Assert.That(result, Is.Not.Null);

            var mediaWithCrops = result.ToArray();
            Assert.That(mediaWithCrops, Has.Length.EqualTo(2));

            Assert.That(mediaWithCrops[0].GetType(), Is.EqualTo(typeof(MediaWithCrops<TestMediaModelOne>)));
            Assert.That(mediaWithCrops[1].GetType(), Is.EqualTo(typeof(MediaWithCrops<TestMediaModelTwo>)));

            Assert.That(((MediaWithCrops<TestMediaModelOne>)mediaWithCrops[0]).Content, Is.SameAs(firstMedia));
            Assert.That(((MediaWithCrops<TestMediaModelTwo>)mediaWithCrops[1]).Content, Is.SameAs(secondMedia));
        }
    }

    private string SerializeMediaWithCropsDtos(params Guid[] mediaKeys)
    {
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        return serializer.Serialize(mediaKeys.Select(mediaKey =>
            new MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.MediaWithCropsDto
            {
                Key = Guid.NewGuid(),
                MediaKey = mediaKey,
                Crops = Array.Empty<ImageCropperValue.ImageCropperCrop>(),
                FocalPoint = new ImageCropperValue.ImageCropperFocalPoint { Left = .2m, Top = .4m }
            }).ToArray());
    }

    private IPublishedPropertyType SetupMediaPropertyType(bool multiSelect)
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new MediaPicker3Configuration
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

    private Guid SetupMedia(string name, string extension, int width, int height, string altText, int bytes, ImageCropperValue? imageCropperValue = null, Func<IPublishedContent, IPublishedContent>? asModel = null)
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
        AddProperty(Constants.Conventions.Media.File, imageCropperValue);
        AddProperty("altText", altText);

        IPublishedContent mediaItem = asModel is null ? media.Object : asModel(media.Object);

        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(mediaKey))
            .Returns(mediaItem);
        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(It.IsAny<bool>(), mediaKey))
            .Returns(mediaItem);

        PublishedUrlProviderMock
            .Setup(p => p.GetMediaUrl(mediaItem, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(name.ToLowerInvariant().Replace(" ", "-"));

        return mediaKey;
    }

    private void ValidateMedia(
        IApiMediaWithCrops actual,
        string expectedName,
        string expectedUrl,
        string expectedExtension,
        int expectedWidth,
        int expectedHeight,
        int expectedBytes)
    {
        Assert.That(actual.Name, Is.EqualTo(expectedName));
        Assert.That(actual.Url, Is.EqualTo(expectedUrl));
        Assert.That(actual.Extension, Is.EqualTo(expectedExtension));
        Assert.That(actual.Width, Is.EqualTo(expectedWidth));
        Assert.That(actual.Height, Is.EqualTo(expectedHeight));
        Assert.That(actual.Bytes, Is.EqualTo(expectedBytes));

    }

    private void ValidateFocalPoint(ImageFocalPoint? actual, decimal expectedLeft, decimal expectedTop)
    {
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Left, Is.EqualTo(expectedLeft));
        Assert.That(actual.Top, Is.EqualTo(expectedTop));
    }

    private void ValidateCrop(
        ImageCrop actual,
        string expectedAlias,
        int expectedWidth,
        int expectedHeight,
        decimal expectedX1,
        decimal expectedX2,
        decimal expectedY1,
        decimal expectedY2)
    {
        Assert.That(actual.Alias, Is.EqualTo(expectedAlias));
        Assert.That(actual.Width, Is.EqualTo(expectedWidth));
        Assert.That(actual.Height, Is.EqualTo(expectedHeight));
        Assert.That(actual.Coordinates, Is.Not.Null);
        Assert.That(actual.Coordinates.X1, Is.EqualTo(expectedX1));
        Assert.That(actual.Coordinates.X2, Is.EqualTo(expectedX2));
        Assert.That(actual.Coordinates.Y1, Is.EqualTo(expectedY1));
        Assert.That(actual.Coordinates.Y2, Is.EqualTo(expectedY2));
    }

    // two distinct media model types, shaped like the models ModelsBuilder generates, so the converter
    // has to close MediaWithCrops<> over a different type per media item
    private sealed class TestMediaModelOne : PublishedContentWrapped
    {
        public TestMediaModelOne(IPublishedContent content)
            : base(content)
        {
        }
    }

    private sealed class TestMediaModelTwo : PublishedContentWrapped
    {
        public TestMediaModelTwo(IPublishedContent content)
            : base(content)
        {
        }
    }
}
