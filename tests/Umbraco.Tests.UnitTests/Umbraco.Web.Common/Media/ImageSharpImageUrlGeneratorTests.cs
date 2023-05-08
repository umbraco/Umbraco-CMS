// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Imaging.ImageSharp.Media;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Media;

/// <summary>
///     Contains tests for all parameters for image generation options.
/// </summary>
[TestFixture]
public class ImageSharpImageUrlGeneratorTests
{
    private const string MediaPath = "/media/1005/img_0671.jpg";

    private static readonly ImageUrlGenerationOptions.CropCoordinates _sCrop = new(0.58729977382575338m, 0.055768992440203169m, 0m, 0.32457553600198386m);
    private static readonly ImageUrlGenerationOptions.FocalPointPosition _sFocus = new(0.96m, 0.80827067669172936m);
    private static readonly ImageSharpImageUrlGenerator _sGenerator = new(Array.Empty<string>());

    /// <summary>
    ///     Tests that the media path is returned if no options are provided.
    /// </summary>
    [Test]
    public void GivenMediaPath_AndNoOptions_ReturnsMediaPath()
    {
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath));
        Assert.AreEqual(MediaPath, actual);
    }

    /// <summary>
    ///     Test that if options is null, the generated image URL is also null.
    /// </summary>
    [Test]
    public void GivenNullOptions_ReturnsNull()
    {
        var actual = _sGenerator.GetImageUrl(null);
        Assert.IsNull(actual);
    }

    /// <summary>
    ///     Test that if a null image url is given, null is returned.
    /// </summary>
    [Test]
    public void GivenNullImageUrl_ReturnsNull()
    {
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(null));
        Assert.IsNull(actual);
    }

    [Test]
    public void GetImageUrlFurtherOptionsModeAndQualityTest()
    {
        var urlString = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            Quality = 10,
            FurtherOptions = "format=webp",
        });
        Assert.AreEqual(
            MediaPath +
            "?format=webp&quality=10",
            urlString);
    }

    [Test]
    public void GetImageUrlFurtherOptionsWithModeAndQualityTest()
    {
        var urlString = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            FurtherOptions = "quality=10&format=webp",
        });
        Assert.AreEqual(
            MediaPath +
            "?format=webp&quality=10",
            urlString);
    }

    /// <summary>
    ///     Test that if an empty string image url is given, null is returned.
    /// </summary>
    [Test]
    public void GivenEmptyStringImageUrl_ReturnsEmptyString()
    {
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty));
        Assert.AreEqual(actual, string.Empty);
    }

    /// <summary>
    ///     Tests the correct query string is returned when given a crop.
    /// </summary>
    [Test]
    public void GivenCrop_ReturnsExpectedQueryString()
    {
        const string expected = "?cc=0.58729977382575338,0.055768992440203169,0,0.32457553600198386";
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty) { Crop = _sCrop });
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///     Tests the correct query string is returned when given a width.
    /// </summary>
    [Test]
    public void GivenWidth_ReturnsExpectedQueryString()
    {
        const string expected = "?width=200";
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty) { Width = 200 });
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///     Tests the correct query string is returned when given a height.
    /// </summary>
    [Test]
    public void GivenHeight_ReturnsExpectedQueryString()
    {
        const string expected = "?height=200";
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty) { Height = 200 });
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///     Tests the correct query string is returned when provided a focal point.
    /// </summary>
    [Test]
    public void GivenFocalPoint_ReturnsExpectedQueryString()
    {
        const string expected = "?rxy=0.96,0.80827067669172936";
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty) { FocalPoint = _sFocus });
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///     Tests the correct query string is returned when given further options.
    ///     There are a few edge case inputs here to ensure thorough testing in future versions.
    /// </summary>
    [TestCase("&filter=comic&roundedcorners=radius-26%7Cbgcolor-fff", "?filter=comic&roundedcorners=radius-26%7Cbgcolor-fff")]
    [TestCase("testoptions", "?testoptions=")]
    [TestCase("&&&should=strip", "?should=strip")]
    [TestCase("should=encode&$^%()", "?should=encode&$%5E%25()=")]
    public void GivenFurtherOptions_ReturnsExpectedQueryString(string input, string expected)
    {
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty)
        {
            FurtherOptions = input,
        });
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///     Test that the correct query string is returned for all image crop modes.
    /// </summary>
    [TestCase(ImageCropMode.Min, "?rmode=min")]
    [TestCase(ImageCropMode.BoxPad, "?rmode=boxpad")]
    [TestCase(ImageCropMode.Pad, "?rmode=pad")]
    [TestCase(ImageCropMode.Max, "?rmode=max")]
    [TestCase(ImageCropMode.Stretch, "?rmode=stretch")]
    public void GivenCropMode_ReturnsExpectedQueryString(ImageCropMode cropMode, string expectedQueryString)
    {
        var cropUrl = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty)
        {
            ImageCropMode = cropMode,
        });

        Assert.AreEqual(expectedQueryString, cropUrl);
    }

    /// <summary>
    ///     Test that the correct query string is returned for all image crop anchors.
    /// </summary>
    [TestCase(ImageCropAnchor.Bottom, "?ranchor=bottom")]
    [TestCase(ImageCropAnchor.BottomLeft, "?ranchor=bottomleft")]
    [TestCase(ImageCropAnchor.BottomRight, "?ranchor=bottomright")]
    [TestCase(ImageCropAnchor.Center, "?ranchor=center")]
    [TestCase(ImageCropAnchor.Left, "?ranchor=left")]
    [TestCase(ImageCropAnchor.Right, "?ranchor=right")]
    [TestCase(ImageCropAnchor.Top, "?ranchor=top")]
    [TestCase(ImageCropAnchor.TopLeft, "?ranchor=topleft")]
    [TestCase(ImageCropAnchor.TopRight, "?ranchor=topright")]
    public void GivenCropAnchor_ReturnsExpectedQueryString(ImageCropAnchor imageCropAnchor, string expectedQueryString)
    {
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty)
        {
            ImageCropAnchor = imageCropAnchor,
        });
        Assert.AreEqual(expectedQueryString, actual);
    }

    /// <summary>
    ///     Tests that the quality query string always returns the input number regardless of value.
    /// </summary>
    [TestCase(int.MinValue)]
    [TestCase(-50)]
    [TestCase(0)]
    [TestCase(50)]
    [TestCase(int.MaxValue)]
    public void GivenQuality_ReturnsExpectedQueryString(int quality)
    {
        var expected = "?quality=" + quality;
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty)
        {
            Quality = quality,
        });
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///     Tests that the correct query string is returned for cache buster.
    ///     There are some edge case tests here to ensure thorough testing in future versions.
    /// </summary>
    [TestCase("test-buster", "?rnd=test-buster")]
    [TestCase("test-buster&&^-value", "?rnd=test-buster%26%26%5E-value")]
    public void GivenCacheBusterValue_ReturnsExpectedQueryString(string input, string expected)
    {
        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty)
        {
            CacheBusterValue = input,
        });
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///     Tests that an expected query string is returned when all options are given.
    ///     This will be a good test to see if something breaks with ordering of query string parameters.
    /// </summary>
    [Test]
    public void GivenAllOptions_ReturnsExpectedQueryString()
    {
        const string expected =
            "/media/1005/img_0671.jpg?cc=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&rxy=0.96,0.80827067669172936&rmode=stretch&ranchor=right&width=200&height=200&quality=50&more=options&rnd=buster";

        var actual = _sGenerator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            Quality = 50,
            Crop = _sCrop,
            FocalPoint = _sFocus,
            CacheBusterValue = "buster",
            FurtherOptions = "more=options",
            Height = 200,
            Width = 200,
            ImageCropAnchor = ImageCropAnchor.Right,
            ImageCropMode = ImageCropMode.Stretch,
        });

        Assert.AreEqual(expected, actual);
    }
}
