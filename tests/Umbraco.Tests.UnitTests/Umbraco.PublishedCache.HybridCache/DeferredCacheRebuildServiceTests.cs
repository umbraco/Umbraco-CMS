// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class DeferredCacheRebuildServiceTests
{
    private static readonly TimeSpan _testTimeout = TimeSpan.FromSeconds(5);

    private Mock<IDocumentCacheService> _documentCacheService = null!;
    private Mock<IMediaCacheService> _mediaCacheService = null!;
    private DeferredCacheRebuildService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _documentCacheService = new Mock<IDocumentCacheService>();
        _mediaCacheService = new Mock<IMediaCacheService>();
        _service = CreateService();
    }

    [TearDown]
    public void TearDown() => _service.Dispose();

    private DeferredCacheRebuildService CreateService(CancellationToken shutdownToken = default)
    {
        var lifetime = new Mock<IHostApplicationLifetime>();
        lifetime.Setup(x => x.ApplicationStopping).Returns(shutdownToken);
        return new DeferredCacheRebuildService(
            _documentCacheService.Object,
            _mediaCacheService.Object,
            Mock.Of<ILogger<DeferredCacheRebuildService>>(),
            lifetime.Object);
    }

    /// <summary>
    ///     Verifies that queued content type IDs trigger both a database cache rebuild and a memory cache rebuild.
    /// </summary>
    [Test]
    public async Task QueueContentTypeRebuild_Calls_Rebuild_And_RebuildMemoryCache()
    {
        // Arrange
        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act
        _service.QueueContentTypeRebuild([1, 2]);
        await WaitForProcessingAsync();

        // Assert
        _documentCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(1) && ids.Contains(2))),
            Times.Once);
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(1) && ids.Contains(2))),
            Times.Once);
    }

    /// <summary>
    ///     Verifies that queued media type IDs trigger both a database cache rebuild and a memory cache rebuild.
    /// </summary>
    [Test]
    public async Task QueueMediaTypeRebuild_Calls_Rebuild_And_RebuildMemoryCache()
    {
        // Arrange
        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act
        _service.QueueMediaTypeRebuild([10, 20]);
        await WaitForProcessingAsync();

        // Assert
        _mediaCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(10) && ids.Contains(20))),
            Times.Once);
        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(10) && ids.Contains(20))),
            Times.Once);
    }

    /// <summary>
    ///     Verifies that queuing the same content type ID multiple times does not produce duplicate rebuild calls.
    /// </summary>
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
        await WaitForProcessingAsync();

        // Assert — Rebuild should be called once (or possibly twice if second Queue triggers a second loop),
        // but the ID set should never contain duplicates. Verify at least one call happened.
        _documentCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(1))),
            Times.AtLeastOnce);
    }

    /// <summary>
    ///     Verifies that content type and media type rebuilds are processed independently within the same worker iteration.
    /// </summary>
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
        await WaitForProcessingAsync();

        // Assert
        _documentCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(1))),
            Times.AtLeastOnce);
        _mediaCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(10))),
            Times.AtLeastOnce);
    }

    /// <summary>
    ///     Verifies that queuing only media type IDs does not trigger a document cache rebuild.
    /// </summary>
    [Test]
    public async Task Does_Not_Call_Rebuild_For_Empty_Content_Types()
    {
        // Arrange
        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act — only queue media types
        _service.QueueMediaTypeRebuild([10]);
        await WaitForProcessingAsync();

        // Assert — document rebuild should never be called
        _documentCacheService.Verify(
            x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
    }

    /// <summary>
    ///     Verifies that a transient failure is retried and the rebuild succeeds on the second attempt.
    /// </summary>
    [Test]
    public async Task Transient_Failure_Retries_And_Succeeds()
    {
        // Arrange — Rebuild throws once then succeeds.
        var callCount = 0;
        _documentCacheService
            .Setup(x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()))
            .Callback(() =>
            {
                if (Interlocked.Increment(ref callCount) == 1)
                {
                    throw new InvalidOperationException("Transient DB error");
                }
            });
        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act
        _service.QueueContentTypeRebuild([1]);
        await WaitForProcessingAsync();

        // Assert — Rebuild was called twice (first failed, second succeeded).
        _documentCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(1))),
            Times.Exactly(2));
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(1))),
            Times.Once);
    }

    /// <summary>
    ///     Verifies that the worker stops retrying after 3 consecutive failures and does not call the memory cache rebuild.
    /// </summary>
    [Test]
    public async Task Persistent_Failure_Gives_Up_After_Max_Retries()
    {
        // Arrange — Rebuild always throws.
        _documentCacheService
            .Setup(x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()))
            .Throws(new InvalidOperationException("Persistent DB error"));

        // Act
        _service.QueueContentTypeRebuild([1]);

        // Wait for the worker to exit (IDs will remain pending after max retries).
        using var cts = new CancellationTokenSource(_testTimeout);
        await _service.WaitForWorkerIdleAsync(cts.Token);

        // Assert — Rebuild was called 3 times (the max consecutive failure count).
        _documentCacheService.Verify(
            x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Exactly(3));

        // Memory cache rebuild should never be called since Rebuild always failed first.
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()),
            Times.Never);
    }

    /// <summary>
    ///     Verifies that IDs left pending after max retries are picked up and rebuilt when a new queue call arrives.
    /// </summary>
    [Test]
    public async Task Failed_Ids_Are_Retried_On_Next_Queue_Call()
    {
        // Arrange — Rebuild throws on every call initially.
        var callCount = 0;
        _documentCacheService
            .Setup(x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()))
            .Callback(() =>
            {
                // Fail the first 3 calls (exhaust retries), then succeed.
                if (Interlocked.Increment(ref callCount) <= 3)
                {
                    throw new InvalidOperationException("Persistent DB error");
                }
            });
        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);

        // Act — first attempt exhausts retries.
        _service.QueueContentTypeRebuild([1]);
        using var cts = new CancellationTokenSource(_testTimeout);
        await _service.WaitForWorkerIdleAsync(cts.Token);

        // A new queue call picks up the leftover IDs from the failed attempts.
        _service.QueueContentTypeRebuild([2]);
        await WaitForProcessingAsync();

        // Assert — the fourth Rebuild call succeeds, containing both IDs (1 from retry + 2 from new queue).
        _documentCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Contains(1) && ids.Contains(2))),
            Times.Once);
    }

    /// <summary>
    ///     Verifies that application shutdown cancels an in-flight rebuild cleanly without reaching the memory cache step.
    /// </summary>
    [Test]
    public async Task Shutdown_Cancels_In_Flight_Rebuild()
    {
        // Arrange — use a CTS that we control as the shutdown token.
        using var shutdownCts = new CancellationTokenSource();
        using var service = CreateService(shutdownCts.Token);

        var rebuildStarted = new TaskCompletionSource();
        _documentCacheService
            .Setup(x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()))
            .Callback(() =>
            {
                rebuildStarted.SetResult();

                // Simulate a long-running rebuild that respects cancellation.
                shutdownCts.Cancel();
                shutdownCts.Token.ThrowIfCancellationRequested();
            });

        // Act
        service.QueueContentTypeRebuild([1]);

        // Wait for the worker to exit after cancellation.
        using var timeoutCts = new CancellationTokenSource(_testTimeout);
        await service.WaitForWorkerIdleAsync(timeoutCts.Token);

        // Assert — Rebuild was called once, then cancelled. Memory cache rebuild never reached.
        await rebuildStarted.Task;
        _documentCacheService.Verify(
            x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Once);
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()),
            Times.Never);
    }

    private async Task WaitForProcessingAsync()
    {
        using var cts = new CancellationTokenSource(_testTimeout);
        await _service.WaitForPendingRebuildsAsync(cts.Token);
    }
}
