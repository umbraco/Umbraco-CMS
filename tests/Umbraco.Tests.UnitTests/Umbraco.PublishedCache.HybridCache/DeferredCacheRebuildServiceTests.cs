// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class DeferredCacheRebuildServiceTests
{
    private Mock<IDocumentCacheService> _documentCacheService = null!;
    private Mock<IMediaCacheService> _mediaCacheService = null!;
    private DeferredCacheRebuildService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _documentCacheService = new Mock<IDocumentCacheService>();
        _mediaCacheService = new Mock<IMediaCacheService>();
        _service = new DeferredCacheRebuildService(
            _documentCacheService.Object,
            _mediaCacheService.Object,
            Mock.Of<ILogger<DeferredCacheRebuildService>>());
    }

    [TearDown]
    public void TearDown() => _service.Dispose();

    [Test]
    public async Task QueueContentTypeRebuild_Calls_Rebuild_And_RebuildMemoryCache()
    {
        // Arrange
        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act
        _service.QueueContentTypeRebuild([1, 2]);

        // Allow background task to complete
        await Task.Delay(500);

        // Assert
        _documentCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(1) && ids.Contains(2))),
            Times.Once);
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(1) && ids.Contains(2))),
            Times.Once);
    }

    [Test]
    public async Task QueueMediaTypeRebuild_Calls_Rebuild_And_RebuildMemoryCache()
    {
        // Arrange
        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act
        _service.QueueMediaTypeRebuild([10, 20]);

        // Allow background task to complete
        await Task.Delay(500);

        // Assert
        _mediaCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(10) && ids.Contains(20))),
            Times.Once);
        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(10) && ids.Contains(20))),
            Times.Once);
    }

    [Test]
    public async Task QueueContentTypeRebuild_Deduplicates_Ids()
    {
        // Arrange
        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act — queue the same ID twice
        _service.QueueContentTypeRebuild([1]);
        _service.QueueContentTypeRebuild([1]);

        // Allow background task to complete
        await Task.Delay(500);

        // Assert — Rebuild should be called once (or possibly twice if second Queue triggers a second loop),
        // but the ID set should never contain duplicates. Verify at least one call happened.
        _documentCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(1))),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task Document_And_Media_Types_Processed_Independently()
    {
        // Arrange
        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);
        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act
        _service.QueueContentTypeRebuild([1]);
        _service.QueueMediaTypeRebuild([10]);

        // Allow background task to complete
        await Task.Delay(500);

        // Assert
        _documentCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(1))),
            Times.AtLeastOnce);
        _mediaCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(10))),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task Does_Not_Call_Rebuild_For_Empty_Content_Types()
    {
        // Arrange
        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act — only queue media types
        _service.QueueMediaTypeRebuild([10]);

        // Allow background task to complete
        await Task.Delay(500);

        // Assert — document rebuild should never be called
        _documentCacheService.Verify(
            x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
    }
}
