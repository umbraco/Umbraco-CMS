// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
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
    private static readonly ImageUrlGenerationOptions.CropCoordinates _crop = new ImageUrlGenerationOptions.CropCoordinates(0.58729977382575338m, 0.055768992440203169m, 0m, 0.32457553600198386m);
    private static readonly ImageUrlGenerationOptions.FocalPointPosition _focus1 = new ImageUrlGenerationOptions.FocalPointPosition(0.96m, 0.80827067669172936m);
    private static readonly ImageUrlGenerationOptions.FocalPointPosition _focus2 = new ImageUrlGenerationOptions.FocalPointPosition(0.4275m, 0.41m);
    private static readonly ImageSharpImageUrlGenerator _generator = new ImageSharpImageUrlGenerator(new string[0]);

    [Test]
    public void GivenMediaPath_AndNoOptions_ReturnsMediaPath()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            Crop = _crop,
            Width = 100,
            Height = 100,
        });

        Assert.AreEqual(MediaPath + "?cc=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&width=100&height=100", urlString);
    }

    /// <summary>
    ///     Test that if options is null, the generated image URL is also null.
    /// </summary>
    [Test]
    public void GivenNullOptions_ReturnsNull()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            FocalPoint = _focus1,
            Width = 200,
            Height = 300,
        });

        Assert.AreEqual(MediaPath + "?rxy=0.96,0.80827067669172936&width=200&height=300", urlString);
    }

    /// <summary>
    ///     Test that if a null image url is given, null is returned.
    /// </summary>
    [Test]
    public void GivenNullImageUrl_ReturnsNull()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            FocalPoint = _focus1,
            Width = 100,
            Height = 100,
        });

        Assert.AreEqual(MediaPath + "?rxy=0.96,0.80827067669172936&width=100&height=100", urlString);
    }

    [Test]
    public void GetImageUrlFurtherOptionsTest()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            FocalPoint = _focus1,
            Width = 200,
            Height = 300,
            FurtherOptions = "&filter=comic&roundedcorners=radius-26|bgcolor-fff",
        });

        Assert.AreEqual(MediaPath + "?rxy=0.96,0.80827067669172936&width=200&height=300&filter=comic&roundedcorners=radius-26%7Cbgcolor-fff", urlString);
    }

    [Test]
    public void GetImageUrlFurtherOptionsModeAndQualityTest()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
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
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            FurtherOptions = "quality=10&format=webp",
        });
        Assert.AreEqual(
            MediaPath +
            "?format=webp&quality=10",
            urlString);
    }

    /// <summary>
    /// Test that if options is null, the generated image URL is also null.
    /// </summary>
    [Test]
    public void GivenEmptyStringImageUrl_ReturnsEmptyString()
    {
        var urlString = _generator.GetImageUrl(null);
        Assert.AreEqual(null, urlString);
    }

    /// <summary>
    /// Test that if the image URL is null, the generated image URL is also null.
    /// </summary>
    [Test]
    public void GivenCrop_ReturnsExpectedQueryString()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(null));
        Assert.AreEqual(null, urlString);
    }

    /// <summary>
    /// Test that if the image URL is empty, the generated image URL is empty.
    /// </summary>
    [Test]
    public void GivenWidth_ReturnsExpectedQueryString()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty));
        Assert.AreEqual(string.Empty, urlString);
    }

    /// <summary>
    /// Test the GetImageUrl method on the ImageCropDataSet Model
    /// </summary>
    [Test]
    public void GivenHeight_ReturnsExpectedQueryString()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(string.Empty)
        {
            Crop = _crop,
            Width = 100,
            Height = 100,
        });

        Assert.AreEqual("?cc=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&width=100&height=100", urlString);
    }

    /// <summary>
    /// Test that if Crop mode is specified as anything other than Crop the image doesn't use the crop
    /// </summary>
    [Test]
    public void GivenFocalPoint_ReturnsExpectedQueryString()
    {
        var urlStringMin = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            ImageCropMode = ImageCropMode.Min,
            Width = 300,
            Height = 150,
        });

        var urlStringBoxPad = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            ImageCropMode = ImageCropMode.BoxPad,
            Width = 300,
            Height = 150,
        });

        var urlStringPad = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            ImageCropMode = ImageCropMode.Pad,
            Width = 300,
            Height = 150,
        });

        var urlStringMax = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            ImageCropMode = ImageCropMode.Max,
            Width = 300,
            Height = 150,
        });

        var urlStringStretch = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
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
    /// Test for upload property type
    /// </summary>
    [TestCase("&filter=comic&roundedcorners=radius-26%7Cbgcolor-fff", "?filter=comic&roundedcorners=radius-26%7Cbgcolor-fff")]
    [TestCase("testoptions", "?testoptions=")]
    [TestCase("&&&should=strip", "?should=strip")]
    [TestCase("should=encode&$^%()", "?should=encode&$%5E%25()=")]
    public void GivenFurtherOptions_ReturnsExpectedQueryString(string input, string expected)
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            FurtherOptions = input,
        });

        Assert.AreEqual(MediaPath + expected, urlString);
    }

    /// <summary>
    /// Test for preferFocalPoint when focal point is centered
    /// </summary>
    [Test]
    public void GetImageUrl_PreferFocalPointCenter()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            Width = 300,
            Height = 150,
        });

        Assert.AreEqual(MediaPath + "?width=300&height=150", urlString);
    }

    /// <summary>
    /// Test to check if crop ratio is ignored if useCropDimensions is true
    /// </summary>
    [Test]
    public void GetImageUrl_PreDefinedCropNoCoordinatesWithWidthAndFocalPointIgnore()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            FocalPoint = _focus2,
            Width = 270,
            Height = 161,
        });

        Assert.AreEqual(MediaPath + "?rxy=0.4275,0.41&width=270&height=161", urlString);
    }

    /// <summary>
    /// Test to check result when only a width parameter is passed, effectivly a resize only
    /// </summary>
    [Test]
    public void GetImageUrl_WidthOnlyParameter()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            Width = 200,
        });

        Assert.AreEqual(MediaPath + "?width=200", urlString);
    }

    /// <summary>
    /// Test to check result when only a height parameter is passed, effectivly a resize only
    /// </summary>
    [Test]
    public void GetImageUrl_HeightOnlyParameter()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            Height = 200,
        });

        Assert.AreEqual(MediaPath + "?height=200", urlString);
    }

    /// <summary>
    /// Test to check result when using a background color with padding
    /// </summary>
    [Test]
    public void GivenAllOptions_ReturnsExpectedQueryString()
    {
        var urlString = _generator.GetImageUrl(new ImageUrlGenerationOptions(MediaPath)
        {
            Quality = 50,
            Crop = _crop,
            FocalPoint = _focus1,
            CacheBusterValue = "buster",
            FurtherOptions = "more=options",
            Height = 200,
            Width = 200,
            ImageCropAnchor = ImageCropAnchor.Right,
            ImageCropMode = ImageCropMode.Stretch,
        });

        Assert.AreEqual(MediaPath + "?cc=0.58729977382575338,0.055768992440203169,0,0.32457553600198386&rxy=0.96,0.80827067669172936&rmode=stretch&ranchor=right&width=200&height=200&quality=50&more=options&v=buster", urlString);
    }

    /// <summary>
    /// Test to check result when using a HMAC security key.
    /// </summary>
    [Test]
    public void GetImageUrl_HMACSecurityKey()
    {
        var requestAuthorizationUtilities = new RequestAuthorizationUtilities(
            Options.Create(new ImageSharpMiddlewareOptions()
            {
                HMACSecretKey = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }
            }),
            new QueryCollectionRequestParser(),
            new[]
            {
                new ResizeWebProcessor()
            },
            new CommandParser(Enumerable.Empty<ICommandConverter>()),
            new ServiceCollection().BuildServiceProvider());

        var generator = new ImageSharpImageUrlGenerator(new string[0], requestAuthorizationUtilities);
        var options = new ImageUrlGenerationOptions(MediaPath)
        {
            Width = 400,
            Height = 400,
        };

        Assert.AreEqual(MediaPath + "?width=400&height=400&hmac=6335195986da0663e23eaadfb9bb32d537375aaeec253aae66b8f4388506b4b2", generator.GetImageUrl(options));

        // CacheBusterValue isn't included in HMAC generation
        options.CacheBusterValue = "not-included-in-hmac";
        Assert.AreEqual(MediaPath + "?width=400&height=400&v=not-included-in-hmac&hmac=6335195986da0663e23eaadfb9bb32d537375aaeec253aae66b8f4388506b4b2", generator.GetImageUrl(options));

        // Removing height should generate a different HMAC
        options.Height = null;
        Assert.AreEqual(MediaPath + "?width=400&v=not-included-in-hmac&hmac=5bd24a05de5ea068533579863773ddac9269482ad515575be4aace7e9e50c88c", generator.GetImageUrl(options));

        // But adding it again using FurtherOptions should include it (and produce the same HMAC as before)
        options.FurtherOptions = "height=400";
        Assert.AreEqual(MediaPath + "?width=400&height=400&v=not-included-in-hmac&hmac=6335195986da0663e23eaadfb9bb32d537375aaeec253aae66b8f4388506b4b2", generator.GetImageUrl(options));
    }
}
