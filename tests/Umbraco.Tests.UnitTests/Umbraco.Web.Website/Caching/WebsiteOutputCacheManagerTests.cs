using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
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
        _storeMock = new Mock<IOutputCacheStore>(MockBehavior.Strict);
        _storeMock
            .Setup(s => s.EvictByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(_storeMock.Object);
        _manager = new WebsiteOutputCacheManager(services.BuildServiceProvider());
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

    [Test]
    public async Task EvictContentAsync_WhenStoreNull_NoOp()
    {
        var services = new ServiceCollection();
        var manager = new WebsiteOutputCacheManager(services.BuildServiceProvider());

        // Should not throw.
        await manager.EvictContentAsync(Guid.NewGuid());
    }
}
