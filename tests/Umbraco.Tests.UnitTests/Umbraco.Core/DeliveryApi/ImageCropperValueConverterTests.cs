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
        var publishedDataType = new PublishedDataType(123, "test", new Lazy<object>(() => new ImageCropperConfiguration
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

        var valueConverter = new ImageCropperValueConverter(Mock.Of<ILogger<ImageCropperValueConverter>>(), new SystemTextJsonSerializer());
        Assert.AreEqual(typeof(ApiImageCropperValue), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));

        var serializer = new SystemTextJsonSerializer();
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
        Assert.NotNull(result);
        Assert.AreEqual("/some/file.jpg", result.Url);
        Assert.NotNull(result.FocalPoint);
        Assert.AreEqual(.2m, result.FocalPoint.Left);
        Assert.AreEqual(.4m, result.FocalPoint.Top);
        Assert.NotNull(result.Crops);
        Assert.AreEqual(1, result.Crops.Count());
        Assert.AreEqual("one", result.Crops.First().Alias);
        Assert.AreEqual(100, result.Crops.First().Height);
        Assert.AreEqual(200, result.Crops.First().Width);
        Assert.NotNull(result.Crops.First().Coordinates);
        Assert.AreEqual(1m, result.Crops.First().Coordinates.X1);
        Assert.AreEqual(2m, result.Crops.First().Coordinates.X2);
        Assert.AreEqual(10m, result.Crops.First().Coordinates.Y1);
        Assert.AreEqual(20m, result.Crops.First().Coordinates.Y2);
    }
}
