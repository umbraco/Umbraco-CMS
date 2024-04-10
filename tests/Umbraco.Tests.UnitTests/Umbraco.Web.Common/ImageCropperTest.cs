// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common;

[TestFixture]
public class ImageCropperTest
{
    private const string CropperJson1 =
        "{\"focalPoint\": {\"left\": 0.96,\"top\": 0.80827067669172936},\"src\": \"/media/1005/img_0671.jpg\",\"crops\": [{\"alias\":\"thumb\",\"width\": 100,\"height\": 100,\"coordinates\": {\"x1\": 0.58729977382575338,\"y1\": 0.055768992440203169,\"x2\": 0,\"y2\": 0.32457553600198386}}]}";

    private const string CropperJson2 =
        "{\"focalPoint\": {\"left\": 0.5,\"top\": 0.5},\"src\": \"/media/1005/img_0672.jpg\",\"crops\": []}";

    private const string MediaPath = "/media/1005/img_0671.jpg";

    [Test]
    public void CanConvertImageCropperDataSetSrcToString()
    {
        SetupJsonSerializerServiceProvider();

        // cropperJson3 - has no crops
        var cropperValue = CropperJson2.DeserializeImageCropperValue();
        var serialized = cropperValue.TryConvertTo<string>();
        Assert.IsTrue(serialized.Success);
        Assert.AreEqual("/media/1005/img_0672.jpg", serialized.Result);
    }

    [Test]
    public void CanConvertJsonStringToImageCropperValue()
    {
        SetupJsonSerializerServiceProvider();

        // cropperJson1 - has crops
        var cropperValue = CropperJson1.DeserializeImageCropperValue();
        Assert.AreEqual(MediaPath, cropperValue.Src);
        Assert.IsNotNull(cropperValue.FocalPoint);
        Assert.AreEqual(0.96m, cropperValue.FocalPoint.Left);
        Assert.AreEqual(0.80827067669172936m, cropperValue.FocalPoint.Top);
        Assert.IsNotNull(cropperValue.Crops);
        Assert.AreEqual(1, cropperValue.Crops.Count());
        var crop = cropperValue.Crops.First();
        Assert.AreEqual("thumb", crop.Alias);
        Assert.AreEqual(100, crop.Width);
        Assert.AreEqual(100, crop.Height);
        Assert.IsNotNull(crop.Coordinates);
        Assert.AreEqual(0.58729977382575338m, crop.Coordinates.X1);
        Assert.AreEqual(0.055768992440203169m, crop.Coordinates.Y1);
        Assert.AreEqual(0m, crop.Coordinates.X2);
        Assert.AreEqual(0.32457553600198386m, crop.Coordinates.Y2);
    }

    [Test]
    public void CanConvertImageCropperDataSetJsonToString()
    {
        SetupJsonSerializerServiceProvider();

        var cropperValue = CropperJson1.DeserializeImageCropperValue();
        var serialized = cropperValue.TryConvertTo<string>();
        Assert.IsTrue(serialized.Success);
        Assert.AreEqual("/media/1005/img_0671.jpg", serialized.Result);
    }

