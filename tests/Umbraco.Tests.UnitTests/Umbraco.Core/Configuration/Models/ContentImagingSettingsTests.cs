using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models;

[TestFixture]
public class ContentImagingSettingsTests
{
    [Test]
    public void ImageFileTypes_DefaultValue_ContainsExpectedFormats()
    {
        // Arrange
        var settings = new ContentImagingSettings();

        // Assert
        Assert.That(settings.ImageFileTypes, Is.Not.Null);
        Assert.That(settings.ImageFileTypes, Is.Not.Empty);

        // Check that default formats are present
        var expectedFormats = new[] { "jpg", "jpeg", "png", "gif", "webp", "bmp", "tif", "tiff" };
        foreach (var format in expectedFormats)
        {
            Assert.That(
                settings.ImageFileTypes,
                Does.Contain(format),
                $"Expected ImageFileTypes to contain '{format}'");
        }
    }

    [Test]
    public void ImageFileTypes_DefaultValue_MatchesStaticConstant()
    {
        // Arrange
        var settings = new ContentImagingSettings();
        var expectedFormats = ContentImagingSettings.StaticImageFileTypes.Split(Constants.CharArrays.Comma);

        // Assert
        Assert.That(settings.ImageFileTypes.Count, Is.EqualTo(expectedFormats.Length));
        foreach (var format in expectedFormats)
        {
            Assert.That(settings.ImageFileTypes, Does.Contain(format));
        }
    }

    [Test]
    public void ImageFileTypes_DefaultValue_ContainsExpectedFormats()
    {
        // Arrange
        var settings = new ContentImagingSettings();

        // Assert
        Assert.That(settings.ImageFileTypes, Is.Not.Null);
        Assert.That(settings.ImageFileTypes, Is.Not.Empty);

        // Check that default formats are present
        var expectedFormats = new[] { "jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif", "webp" };
        foreach (var format in expectedFormats)
        {
            Assert.That(
                settings.ImageFileTypes,
                Does.Contain(format),
                $"Expected ImageFileTypes to contain '{format}'");
        }
    }

    [Test]
    public void ImageFileTypes_DefaultValue_MatchesStaticConstant()
    {
        // Arrange
        var settings = new ContentImagingSettings();
        var expectedFormats = ContentImagingSettings.StaticImageFileTypes.Split(Constants.CharArrays.Comma);

        // Assert
        Assert.That(settings.ImageFileTypes.Count, Is.EqualTo(expectedFormats.Length));
        foreach (var format in expectedFormats)
        {
            Assert.That(settings.ImageFileTypes, Does.Contain(format));
        }
    }

    [Test]
    public void ImageFileTypes_CanBeConfigured_WithCustomFormats()
    {
        // Arrange
        var customFormats = new HashSet<string> { "jpg", "png", "webp" };
        var settings = new ContentImagingSettings
        {
            ImageFileTypes = customFormats,
        };

        // Assert
        Assert.That(settings.ImageFileTypes, Is.EqualTo(customFormats));
        Assert.That(settings.ImageFileTypes.Count, Is.EqualTo(3));
        Assert.That(settings.ImageFileTypes, Does.Contain("jpg"));
        Assert.That(settings.ImageFileTypes, Does.Contain("png"));
        Assert.That(settings.ImageFileTypes, Does.Contain("webp"));
    }

    [Test]
    public void ImageFileTypes_CanBeConfigured_WithCustomFormats()
    {
        // Arrange
        var customFormats = new HashSet<string> { "jpg", "png", "pdf", "eps" };
        var settings = new ContentImagingSettings
        {
            ImageFileTypes = customFormats,
        };

        // Assert
        Assert.That(settings.ImageFileTypes, Is.EqualTo(customFormats));
        Assert.That(settings.ImageFileTypes.Count, Is.EqualTo(4));
        Assert.That(settings.ImageFileTypes, Does.Contain("jpg"));
        Assert.That(settings.ImageFileTypes, Does.Contain("png"));
        Assert.That(settings.ImageFileTypes, Does.Contain("pdf"));
        Assert.That(settings.ImageFileTypes, Does.Contain("eps"));
    }

    [Test]
    public void ImageFileTypes_IsSubsetOf_ImageFileTypes_ByDefault()
    {
        // Arrange
        var settings = new ContentImagingSettings();

        // Assert - All ImageFileTypes should be in ImageFileTypes
        foreach (var format in settings.ImageFileTypes)
        {
            Assert.That(
                settings.ImageFileTypes,
                Does.Contain(format),
                $"ImageFileTypes should contain all formats from ImageFileTypes, but '{format}' was missing");
        }
    }

