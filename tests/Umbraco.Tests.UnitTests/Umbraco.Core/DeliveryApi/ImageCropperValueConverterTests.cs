using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ImageCropperValueConverterTests : PropertyValueConverterTests
{
    [Test]
    public void ImageCropperValueConverter_ConvertsValueToImageCropperValue()
    {
        var publishedDataType = new PublishedDataType(123, "test", "test", new Lazy<object>(() => new ImageCropperConfiguration
        {
            Crops = new ImageCropperConfiguration.Crop[]
            {
                new ImageCropperConfiguration.Crop
                {
                    Alias = "one", Width = 200, Height = 100
                }
            }
        }));
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.DataType).Returns(publishedDataType);

        var valueConverter = new ImageCropperValueConverter(Mock.Of<ILogger<ImageCropperValueConverter>>(), new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(ApiImageCropperValue)));

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var source = serializer.Serialize(new ImageCropperValue
            {
                Src = "/some/file.jpg",
                Crops = new[]
                {
                    new ImageCropperValue.ImageCropperCrop
                    {
                        Alias = "one",
                        Coordinates = new ImageCropperValue.ImageCropperCropCoordinates { X1 = 1m, X2 = 2m, Y1 = 10m, Y2 = 20m }
                    }
                },
                FocalPoint = new ImageCropperValue.ImageCropperFocalPoint { Left = .2m, Top = .4m }
            });
        var inter = valueConverter.ConvertSourceToIntermediate(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, source, false);
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(Mock.Of<IPublishedElement>(), publishedPropertyType.Object, PropertyCacheLevel.Element, inter, false, false) as ApiImageCropperValue;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Url, Is.EqualTo("/some/file.jpg"));
        Assert.That(result.FocalPoint, Is.Not.Null);
        Assert.That(result.FocalPoint.Left, Is.EqualTo(.2m));
        Assert.That(result.FocalPoint.Top, Is.EqualTo(.4m));
        Assert.That(result.Crops, Is.Not.Null);
        Assert.That(result.Crops.Count(), Is.EqualTo(1));
        Assert.That(result.Crops.First().Alias, Is.EqualTo("one"));
        Assert.That(result.Crops.First().Height, Is.EqualTo(100));
        Assert.That(result.Crops.First().Width, Is.EqualTo(200));
        Assert.That(result.Crops.First().Coordinates, Is.Not.Null);
        Assert.That(result.Crops.First().Coordinates.X1, Is.EqualTo(1m));
        Assert.That(result.Crops.First().Coordinates.X2, Is.EqualTo(2m));
        Assert.That(result.Crops.First().Coordinates.Y1, Is.EqualTo(10m));
        Assert.That(result.Crops.First().Coordinates.Y2, Is.EqualTo(20m));
    }
}
