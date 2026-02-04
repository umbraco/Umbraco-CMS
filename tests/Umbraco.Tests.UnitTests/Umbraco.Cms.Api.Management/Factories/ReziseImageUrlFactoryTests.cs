using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class ReziseImageUrlFactoryTests
{
    private const string BaseUrl = "https://example.com";
    private const string PropertyAlias = Constants.Conventions.Media.File;

    [TestCase("/media/document.pdf", "webp", Description = "PDF should convert to WebP")]
    [TestCase("/media/document.eps", "webp", Description = "EPS should convert to WebP")]
    [TestCase("/media/document.PDF", "webp", Description = "PDF with uppercase extension should convert to WebP")]
    [TestCase("/media/document.Pdf", "webp", Description = "PDF with mixed case extension should convert to WebP")]
    public void CreateUrlSets_NonImageFormat_AddsWebpFormatParameter(string mediaPath, string expectedFormat)
    {
        // Arrange
        var factory = CreateFactory();
        var media = CreateMediaWithUrl(mediaPath);
        var options = new ImageResizeOptions(Height: 200, Width: 200);

        // Act
        var result = factory.CreateUrlSets([media], options).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var urlInfo = result[0].UrlInfos.FirstOrDefault();
        Assert.That(urlInfo, Is.Not.Null);
        Assert.That(urlInfo!.Url, Does.Contain($"format={expectedFormat}"));
    }

    [TestCase("/media/image.jpg", Description = "JPG should keep original format")]
    [TestCase("/media/image.jpeg", Description = "JPEG should keep original format")]
    [TestCase("/media/image.png", Description = "PNG should keep original format")]
    [TestCase("/media/image.gif", Description = "GIF should keep original format")]
    [TestCase("/media/image.webp", Description = "WebP should keep original format")]
    [TestCase("/media/image.bmp", Description = "BMP should keep original format")]
    [TestCase("/media/image.tif", Description = "TIF should keep original format")]
    [TestCase("/media/image.tiff", Description = "TIFF should keep original format")]
    [TestCase("/media/image.JPG", Description = "JPG with uppercase extension should keep original format")]
    [TestCase("/media/image.PNG", Description = "PNG with uppercase extension should keep original format")]
    public void CreateUrlSets_TrueImageFormat_NoFormatParameter(string mediaPath)
    {
        // Arrange
        var factory = CreateFactory();
        var media = CreateMediaWithUrl(mediaPath);
        var options = new ImageResizeOptions(Height: 200, Width: 200);

        // Act
        var result = factory.CreateUrlSets([media], options).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var urlInfo = result[0].UrlInfos.FirstOrDefault();
        Assert.That(urlInfo, Is.Not.Null);
        Assert.That(urlInfo!.Url, Does.Not.Contain("format="));
    }

    [TestCase("/media/document.pdf", "png", Description = "Explicitly requested PNG format overrides automatic WebP")]
    [TestCase("/media/image.jpg", "webp", Description = "Explicitly requested WebP format is honored for JPG")]
    [TestCase("/media/image.png", "jpg", Description = "Explicitly requested JPG format is honored for PNG")]
    public void CreateUrlSets_ExplicitFormatRequested_HonorsRequestedFormat(string mediaPath, string requestedFormat)
    {
        // Arrange
        var factory = CreateFactory();
        var media = CreateMediaWithUrl(mediaPath);
        var options = new ImageResizeOptions(Height: 200, Width: 200, Format: requestedFormat);

        // Act
        var result = factory.CreateUrlSets([media], options).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var urlInfo = result[0].UrlInfos.FirstOrDefault();
        Assert.That(urlInfo, Is.Not.Null);
        Assert.That(urlInfo!.Url, Does.Contain($"format={requestedFormat}"));
    }

    [Test]
    public void CreateUrlSets_UrlWithQueryString_ReturnsEmpty()
    {
        // Arrange
        var factory = CreateFactory();
        var mediaPath = "/media/document.pdf?v=123";
        var media = CreateMediaWithUrl(mediaPath);
        var options = new ImageResizeOptions(Height: 200, Width: 200);

        // Act
        var result = factory.CreateUrlSets([media], options).ToList();

        // Assert
        // URLs with query strings cause Path.GetExtension to include the query string
        // (e.g., ".pdf?v=123"), which doesn't match SupportedImageFileTypes, so returns empty
        // Note: This is a limitation of the current implementation
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].UrlInfos, Is.Empty);
    }

    [Test]
    public void CreateUrlSets_UrlWithoutExtension_ReturnsEmpty()
    {
        // Arrange
        var factory = CreateFactory();
        var mediaPath = "/media/someimage";
        var media = CreateMediaWithUrl(mediaPath);
        var options = new ImageResizeOptions(Height: 200, Width: 200);

        // Act
        var result = factory.CreateUrlSets([media], options).ToList();

        // Assert
        // Files without extensions are not in SupportedImageFileTypes, so should return empty
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].UrlInfos, Is.Empty);
    }

    [Test]
    public void CreateUrlSets_SvgImage_ReturnsOriginalUrlWithoutResizing()
    {
        // Arrange
        var factory = CreateFactory();
        var mediaPath = "/media/logo.svg";
        var media = CreateMediaWithUrl(mediaPath);
        var options = new ImageResizeOptions(Height: 200, Width: 200);

        // Act
        var result = factory.CreateUrlSets([media], options).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var urlInfo = result[0].UrlInfos.FirstOrDefault();
        Assert.That(urlInfo, Is.Not.Null);
        // SVG should return original URL without any processing parameters
        Assert.That(urlInfo!.Url, Is.EqualTo($"{BaseUrl}{mediaPath}"));
        Assert.That(urlInfo.Url, Does.Not.Contain("width="));
        Assert.That(urlInfo.Url, Does.Not.Contain("height="));
        Assert.That(urlInfo.Url, Does.Not.Contain("format="));
    }

    [Test]
    public void CreateUrlSets_CustomImageFileTypes_RespectsConfiguration()
    {
        // Arrange
        var customFormats = new HashSet<string> { "jpg", "png" }; // Only JPG and PNG, not GIF
        var factory = CreateFactory(trueImageFormats: customFormats);

        var gifMedia = CreateMediaWithUrl("/media/image.gif");
        var jpgMedia = CreateMediaWithUrl("/media/image.jpg");
        var options = new ImageResizeOptions(Height: 200, Width: 200);

        // Act
        var gifResult = factory.CreateUrlSets([gifMedia], options).ToList();
        var jpgResult = factory.CreateUrlSets([jpgMedia], options).ToList();

        // Assert
        // GIF should get format=webp since it's not in custom ImageFileTypes
        var gifUrl = gifResult[0].UrlInfos.First().Url;
        Assert.That(gifUrl, Does.Contain("format=webp"));

        // JPG should not get format parameter since it's in custom ImageFileTypes
        var jpgUrl = jpgResult[0].UrlInfos.First().Url;
        Assert.That(jpgUrl, Does.Not.Contain("format="));
    }

    [Test]
    public void CreateUrlSets_UnsupportedFileType_ReturnsEmpty()
    {
        // Arrange
        var factory = CreateFactory();
        var media = CreateMediaWithUrl("/media/document.txt");
        var options = new ImageResizeOptions(Height: 200, Width: 200);

        // Act
        var result = factory.CreateUrlSets([media], options).ToList();

        // Assert
        // TXT is not in SupportedImageFileTypes, so should return empty
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].UrlInfos, Is.Empty);
    }

    [Test]
    public void CreateUrlSets_MultipleMediaItems_ProcessesAllCorrectly()
    {
        // Arrange
        var factory = CreateFactory();
        var pdfMedia = CreateMediaWithUrl("/media/document.pdf", Guid.NewGuid());
        var jpgMedia = CreateMediaWithUrl("/media/image.jpg", Guid.NewGuid());
        var options = new ImageResizeOptions(Height: 200, Width: 200);

        // Act
        var result = factory.CreateUrlSets([pdfMedia, jpgMedia], options).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));

        // PDF should have format=webp
        var pdfUrl = result[0].UrlInfos.First().Url;
        Assert.That(pdfUrl, Does.Contain("format=webp"));

        // JPG should not have format parameter
        var jpgUrl = result[1].UrlInfos.First().Url;
        Assert.That(jpgUrl, Does.Not.Contain("format="));
    }

    [Test]
    public void CreateUrlSets_ImageResizeOptions_AppliesWidthAndHeight()
    {
        // Arrange
        var factory = CreateFactory();
        var media = CreateMediaWithUrl("/media/image.jpg");
        var options = new ImageResizeOptions(Height: 300, Width: 400);

        // Act
        var result = factory.CreateUrlSets([media], options).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var urlInfo = result[0].UrlInfos.FirstOrDefault();
        Assert.That(urlInfo, Is.Not.Null);
        Assert.That(urlInfo!.Url, Does.Contain("width=400"));
        Assert.That(urlInfo.Url, Does.Contain("height=300"));
    }

    [Test]
    public void CreateUrlSets_ImageResizeOptionsWithMode_AppliesMode()
    {
        // Arrange
        var factory = CreateFactory();
        var media = CreateMediaWithUrl("/media/image.jpg");
        var options = new ImageResizeOptions(Height: 200, Width: 200, Mode: ImageCropMode.Crop);

        // Act
        var result = factory.CreateUrlSets([media], options).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var urlInfo = result[0].UrlInfos.FirstOrDefault();
        Assert.That(urlInfo, Is.Not.Null);
        Assert.That(urlInfo!.Url, Does.Contain("mode=crop"));
    }

    [Test]
    public void CreateUrlSets_ObsoleteOverload_StillWorks()
    {
        // Arrange
        var factory = CreateFactory();
        var media = CreateMediaWithUrl("/media/document.pdf");

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        var result = factory.CreateUrlSets([media], height: 200, width: 200, mode: ImageCropMode.Crop).ToList();
#pragma warning restore CS0618

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        var urlInfo = result[0].UrlInfos.FirstOrDefault();
        Assert.That(urlInfo, Is.Not.Null);
        Assert.That(urlInfo!.Url, Does.Contain("format=webp"));
    }

    private static ReziseImageUrlFactory CreateFactory(ISet<string>? trueImageFormats = null)
    {
        var contentSettings = CreateContentSettings();
        var imagingSettings = CreateImagingSettings(trueImageFormats);
        var imageUrlGenerator = CreateImageUrlGenerator();
        var mediaUrlGenerators = CreateMediaUrlGeneratorCollection();
        var absoluteUrlBuilder = CreateAbsoluteUrlBuilder();

        return new ReziseImageUrlFactory(
            imageUrlGenerator,
            Options.Create(contentSettings),
            Options.Create(imagingSettings),
            mediaUrlGenerators,
            absoluteUrlBuilder);
    }

    private static ContentSettings CreateContentSettings()
    {
        var autoFillProperties = new HashSet<ImagingAutoFillUploadField>
        {
            new() { Alias = PropertyAlias },
        };

        return new ContentSettings
        {
            Imaging = new ContentImagingSettings
            {
                AutoFillImageProperties = autoFillProperties,
            },
        };
    }

    private static ContentImagingSettings CreateImagingSettings(ISet<string>? trueImageFormats = null)
    {
        return new ContentImagingSettings
        {
            ImageFileTypes = trueImageFormats ?? new HashSet<string>(
                ContentImagingSettings.StaticImageFileTypes.Split(Constants.CharArrays.Comma)),
        };
    }

    private static IImageUrlGenerator CreateImageUrlGenerator()
    {
        var mock = new Mock<IImageUrlGenerator>();

        // Setup supported file types (includes PDF for conversion scenarios)
        // Note: SVG is intentionally excluded - the factory handles SVG specially by returning the original URL
        mock.SetupGet(x => x.SupportedImageFileTypes)
            .Returns(new[] { "jpg", "jpeg", "png", "gif", "webp", "bmp", "tif", "tiff", "pdf", "eps" });

        // Setup GetImageUrl to return a URL with query parameters
        mock.Setup(x => x.GetImageUrl(It.IsAny<ImageUrlGenerationOptions>()))
            .Returns<ImageUrlGenerationOptions>(options =>
            {
                if (options.ImageUrl == null)
                {
                    return null;
                }

                var queryParams = new List<string>();

                if (options.Width.HasValue)
                {
                    queryParams.Add($"width={options.Width}");
                }

                if (options.Height.HasValue)
                {
                    queryParams.Add($"height={options.Height}");
                }

                if (options.ImageCropMode.HasValue)
                {
                    queryParams.Add($"mode={options.ImageCropMode.ToString()!.ToLowerInvariant()}");
                }

                if (!string.IsNullOrEmpty(options.Format))
                {
                    queryParams.Add($"format={options.Format}");
                }

                var separator = options.ImageUrl.Contains('?') ? "&" : "?";
                return queryParams.Count > 0
                    ? $"{options.ImageUrl}{separator}{string.Join("&", queryParams)}"
                    : options.ImageUrl;
            });

        return mock.Object;
    }

    private static MediaUrlGeneratorCollection CreateMediaUrlGeneratorCollection()
    {
        var generators = new List<IMediaUrlGenerator>
        {
            new StubMediaUrlGenerator(),
        };

        return new MediaUrlGeneratorCollection(() => generators);
    }

    private static IAbsoluteUrlBuilder CreateAbsoluteUrlBuilder()
    {
        var mock = new Mock<IAbsoluteUrlBuilder>();
        mock.Setup(x => x.ToAbsoluteUrl(It.IsAny<string>()))
            .Returns<string>(url => new Uri($"{BaseUrl}{url}"));

        return mock.Object;
    }

    private static IMedia CreateMediaWithUrl(string? mediaPath, Guid? key = null)
    {
        var mediaMock = new Mock<IMedia>();
        mediaMock.SetupGet(m => m.Key).Returns(key ?? Guid.NewGuid());

        var propertiesMock = new Mock<IPropertyCollection>();

        if (mediaPath != null)
        {
            IProperty? outProperty = CreatePropertyMock(PropertyAlias, mediaPath);
            propertiesMock
                .Setup(p => p.TryGetValue(PropertyAlias, out outProperty))
                .Returns(true);
        }

        mediaMock.SetupGet(m => m.Properties).Returns(propertiesMock.Object);

        return mediaMock.Object;
    }

    private static IProperty CreatePropertyMock(string alias, string path)
    {
        var propertyTypeMock = new Mock<IPropertyType>();
        propertyTypeMock.SetupGet(pt => pt.PropertyEditorAlias).Returns(Constants.PropertyEditors.Aliases.UploadField);

        var propertyMock = new Mock<IProperty>();
        propertyMock.SetupGet(p => p.Alias).Returns(alias);
        propertyMock.SetupGet(p => p.PropertyType).Returns(propertyTypeMock.Object);
        propertyMock.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>())).Returns(path);

        return propertyMock.Object;
    }

    private class StubMediaUrlGenerator : IMediaUrlGenerator
    {
        public bool TryGetMediaPath(string? propertyEditorAlias, object? value, out string? mediaPath)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                mediaPath = stringValue;
                return true;
            }

            mediaPath = null;
            return false;
        }
    }
}
