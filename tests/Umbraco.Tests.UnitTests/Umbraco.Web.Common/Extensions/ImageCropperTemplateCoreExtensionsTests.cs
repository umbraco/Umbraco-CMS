// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions;

[TestFixture]
public class ImageCropperTemplateCoreExtensionsTests
{
    [Test]
    public void GetCropUrl_WithCropSpecifiedButNotFound_ReturnsNull()
    {
        var imageUrl = "/test.jpg";
        var imageUrlGenerator = CreateMockImageUrlGenerator();
        var result = imageUrl.GetCropUrl(
            imageUrlGenerator.Object,
            new ImageCropperValue(),
            imageCropMode: ImageCropMode.Crop,
            cropAlias: "Missing");

        Assert.IsNull(result);
    }

    [Test]
    public void GetCropUrl_WithCropSpecifiedAndUsingCropDimensions_CallsImageGeneratorWithCorrectParameters()
    {
        var imageUrl = "/test.jpg";
        var imageUrlGenerator = CreateMockImageUrlGenerator();
        var result = imageUrl.GetCropUrl(
            imageUrlGenerator.Object,
            CreateImageCropperValueWithCrops(),
            imageCropMode: ImageCropMode.Crop,
            cropAlias: "TestCrop",
            useCropDimensions: true);

        imageUrlGenerator
            .Verify(x => x.GetImageUrl(
                It.Is<ImageUrlGenerationOptions>(y => y.Width == 100 &&
                                                      y.Height == 200)));
    }

    [Test]
    public void GetCropUrl_WithCropSpecifiedAndWidthAndHeightProvided_CallsImageGeneratorWithCorrectParameters()
    {
        var imageUrl = "/test.jpg";
        var imageUrlGenerator = CreateMockImageUrlGenerator();
        var result = imageUrl.GetCropUrl(
            imageUrlGenerator.Object,
            CreateImageCropperValueWithCrops(),
            imageCropMode: ImageCropMode.Crop,
            cropAlias: "TestCrop",
            width: 50,
            height: 80);

        imageUrlGenerator
            .Verify(x => x.GetImageUrl(
                It.Is<ImageUrlGenerationOptions>(y => y.Width == 50 &&
                                                      y.Height == 80)));
    }

    [Test]
    public void GetCropUrl_WithCropSpecifiedAndWidthOnlyProvided_CallsImageGeneratorWithCorrectParameters()
    {
        var imageUrl = "/test.jpg";
        var imageUrlGenerator = CreateMockImageUrlGenerator();
        var result = imageUrl.GetCropUrl(
            imageUrlGenerator.Object,
            CreateImageCropperValueWithCrops(),
            imageCropMode: ImageCropMode.Crop,
            cropAlias: "TestCrop",
            width: 50);

        imageUrlGenerator
            .Verify(x => x.GetImageUrl(
                It.Is<ImageUrlGenerationOptions>(y => y.Width == 50 &&
                                                      y.Height == 100)));
    }

    [Test]
    public void GetCropUrl_WithCropSpecifiedAndHeightOnlyProvided_CallsImageGeneratorWithCorrectParameters()
    {
        var imageUrl = "/test.jpg";
        var imageUrlGenerator = CreateMockImageUrlGenerator();
        var result = imageUrl.GetCropUrl(
            imageUrlGenerator.Object,
            CreateImageCropperValueWithCrops(),
            imageCropMode: ImageCropMode.Crop,
            cropAlias: "TestCrop",
            height: 50);

        imageUrlGenerator
            .Verify(x => x.GetImageUrl(
                It.Is<ImageUrlGenerationOptions>(y => y.Width == 25 &&
                                                      y.Height == 50)));
    }

    private static Mock<IImageUrlGenerator> CreateMockImageUrlGenerator() => new();

    private static ImageCropperValue CreateImageCropperValueWithCrops() => new()
    {
        Crops = new List<ImageCropperValue.ImageCropperCrop> { new() { Alias = "TestCrop", Width = 100, Height = 200 } },
    };
}
