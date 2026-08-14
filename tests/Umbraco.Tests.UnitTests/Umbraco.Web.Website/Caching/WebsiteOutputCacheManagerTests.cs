using Microsoft.AspNetCore.OutputCaching;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Web.Website.Caching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Caching;

[TestFixture]
public class WebsiteOutputCacheManagerTests
{
    private Mock<IOutputCacheStore> _storeMock = null!;
    private WebsiteOutputCacheManager _manager = null!;

    [SetUp]
    public void SetUp()
    {
        _storeMock = new Mock<IOutputCacheStore>();
        _storeMock
            .Setup(s => s.EvictByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _manager = new WebsiteOutputCacheManager(_storeMock.Object);
    }

    [Test]
    public async Task EvictContentAsync_EvictsCorrectTag()
    {
        var key = Guid.NewGuid();

        await _manager.EvictContentAsync(key);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-content-{key}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EvictAllAsync_EvictsAllTag()
    {
        await _manager.EvictAllAsync();

        _storeMock.Verify(
            s => s.EvictByTagAsync("umb-content-all", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EvictByTagAsync_PassesThrough()
    {
        await _manager.EvictByTagAsync("custom-tag");

        _storeMock.Verify(
            s => s.EvictByTagAsync("custom-tag", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
