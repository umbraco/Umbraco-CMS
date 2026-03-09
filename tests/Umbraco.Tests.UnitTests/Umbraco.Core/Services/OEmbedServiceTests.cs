using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class OEmbedServiceTests
{
    [Test]
    public void MatchesUrlScheme_WithMatchingPattern_ReturnsTrue()
    {
        var patterns = new[] { @"^https?:\/\/(www\.)?example\.com\/" };

        var result = OEmbedService.MatchesUrlScheme("https://example.com/video/123", patterns);

        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesUrlScheme_WithNonMatchingPattern_ReturnsFalse()
    {
        var patterns = new[] { @"^https?:\/\/(www\.)?example\.com\/" };

        var result = OEmbedService.MatchesUrlScheme("https://other.com/video/123", patterns);

        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesUrlScheme_WithMultiplePatterns_ReturnsTrueIfAnyMatches()
    {
        var patterns = new[]
        {
            @"^https?:\/\/(www\.)?first\.com\/",
            @"^https?:\/\/(www\.)?second\.com\/",
            @"^https?:\/\/(www\.)?third\.com\/",
        };

        var result = OEmbedService.MatchesUrlScheme("https://second.com/resource", patterns);

        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesUrlScheme_WithEmptyPatterns_ReturnsFalse()
    {
        var patterns = Array.Empty<string>();

        var result = OEmbedService.MatchesUrlScheme("https://example.com/video/123", patterns);

        Assert.That(result, Is.False);
    }

    [Test]
    public void MatchesUrlScheme_IsCaseInsensitive()
    {
        var patterns = new[] { @"^https?:\/\/(www\.)?example\.com\/" };

        var result = OEmbedService.MatchesUrlScheme("HTTPS://EXAMPLE.COM/VIDEO/123", patterns);

        Assert.That(result, Is.True);
    }

    [Test]
    public void MatchesUrlScheme_CachingDoesNotAffectResults()
    {
        var patterns = new[] { @"^https?:\/\/(www\.)?cached\.com\/" };

        // Call multiple times with same pattern to exercise cache
        var result1 = OEmbedService.MatchesUrlScheme("https://cached.com/a", patterns);
        var result2 = OEmbedService.MatchesUrlScheme("https://cached.com/b", patterns);
        var result3 = OEmbedService.MatchesUrlScheme("https://other.com/c", patterns);

        Assert.That(result1, Is.True);
        Assert.That(result2, Is.True);
        Assert.That(result3, Is.False);
    }

    [Test]
    public async Task GetMarkupAsync_WithNoMatchingProvider_ReturnsNoSupportedProvider()
    {
        // Arrange
        var provider = CreateMockProvider("https://provider.com/", @"^https?:\/\/provider\.com\/");
        var service = CreateService(provider);

        // Act
        var result = await service.GetMarkupAsync(new Uri("https://unknown.com/video"), null, null, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(OEmbedOperationStatus.NoSupportedProvider));
    }

    [Test]
    public async Task GetMarkupAsync_WithMatchingProvider_ReturnsSuccess()
    {
        // Arrange
        var expectedMarkup = "<iframe src=\"https://embed.example.com/123\"></iframe>";
        var provider = CreateMockProvider(
            "https://example.com/oembed",
            @"^https?:\/\/(www\.)?example\.com\/",
            expectedMarkup);
        var service = CreateService(provider);

        // Act
        var result = await service.GetMarkupAsync(new Uri("https://example.com/video/123"), 800, 600, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(OEmbedOperationStatus.Success));
        Assert.That(result.Result, Is.EqualTo(expectedMarkup));
    }

    [Test]
    public async Task GetMarkupAsync_WhenProviderReturnsNull_ReturnsProviderReturnedInvalidResult()
    {
        // Arrange
        var provider = CreateMockProvider(
            "https://example.com/oembed",
            @"^https?:\/\/(www\.)?example\.com\/",
            markupResult: null);
        var service = CreateService(provider);

        // Act
        var result = await service.GetMarkupAsync(new Uri("https://example.com/video/123"), null, null, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(OEmbedOperationStatus.ProviderReturnedInvalidResult));
    }

    [Test]
    public async Task GetMarkupAsync_WhenProviderThrows_ReturnsUnexpectedException()
    {
        // Arrange
        var providerMock = new Mock<IEmbedProvider>();
        providerMock.Setup(p => p.UrlSchemeRegex).Returns([@"^https?:\/\/(www\.)?example\.com\/"]);
        providerMock.Setup(p => p.GetMarkupAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));
        var service = CreateService(providerMock.Object);

        // Act
        var result = await service.GetMarkupAsync(new Uri("https://example.com/video/123"), null, null, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(OEmbedOperationStatus.UnexpectedException));
    }

    [Test]
    public async Task GetMarkupAsync_WithMultipleProviders_SelectsCorrectProvider()
    {
        // Arrange
        var youtubeMarkup = "<iframe src=\"youtube\"></iframe>";
        var vimeoMarkup = "<iframe src=\"vimeo\"></iframe>";

        var youtubeProvider = CreateMockProvider(
            "https://youtube.com/oembed",
            @"^https?:\/\/(www\.)?youtube\.com\/",
            youtubeMarkup);
        var vimeoProvider = CreateMockProvider(
            "https://vimeo.com/oembed",
            @"^https?:\/\/(www\.)?vimeo\.com\/",
            vimeoMarkup);

        var service = CreateService(youtubeProvider, vimeoProvider);

        // Act
        var result = await service.GetMarkupAsync(new Uri("https://vimeo.com/123456"), null, null, CancellationToken.None);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result, Is.EqualTo(vimeoMarkup));
    }

    [Test]
    public async Task GetMarkupAsync_PassesDimensionsToProvider()
    {
        // Arrange
        var providerMock = new Mock<IEmbedProvider>();
        providerMock.Setup(p => p.UrlSchemeRegex).Returns([@"^https?:\/\/(www\.)?example\.com\/"]);
        providerMock.Setup(p => p.GetMarkupAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("<iframe></iframe>");
        var service = CreateService(providerMock.Object);

        // Act
        await service.GetMarkupAsync(new Uri("https://example.com/video/123"), 1920, 1080, CancellationToken.None);

        // Assert
        providerMock.Verify(p => p.GetMarkupAsync(
            "https://example.com/video/123",
            1920,
            1080,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static OEmbedService CreateService(params IEmbedProvider[] providers)
    {
        var collection = new EmbedProvidersCollection(() => providers);
        var logger = Mock.Of<ILogger<OEmbedService>>();
        return new OEmbedService(collection, logger);
    }

    private static IEmbedProvider CreateMockProvider(string apiEndpoint, string urlPattern, string? markupResult = "<iframe></iframe>")
    {
        var mock = new Mock<IEmbedProvider>();
        mock.Setup(p => p.ApiEndpoint).Returns(apiEndpoint);
        mock.Setup(p => p.UrlSchemeRegex).Returns([urlPattern]);
        mock.Setup(p => p.RequestParams).Returns(new Dictionary<string, string>());
        mock.Setup(p => p.GetMarkupAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(markupResult);
        return mock.Object;
    }
}
