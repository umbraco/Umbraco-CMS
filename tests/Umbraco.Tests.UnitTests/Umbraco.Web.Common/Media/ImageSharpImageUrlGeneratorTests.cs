// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.Common.Media;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Media;

[TestFixture]
public class ImageSharpImageUrlGeneratorTests
{
    private const string MediaPath = "/media/1005/img_0671.jpg";

    private static readonly ImageUrlGenerationOptions.CropCoordinates s_crop = new(0.58729977382575338m, 0.055768992440203169m, 0m, 0.32457553600198386m);

    private static readonly ImageUrlGenerationOptions.FocalPointPosition s_focus1 = new(0.96m, 0.80827067669172936m);
    private static readonly ImageUrlGenerationOptions.FocalPointPosition s_focus2 = new(0.4275m, 0.41m);
    private static readonly ImageSharpImageUrlGenerator s_generator = new(new string[0]);

    [Test]
    public void GetImageUrl_CropAliasTest()
    {
        var urlString =
            s_generator.GetImageUrl(
                new ImageUrlGenerationOptions(MediaPath) { Crop = s_crop, Width = 100, Height = 100 });
        Assert.AreEqual(
            MediaPath + "?cc=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&width=100&height=100",
            urlString);
    }

    [Test]
    public void GetImageUrl_WidthHeightTest()
    {
        var urlString =
            s_generator.GetImageUrl(
                new ImageUrlGenerationOptions(MediaPath) { FocalPoint = s_focus1, Width = 200, Height = 300 });
        Assert.AreEqual(MediaPath + "?rxy=0.96,0.80827067669172936&width=200&height=300", urlString);
    }

    [Test]
    public void GetImageUrl_FocalPointTest()
    {
        var urlString =
            s_generator.GetImageUrl(
                new ImageUrlGenerationOptions(MediaPath) { FocalPoint = s_focus1, Width = 100, Height = 100 });
        Assert.AreEqual(MediaPath + "?rxy=0.96,0.80827067669172936&width=100&height=100", urlString);
    }