    // [TestCase(CropperJson1, CropperJson1, true)]
    // [TestCase(CropperJson1, CropperJson2, false)]
    // public void CanConvertImageCropperPropertyEditor(string val1, string val2, bool expected)
    // {
    //     try
    //     {
    //         var container = RegisterFactory.Create();
    //         var composition = new Composition(container, new TypeLoader(), Mock.Of<IProfilingLogger>(), ComponentTests.MockRuntimeState(RuntimeLevel.Run));
    //
    //         composition.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>();
    //
    //         Current.Factory = composition.CreateFactory();
    //
    //         var logger = Mock.Of<ILogger>();
    //         var scheme = Mock.Of<IMediaPathScheme>();
    //         var config = Mock.Of<IContentSection>();
    //
    //         var mediaFileSystem = new MediaFileSystem(Mock.Of<IFileSystem>(), config, scheme, logger);
    //
    //         var imageCropperConfiguration = new ImageCropperConfiguration()
    //         {
    //             Crops = new[]
    //             {
    //                 new ImageCropperConfiguration.Crop()
    //                 {
    //                     Alias = "thumb",
    //                     Width = 100,
    //                     Height = 100
    //                 }
    //             }
    //         };
    //         var dataTypeService = new TestObjects.TestDataTypeService(
    //             new DataType(new ImageCropperPropertyEditor(Mock.Of<ILogger>(), mediaFileSystem, Mock.Of<IContentSection>(), Mock.Of<IDataTypeService>())) { Id = 1, Configuration = imageCropperConfiguration });
    //
    //         var factory = new PublishedContentTypeFactory(Mock.Of<IPublishedModelFactory>(), new PropertyValueConverterCollection(Array.Empty<IPropertyValueConverter>()), dataTypeService);
    //
    //         var converter = new ImageCropperValueConverter();
    //         var result = converter.ConvertSourceToIntermediate(null, factory.CreatePropertyType("test", 1), val1, false); // does not use type for conversion
    //
    //         var resultShouldMatch = val2.DeserializeImageCropperValue();
    //         if (expected)
    //         {
    //             Assert.AreEqual(resultShouldMatch, result);
    //         }
    //         else
    //         {
    //             Assert.AreNotEqual(resultShouldMatch, result);
    //         }
    //     }
    //     finally
    //     {
    //         Current.Reset();
    //     }
    // }
    [Test]
    public void GetCropUrl_CropAliasTest()
    {
        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            cropAlias: "Thumb",
            useCropDimensions: true);
        Assert.AreEqual(
            MediaPath + "?c=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&w=100&h=100",
            urlString);
    }

    /// <summary>
    ///     Test to ensure useCropDimensions is observed
    /// </summary>
    [Test]
    public void GetCropUrl_CropAliasIgnoreWidthHeightTest()
    {
        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            cropAlias: "Thumb",
            useCropDimensions: true,
            width: 50,
            height: 50);
        Assert.AreEqual(MediaPath + "?c=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&w=100&h=100", urlString);
    }

    [Test]
    public void GetCropUrl_WidthHeightTest()
    {
        var urlString = MediaPath.GetCropUrl(new TestImageUrlGenerator(), imageCropperValue: CropperJson1, width: 200, height: 300);
        Assert.AreEqual(MediaPath + "?f=0.80827067669172936,0.96&w=200&h=300", urlString);
    }

    [Test]
    public void GetCropUrl_FocalPointTest()
    {
        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            cropAlias: "thumb",
            preferFocalPoint: true,
            useCropDimensions: true);
        Assert.AreEqual(MediaPath + "?f=0.80827067669172936,0.96&w=100&h=100", urlString);
    }

    [Test]
    public void GetCropUrlFurtherOptionsTest()
    {
        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            width: 200,
            height: 300,
            furtherOptions: "filter=comic&roundedcorners=radius-26|bgcolor-fff");
        Assert.AreEqual(
            MediaPath + "?f=0.80827067669172936,0.96&w=200&h=300&filter=comic&roundedcorners=radius-26|bgcolor-fff",
            urlString);
    }

    /// <summary>
    ///     Test that if a crop alias has been specified that doesn't exist the method returns null
    /// </summary>
    [Test]
    public void GetCropUrlNullTest()
    {
        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            cropAlias: "Banner",
            useCropDimensions: true);
        Assert.AreEqual(null, urlString);
    }

    /// <summary>
    ///     Test the GetCropUrl method on the ImageCropDataSet Model
    /// </summary>
    [Test]
    public void GetBaseCropUrlFromModelTest()
    {
        var cropDataSet = CropperJson1.DeserializeImageCropperValue();
        var urlString = cropDataSet.GetCropUrl("thumb", new TestImageUrlGenerator());
        Assert.AreEqual("/media/1005/img_0671.jpg?c=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&w=100&h=100", urlString);
    }

    /// <summary>
    ///     Test the height ratio mode with predefined crop dimensions
    /// </summary>
    [Test]
    public void GetCropUrl_CropAliasHeightRatioModeTest()
    {
        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            cropAlias: "Thumb",
            useCropDimensions: true);
        Assert.AreEqual(MediaPath + "?c=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&w=100&h=100", urlString);
    }

    /// <summary>
    ///     Test the height ratio mode with manual width/height dimensions
    /// </summary>
    [Test]
    public void GetCropUrl_WidthHeightRatioModeTest()
    {
        var urlString = MediaPath.GetCropUrl(new TestImageUrlGenerator(), imageCropperValue: CropperJson1, width: 300, height: 150);
        Assert.AreEqual(MediaPath + "?f=0.80827067669172936,0.96&w=300&h=150", urlString);
    }

    /// <summary>
    ///     Test the height ratio mode with width/height dimensions
    /// </summary>
    [Test]
    public void GetCropUrl_HeightWidthRatioModeTest()
    {
        var urlString = MediaPath.GetCropUrl(new TestImageUrlGenerator(), imageCropperValue: CropperJson1, width: 300, height: 150);
        Assert.AreEqual(MediaPath + "?f=0.80827067669172936,0.96&w=300&h=150", urlString);
    }

    /// <summary>
    ///     Test that if Crop mode is specified as anything other than Crop the image doesn't use the crop
    /// </summary>
    [Test]
    public void GetCropUrl_SpecifiedCropModeTest()
    {
        var urlStringMin = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            width: 300,
            height: 150,
            imageCropMode: ImageCropMode.Min);
        var urlStringBoxPad = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            width: 300,
            height: 150,
            imageCropMode: ImageCropMode.BoxPad);
        var urlStringPad = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            width: 300,
            height: 150,
            imageCropMode: ImageCropMode.Pad);
        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            width: 300,
            height: 150,
            imageCropMode: ImageCropMode.Max);
        var urlStringStretch = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: CropperJson1,
            width: 300,
            height: 150,
            imageCropMode: ImageCropMode.Stretch);

        Assert.AreEqual(MediaPath + "?m=min&w=300&h=150", urlStringMin);
        Assert.AreEqual(MediaPath + "?m=boxpad&w=300&h=150", urlStringBoxPad);
        Assert.AreEqual(MediaPath + "?m=pad&w=300&h=150", urlStringPad);
        Assert.AreEqual(MediaPath + "?m=max&w=300&h=150", urlString);
        Assert.AreEqual(MediaPath + "?m=stretch&w=300&h=150", urlStringStretch);
    }

    /// <summary>
    ///     Test for upload property type
    /// </summary>
    [Test]
    public void GetCropUrl_UploadTypeTest()
    {
        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            100,
            270,
            imageCropMode: ImageCropMode.Crop,
            imageCropAnchor: ImageCropAnchor.Center);
        Assert.AreEqual(MediaPath + "?m=crop&a=center&w=100&h=270", urlString);
    }

    /// <summary>
    ///     Test for preferFocalPoint when focal point is centered
    /// </summary>
    [Test]
    public void GetCropUrl_PreferFocalPointCenter()
    {
        const string cropperJson =
            "{\"focalPoint\": {\"left\": 0.5,\"top\": 0.5},\"src\": \"/media/1005/img_0671.jpg\",\"crops\": [{\"alias\":\"thumb\",\"width\": 100,\"height\": 100,\"coordinates\": {\"x1\": 0.58729977382575338,\"y1\": 0.055768992440203169,\"x2\": 0,\"y2\": 0.32457553600198386}}]}";

        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            imageCropperValue: cropperJson,
            width: 300,
            height: 150,
            preferFocalPoint: true);
        Assert.AreEqual(MediaPath + "?w=300&h=150", urlString);
    }

    /// <summary>
    ///     Test to check if height ratio is returned for a predefined crop without coordinates and focal point in centre when
    ///     a width parameter is passed
    /// </summary>
    [Test]
    public void GetCropUrl_PreDefinedCropNoCoordinatesWithWidth()
    {
        const string cropperJson =
            "{\"focalPoint\": {\"left\": 0.5,\"top\": 0.5},\"src\": \"/media/1005/img_0671.jpg\",\"crops\": [{\"alias\": \"home\",\"width\": 270,\"height\": 161}]}";

        var urlString = MediaPath.GetCropUrl(new TestImageUrlGenerator(), imageCropperValue: cropperJson, cropAlias: "home", width: 200);
        Assert.AreEqual(MediaPath + "?w=200&h=119", urlString);
    }

    /// <summary>
    ///     Test to check if height ratio is returned for a predefined crop without coordinates and focal point is custom when
    ///     a width parameter is passed
    /// </summary>
    [Test]
    public void GetCropUrl_PreDefinedCropNoCoordinatesWithWidthAndFocalPoint()
    {
        const string cropperJson =
            "{\"focalPoint\": {\"left\": 0.4275,\"top\": 0.41},\"src\": \"/media/1005/img_0671.jpg\",\"crops\": [{\"alias\": \"home\",\"width\": 270,\"height\": 161}]}";

        var urlString = MediaPath.GetCropUrl(new TestImageUrlGenerator(), imageCropperValue: cropperJson, cropAlias: "home", width: 200);
        Assert.AreEqual(MediaPath + "?f=0.41,0.4275&w=200&h=119", urlString);
    }

    /// <summary>
    ///     Test to check if crop ratio is ignored if useCropDimensions is true
    /// </summary>
    [Test]
    public void GetCropUrl_PreDefinedCropNoCoordinatesWithWidthAndFocalPointIgnore()
    {
        const string cropperJson =
            "{\"focalPoint\": {\"left\": 0.4275,\"top\": 0.41},\"src\": \"/media/1005/img_0671.jpg\",\"crops\": [{\"alias\": \"home\",\"width\": 270,\"height\": 161}]}";

        var urlString = MediaPath.GetCropUrl(new TestImageUrlGenerator(), imageCropperValue: cropperJson, cropAlias: "home", width: 200, useCropDimensions: true);
        Assert.AreEqual(MediaPath + "?f=0.41,0.4275&w=270&h=161", urlString);
    }

    /// <summary>
    ///     Test to check if width ratio is returned for a predefined crop without coordinates and focal point in centre when a
    ///     height parameter is passed
    /// </summary>
    [Test]
    public void GetCropUrl_PreDefinedCropNoCoordinatesWithHeight()
    {
        const string cropperJson =
            "{\"focalPoint\": {\"left\": 0.5,\"top\": 0.5},\"src\": \"/media/1005/img_0671.jpg\",\"crops\": [{\"alias\": \"home\",\"width\": 270,\"height\": 161}]}";

        var urlString = MediaPath.GetCropUrl(new TestImageUrlGenerator(), imageCropperValue: cropperJson, cropAlias: "home", height: 200);
        Assert.AreEqual(MediaPath + "?w=335&h=200", urlString);
    }

    /// <summary>
    ///     Test to check result when only a width parameter is passed, effectivly a resize only
    /// </summary>
    [Test]
    public void GetCropUrl_WidthOnlyParameter()
    {
        const string cropperJson =
            "{\"focalPoint\": {\"left\": 0.5,\"top\": 0.5},\"src\": \"/media/1005/img_0671.jpg\",\"crops\": [{\"alias\": \"home\",\"width\": 270,\"height\": 161}]}";

        var urlString = MediaPath.GetCropUrl(new TestImageUrlGenerator(), imageCropperValue: cropperJson, width: 200);
        Assert.AreEqual(MediaPath + "?w=200", urlString);
    }

    /// <summary>
    ///     Test to check result when only a height parameter is passed, effectivly a resize only
    /// </summary>
    [Test]
    public void GetCropUrl_HeightOnlyParameter()
    {
        const string cropperJson =
            "{\"focalPoint\": {\"left\": 0.5,\"top\": 0.5},\"src\": \"/media/1005/img_0671.jpg\",\"crops\": [{\"alias\": \"home\",\"width\": 270,\"height\": 161}]}";

        var urlString = MediaPath.GetCropUrl(new TestImageUrlGenerator(), imageCropperValue: cropperJson, height: 200);
        Assert.AreEqual(MediaPath + "?h=200", urlString);
    }

    /// <summary>
    ///     Test to check result when using a background color with padding
    /// </summary>
    [Test]
    public void GetCropUrl_BackgroundColorParameter()
    {
        var cropperJson = "{\"focalPoint\": {\"left\": 0.5,\"top\": 0.5},\"src\": \"" + MediaPath +
                          "\",\"crops\": [{\"alias\": \"home\",\"width\": 270,\"height\": 161}]}";

        var urlString = MediaPath.GetCropUrl(
            new TestImageUrlGenerator(),
            400,
            400,
            cropperJson,
            imageCropMode: ImageCropMode.Pad,
            furtherOptions: "bgcolor=fff");
        Assert.AreEqual(MediaPath + "?m=pad&w=400&h=400&bgcolor=fff", urlString);
    }

    private void SetupJsonSerializerServiceProvider()
    {
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(s => s.GetService(typeof(IJsonSerializer))).Returns(new SystemTextJsonSerializer());
        StaticServiceProvider.Instance = serviceProvider.Object;
    }

    internal class TestImageUrlGenerator : IImageUrlGenerator
    {
        public IEnumerable<string> SupportedImageFileTypes => new[] { "jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif" };

        public string GetImageUrl(ImageUrlGenerationOptions options)
        {
            if (options == null)
            {
                return null;
            }

            var imageUrl = new StringBuilder(options.ImageUrl);

            var queryStringHasStarted = false;

            void AppendQueryString(string value)
            {
                imageUrl.Append(queryStringHasStarted ? '&' : '?');
                queryStringHasStarted = true;

                imageUrl.Append(value);
            }

            void AddQueryString(string key, params IConvertible[] values)
            {
                AppendQueryString(key + '=' +
                                  string.Join(",", values.Select(x => x.ToString(CultureInfo.InvariantCulture))));
            }

            if (options.Crop != null)
            {
                AddQueryString("c", options.Crop.Left, options.Crop.Top, options.Crop.Right, options.Crop.Bottom);
            }

            if (options.FocalPoint != null)
            {
                AddQueryString("f", options.FocalPoint.Top, options.FocalPoint.Left);
            }

            if (options.ImageCropMode.HasValue)
            {
                AddQueryString("m", options.ImageCropMode.Value.ToString().ToLowerInvariant());
            }

            if (options.ImageCropAnchor.HasValue)
            {
                AddQueryString("a", options.ImageCropAnchor.Value.ToString().ToLowerInvariant());
            }

            if (options.Width != null)
            {
                AddQueryString("w", options.Width.Value);
            }

            if (options.Height != null)
            {
                AddQueryString("h", options.Height.Value);
            }

            if (options.Quality.HasValue)
            {
                AddQueryString("q", options.Quality.Value);
            }

            if (options.FurtherOptions != null)
            {
                AppendQueryString(options.FurtherOptions.TrimStart('?', '&'));
            }

            if (options.CacheBusterValue != null)
            {
                AddQueryString("v", options.CacheBusterValue);
            }

            return imageUrl.ToString();
        }
    }
}