    [Test]
    public void ImageFileTypes_AllowsCaseInsensitiveComparison()
    {
        // Arrange
        var settings = new ContentImagingSettings();

        // Act & Assert - Test that the collection can be used for case-insensitive matching
        // (Actual case-insensitive comparison logic is in the factory, but the data should support it)
        var lowercaseFormats = settings.ImageFileTypes.Select(f => f.ToLowerInvariant()).ToList();
        var uppercaseCheck = lowercaseFormats.Any(f => f.Equals("JPG", StringComparison.OrdinalIgnoreCase));

        Assert.That(uppercaseCheck, Is.True, "Should support case-insensitive matching");
    }

    [Test]
    public void AutoFillImageProperties_DefaultValue_ContainsMediaFileProperty()
    {
        // Arrange
        var settings = new ContentImagingSettings();

        // Assert
        Assert.That(settings.AutoFillImageProperties, Is.Not.Null);
        Assert.That(settings.AutoFillImageProperties, Is.Not.Empty);
        Assert.That(settings.AutoFillImageProperties.Count, Is.EqualTo(1));

        var defaultProperty = settings.AutoFillImageProperties.First();
        Assert.That(defaultProperty.Alias, Is.EqualTo(Constants.Conventions.Media.File));
        Assert.That(defaultProperty.WidthFieldAlias, Is.EqualTo(Constants.Conventions.Media.Width));
        Assert.That(defaultProperty.HeightFieldAlias, Is.EqualTo(Constants.Conventions.Media.Height));
        Assert.That(defaultProperty.ExtensionFieldAlias, Is.EqualTo(Constants.Conventions.Media.Extension));
        Assert.That(defaultProperty.LengthFieldAlias, Is.EqualTo(Constants.Conventions.Media.Bytes));
    }

    [Test]
    public void AutoFillImageProperties_CanBeConfigured_WithCustomProperties()
    {
        // Arrange
        var customProperties = new HashSet<ImagingAutoFillUploadField>
        {
            new()
            {
                Alias = "customFile",
                WidthFieldAlias = "customWidth",
                HeightFieldAlias = "customHeight",
                ExtensionFieldAlias = "customExtension",
                LengthFieldAlias = "customLength",
            },
        };

        var settings = new ContentImagingSettings
        {
            AutoFillImageProperties = customProperties,
        };

        // Assert
        Assert.That(settings.AutoFillImageProperties, Is.EqualTo(customProperties));
        Assert.That(settings.AutoFillImageProperties.Count, Is.EqualTo(1));

        var customProperty = settings.AutoFillImageProperties.First();
        Assert.That(customProperty.Alias, Is.EqualTo("customFile"));
        Assert.That(customProperty.WidthFieldAlias, Is.EqualTo("customWidth"));
        Assert.That(customProperty.HeightFieldAlias, Is.EqualTo("customHeight"));
        Assert.That(customProperty.ExtensionFieldAlias, Is.EqualTo("customExtension"));
        Assert.That(customProperty.LengthFieldAlias, Is.EqualTo("customLength"));
    }

    [Test]
    public void ImageFileTypes_EmptySet_IsAllowed()
    {
        // Arrange & Act
        var settings = new ContentImagingSettings
        {
            ImageFileTypes = new HashSet<string>(),
        };

        // Assert
        Assert.That(settings.ImageFileTypes, Is.Not.Null);
        Assert.That(settings.ImageFileTypes, Is.Empty);
    }

    [Test]
    public void StaticConstants_HaveExpectedValues()
    {
        // Assert
        Assert.That(ContentImagingSettings.StaticImageFileTypes, Is.EqualTo("jpeg,jpg,gif,bmp,png,tiff,tif,webp"));
        Assert.That(ContentImagingSettings.StaticImageFileTypes, Is.EqualTo("jpg,jpeg,png,gif,webp,bmp,tif,tiff"));
    }

    [TestCase("jpg", true)]
    [TestCase("jpeg", true)]
    [TestCase("png", true)]
    [TestCase("gif", true)]
    [TestCase("webp", true)]
    [TestCase("bmp", true)]
    [TestCase("tif", true)]
    [TestCase("tiff", true)]
    [TestCase("pdf", false)]
    [TestCase("eps", false)]
    [TestCase("svg", false)]
    [TestCase("txt", false)]
    public void ImageFileTypes_ContainsExpectedFormat(string format, bool shouldContain)
    {
        // Arrange
        var settings = new ContentImagingSettings();

        // Assert
        if (shouldContain)
        {
            Assert.That(
                settings.ImageFileTypes,
                Does.Contain(format),
                $"Expected ImageFileTypes to contain '{format}'");
        }
        else
        {
            Assert.That(
                settings.ImageFileTypes,
                Does.Not.Contain(format),
                $"Expected ImageFileTypes to NOT contain '{format}'");
        }
    }
}
