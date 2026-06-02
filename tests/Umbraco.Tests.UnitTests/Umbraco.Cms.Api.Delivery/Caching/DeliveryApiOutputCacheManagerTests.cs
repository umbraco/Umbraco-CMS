using Microsoft.AspNetCore.OutputCaching;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Caching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Caching;

[TestFixture]
public class DeliveryApiOutputCacheManagerTests
{
    private Mock<IOutputCacheStore> _storeMock = null!;
    private DeliveryApiOutputCacheManager _manager = null!;

    [SetUp]
    public void SetUp()
    {
        _storeMock = new Mock<IOutputCacheStore>();
        _storeMock
            .Setup(s => s.EvictByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _manager = new DeliveryApiOutputCacheManager(_storeMock.Object);
    }

    [Test]
    public async Task EvictContentAsync_EvictsCorrectTag()
    {
        var key = Guid.NewGuid();

        await _manager.EvictContentAsync(key);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{key}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EvictMediaAsync_EvictsCorrectTag()
    {
        var key = Guid.NewGuid();

        await _manager.EvictMediaAsync(key);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-media-{key}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EvictAllContentAsync_EvictsAllContentTag()
    {
        await _manager.EvictAllContentAsync();

        _storeMock.Verify(
            s => s.EvictByTagAsync("umb-dapi-content-all", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EvictAllMediaAsync_EvictsAllMediaTag()
    {
        await _manager.EvictAllMediaAsync();

        _storeMock.Verify(
            s => s.EvictByTagAsync("umb-dapi-media-all", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EvictAllAsync_EvictsAllTag()
    {
        await _manager.EvictAllAsync();

        _storeMock.Verify(
            s => s.EvictByTagAsync("umb-dapi-all", It.IsAny<CancellationToken>()),
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
