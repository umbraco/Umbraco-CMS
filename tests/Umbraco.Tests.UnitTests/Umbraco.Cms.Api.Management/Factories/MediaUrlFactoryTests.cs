using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class MediaUrlFactoryTests
{
    private const string BaseUrl = "https://example.com";
    private const string MediaPath = "/media/image.jpg";
    private const string PropertyAlias = Constants.Conventions.Media.File;

    [Test]
    public void CreateUrls_WithRecycleBinProtectionDisabled_ReturnsUrlWithoutDeletedSuffix()
    {
        // Arrange
        var factory = CreateFactory(enableMediaRecycleBinProtection: false);
        var media = CreateMediaWithUrl(MediaPath);

        // Act
        var result = factory.CreateUrls(media).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url, Is.EqualTo($"{BaseUrl}{MediaPath}"));
        Assert.That(result[0].Culture, Is.Null);
    }

    [Test]
    public void CreateUrls_WithRecycleBinProtectionEnabled_AndMediaNotTrashed_ReturnsUrlWithoutDeletedSuffix()
    {
        // Arrange
        var factory = CreateFactory(enableMediaRecycleBinProtection: true);
        var media = CreateMediaWithUrl(MediaPath, trashed: false);

        // Act
        var result = factory.CreateUrls(media).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url, Is.EqualTo($"{BaseUrl}{MediaPath}"));
        Assert.That(result[0].Culture, Is.Null);
    }

    [Test]
    public void CreateUrls_WithRecycleBinProtectionEnabled_AndMediaTrashed_ReturnsUrlWithDeletedSuffix()
    {
        // Arrange
        var factory = CreateFactory(enableMediaRecycleBinProtection: true);
        var media = CreateMediaWithUrl(MediaPath, trashed: true);

        // Act
        var result = factory.CreateUrls(media).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url, Is.EqualTo($"{BaseUrl}/media/image{Constants.Conventions.Media.TrashedMediaSuffix}.jpg"));
        Assert.That(result[0].Culture, Is.Null);
    }

    [Test]
    public void CreateUrls_WithRecycleBinProtectionDisabled_AndMediaTrashed_ReturnsUrlWithoutDeletedSuffix()
    {
        // Arrange
        var factory = CreateFactory(enableMediaRecycleBinProtection: false);
        var media = CreateMediaWithUrl(MediaPath, trashed: true);

        // Act
        var result = factory.CreateUrls(media).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url, Is.EqualTo($"{BaseUrl}{MediaPath}"));
        Assert.That(result[0].Culture, Is.Null);
    }

    [Test]
    public void CreateUrls_WithNoUrls_ReturnsEmptyCollection()
    {
        // Arrange
        var factory = CreateFactory(enableMediaRecycleBinProtection: false);
        var media = CreateMediaWithUrl(null);

        // Act
        var result = factory.CreateUrls(media).ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CreateUrls_WithMultipleUrls_ReturnsAllUrls()
    {
        // Arrange
        const string secondPropertyAlias = "secondImage";
        const string secondMediaPath = "/media/second.png";

        var contentSettings = CreateContentSettings(false, PropertyAlias, secondPropertyAlias);
        var mediaUrlGenerators = CreateMediaUrlGeneratorCollection();
        var absoluteUrlBuilder = CreateAbsoluteUrlBuilder();

        var factory = new MediaUrlFactory(
            Options.Create(contentSettings),
            mediaUrlGenerators,
            absoluteUrlBuilder);

        var media = CreateMediaWithMultipleUrls(
            (PropertyAlias, MediaPath),
            (secondPropertyAlias, secondMediaPath));

        // Act
        var result = factory.CreateUrls(media).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Url, Is.EqualTo($"{BaseUrl}{MediaPath}"));
        Assert.That(result[1].Url, Is.EqualTo($"{BaseUrl}{secondMediaPath}"));
    }

    [Test]
    public void CreateUrlSets_WithSingleMedia_ReturnsCorrectResponseModel()
    {
        // Arrange
        var factory = CreateFactory(enableMediaRecycleBinProtection: false);
        var mediaKey = Guid.NewGuid();
        var media = CreateMediaWithUrl(MediaPath, mediaKey);

        // Act
        var result = factory.CreateUrlSets([media]).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(mediaKey));
        Assert.That(result[0].UrlInfos.Count(), Is.EqualTo(1));
        Assert.That(result[0].UrlInfos.First().Url, Is.EqualTo($"{BaseUrl}{MediaPath}"));
    }

    [Test]
    public void CreateUrlSets_WithMultipleMedia_ReturnsAllMediaUrlInfos()
    {
        // Arrange
        var factory = CreateFactory(enableMediaRecycleBinProtection: false);
        var mediaKey1 = Guid.NewGuid();
        var mediaKey2 = Guid.NewGuid();
        const string mediaPath1 = "/media/image1.jpg";
        const string mediaPath2 = "/media/image2.png";

        var media1 = CreateMediaWithUrl(mediaPath1, mediaKey1);
        var media2 = CreateMediaWithUrl(mediaPath2, mediaKey2);

        // Act
        var result = factory.CreateUrlSets([media1, media2]).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));

        Assert.That(result[0].Id, Is.EqualTo(mediaKey1));
        Assert.That(result[0].UrlInfos.First().Url, Is.EqualTo($"{BaseUrl}{mediaPath1}"));

        Assert.That(result[1].Id, Is.EqualTo(mediaKey2));
        Assert.That(result[1].UrlInfos.First().Url, Is.EqualTo($"{BaseUrl}{mediaPath2}"));
    }

    private static MediaUrlFactory CreateFactory(bool enableMediaRecycleBinProtection)
    {
        var contentSettings = CreateContentSettings(enableMediaRecycleBinProtection, PropertyAlias);
        var mediaUrlGenerators = CreateMediaUrlGeneratorCollection();
        var absoluteUrlBuilder = CreateAbsoluteUrlBuilder();

        return new MediaUrlFactory(
            Options.Create(contentSettings),
            mediaUrlGenerators,
            absoluteUrlBuilder);
    }

    private static ContentSettings CreateContentSettings(bool enableMediaRecycleBinProtection, params string[] propertyAliases)
    {
        var autoFillProperties = propertyAliases
            .Select(alias => new ImagingAutoFillUploadField { Alias = alias })
            .ToHashSet();

        return new ContentSettings
        {
            EnableMediaRecycleBinProtection = enableMediaRecycleBinProtection,
            Imaging = new ContentImagingSettings
            {
                AutoFillImageProperties = autoFillProperties,
            },
        };
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
        mock
            .Setup(x => x.ToAbsoluteUrl(It.IsAny<string>()))
            .Returns<string>(url => new Uri($"{BaseUrl}{url}"));

        return mock.Object;
    }

    private static IMedia CreateMediaWithUrl(string? mediaPath, Guid? key = null, bool trashed = false)
    {
        var urlMappings = mediaPath != null
            ? [(PropertyAlias, mediaPath)]
            : Array.Empty<(string, string)>();

        return CreateMedia(key ?? Guid.NewGuid(), urlMappings, trashed);
    }

    private static IMedia CreateMediaWithMultipleUrls(params (string Alias, string Path)[] urlMappings)
        => CreateMedia(Guid.NewGuid(), urlMappings, trashed: false);

    private static IMedia CreateMedia(Guid key, (string Alias, string Path)[] urlMappings, bool trashed = false)
    {
        var mediaMock = new Mock<IMedia>();
        mediaMock.SetupGet(m => m.Key).Returns(key);
        mediaMock.SetupGet(m => m.Trashed).Returns(trashed);

        var propertiesMock = new Mock<IPropertyCollection>();

        foreach (var (alias, path) in urlMappings)
        {
            IProperty? outProperty = CreatePropertyMock(alias, path);
            propertiesMock
                .Setup(p => p.TryGetValue(alias, out outProperty))
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