    [Test]
    public void GetImageUrlFurtherOptionsTest()
    {
        var urlString = s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            FocalPoint = s_focus1,
            Width = 200,
            Height = 300,
            FurtherOptions = "&filter=comic&roundedcorners=radius-26|bgcolor-fff",
        });
        Assert.AreEqual(
            MediaPath +
            "?rxy=0.96,0.80827067669172936&width=200&height=300&filter=comic&roundedcorners=radius-26%7Cbgcolor-fff",
            urlString);
    }

    /// <summary>
    ///     Test that if options is null, the generated image URL is also null.
    /// </summary>
    [Test]
    public void GetImageUrlNullOptionsTest()
    {
        var urlString = s_generator.GetImageUrl(null);
        Assert.AreEqual(null, urlString);
    }

    /// <summary>
    ///     Test that if the image URL is null, the generated image URL is also null.
    /// </summary>
    [Test]
    public void GetImageUrlNullTest()
    {
        var urlString = s_generator.GetImageUrl(new ImageUrlGenerationOptions(null));
        Assert.AreEqual(null, urlString);
    }

    /// <summary>
    ///     Test that if the image URL is empty, the generated image URL is empty.
    /// </summary>
    [Test]
    public void GetImageUrlEmptyTest()
    {
        var urlString = s_generator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty));
        Assert.AreEqual(string.Empty, urlString);
    }

    /// <summary>
    ///     Test the GetImageUrl method on the ImageCropDataSet Model
    /// </summary>
    [Test]
    public void GetBaseCropUrlFromModelTest()
    {
        var urlString =
            s_generator.GetImageUrl(
                new ImageUrlGenerationOptions(string.Empty) { Crop = s_crop, Width = 100, Height = 100 });
        Assert.AreEqual(
            "?cc=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&width=100&height=100",
            urlString);
    }

    /// <summary>
    ///     Test that if Crop mode is specified as anything other than Crop the image doesn't use the crop
    /// </summary>
    [Test]
    public void GetImageUrl_SpecifiedCropModeTest()
    {
        var urlStringMin =
            s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
            {
                ImageCropMode = ImageCropMode.Min,
                Width = 300,
                Height = 150,
            });
        var urlStringBoxPad =
            s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
            {
                ImageCropMode = ImageCropMode.BoxPad,
                Width = 300,
                Height = 150,
            });
        var urlStringPad =
            s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
            {
                ImageCropMode = ImageCropMode.Pad,
                Width = 300,
                Height = 150,
            });
        var urlStringMax =
            s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
            {
                ImageCropMode = ImageCropMode.Max,
                Width = 300,
                Height = 150,
            });
        var urlStringStretch =
            s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
            {
                ImageCropMode = ImageCropMode.Stretch,
                Width = 300,
                Height = 150,
            });

        Assert.AreEqual(MediaPath + "?rmode=min&width=300&height=150", urlStringMin);
        Assert.AreEqual(MediaPath + "?rmode=boxpad&width=300&height=150", urlStringBoxPad);
        Assert.AreEqual(MediaPath + "?rmode=pad&width=300&height=150", urlStringPad);
        Assert.AreEqual(MediaPath + "?rmode=max&width=300&height=150", urlStringMax);
        Assert.AreEqual(MediaPath + "?rmode=stretch&width=300&height=150", urlStringStretch);
    }

    /// <summary>
    ///     Test for upload property type
    /// </summary>
    [Test]
    public void GetImageUrl_UploadTypeTest()
    {
        var urlString = s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            ImageCropMode = ImageCropMode.Crop,
            ImageCropAnchor = ImageCropAnchor.Center,
            Width = 100,
            Height = 270,
        });
        Assert.AreEqual(MediaPath + "?rmode=crop&ranchor=center&width=100&height=270", urlString);
    }

    /// <summary>
    ///     Test for preferFocalPoint when focal point is centered
    /// </summary>
    [Test]
    public void GetImageUrl_PreferFocalPointCenter()
    {
        var urlString = s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { Width = 300, Height = 150 });
        Assert.AreEqual(MediaPath + "?width=300&height=150", urlString);
    }

    /// <summary>
    ///     Test to check if crop ratio is ignored if useCropDimensions is true
    /// </summary>
    [Test]
    public void GetImageUrl_PreDefinedCropNoCoordinatesWithWidthAndFocalPointIgnore()
    {
        var urlString =
            s_generator.GetImageUrl(
                new ImageUrlGenerationOptions(MediaPath) { FocalPoint = s_focus2, Width = 270, Height = 161 });
        Assert.AreEqual(MediaPath + "?rxy=0.4275,0.41&width=270&height=161", urlString);
    }

    /// <summary>
    ///     Test to check result when only a width parameter is passed, effectivly a resize only
    /// </summary>
    [Test]
    public void GetImageUrl_WidthOnlyParameter()
    {
        var urlString = s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { Width = 200 });
        Assert.AreEqual(MediaPath + "?width=200", urlString);
    }

    /// <summary>
    ///     Test to check result when only a height parameter is passed, effectivly a resize only
    /// </summary>
    [Test]
    public void GetImageUrl_HeightOnlyParameter()
    {
        var urlString = s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath) { Height = 200 });
        Assert.AreEqual(MediaPath + "?height=200", urlString);
    }

    /// <summary>
    ///     Test to check result when using a background color with padding
    /// </summary>
    [Test]
    public void GetImageUrl_BackgroundColorParameter()
    {
        var urlString = s_generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            ImageCropMode = ImageCropMode.Pad,
            Width = 400,
            Height = 400,
            FurtherOptions = "&bgcolor=fff",
        });
        Assert.AreEqual(MediaPath + "?rmode=pad&width=400&height=400&bgcolor=fff", urlString);
    }
}
