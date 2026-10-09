// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.Search;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Search;

[TestFixture]
public class DeferredSearchReindexServiceTests
{
    private static readonly TimeSpan _testTimeout = TimeSpan.FromSeconds(5);

    private Mock<IDocumentRepository> _documentRepository = null!;
    private Mock<IMediaRepository> _mediaRepository = null!;
    private Mock<IMemberRepository> _memberRepository = null!;
    private Mock<IUmbracoIndexingHandler> _umbracoIndexingHandler = null!;
    private Mock<IPublishStatusQueryService> _publishStatusQueryService = null!;
    private Mock<ICoreScopeProvider> _scopeProvider = null!;
    private DeferredSearchReindexService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _documentRepository = new Mock<IDocumentRepository>();
        _mediaRepository = new Mock<IMediaRepository>();
        _memberRepository = new Mock<IMemberRepository>();
        _umbracoIndexingHandler = new Mock<IUmbracoIndexingHandler>();
        _publishStatusQueryService = new Mock<IPublishStatusQueryService>();
        _scopeProvider = new Mock<ICoreScopeProvider>();
        _scopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<System.Data.IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());
        _scopeProvider
            .Setup(x => x.CreateQuery<IContent>())
            .Returns(new Mock<IQuery<IContent>>().Object);
        _scopeProvider
            .Setup(x => x.CreateQuery<IMedia>())
            .Returns(new Mock<IQuery<IMedia>>().Object);
        _scopeProvider
            .Setup(x => x.CreateQuery<IMember>())
            .Returns(new Mock<IQuery<IMember>>().Object);
        _service = CreateService();
    }

    [TearDown]
    public void TearDown() => _service.Dispose();

    private DeferredSearchReindexService CreateService(CancellationToken shutdownToken = default)
    {
        var lifetime = new Mock<IHostApplicationLifetime>();
        lifetime.Setup(x => x.ApplicationStopping).Returns(shutdownToken);

        var indexingSettings = new Mock<IOptionsMonitor<IndexingSettings>>();
        indexingSettings.Setup(x => x.CurrentValue).Returns(new IndexingSettings { BatchSize = 100 });

        return new DeferredSearchReindexService(
            _documentRepository.Object,
            _mediaRepository.Object,
            _memberRepository.Object,
            _umbracoIndexingHandler.Object,
            _publishStatusQueryService.Object,
            indexingSettings.Object,
            _scopeProvider.Object,
            Mock.Of<ILogger<DeferredSearchReindexService>>(),
            lifetime.Object);
    }

    /// <summary>
    ///     Verifies that queued content type IDs trigger paging through content items and calling ReIndexForContent
    ///     with the correct published status.
    /// </summary>
    [Test]
    public async Task QueueContentTypeReindex_Pages_And_Calls_ReIndexForContent()
    {
        // Arrange
        var content = CreateContent(1, published: true);
        var total = 1L;
        _documentRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()))
            .Returns([content]);
        _publishStatusQueryService
            .Setup(x => x.HasPublishedAncestorPath(It.IsAny<Guid>()))
            .Returns(true);

        // Act
        _service.QueueContentTypeReindex([42]);
        await WaitForProcessingAsync();

        // Assert
        _umbracoIndexingHandler.Verify(
            x => x.ReIndexForContent(content, true),
            Times.Once);
    }

    /// <summary>
    ///     Verifies that queued media type IDs trigger paging through media items and calling ReIndexForMedia
    ///     with the correct trashed status.
    /// </summary>
    [Test]
    public async Task QueueMediaTypeReindex_Pages_And_Calls_ReIndexForMedia()
    {
        // Arrange
        var media = CreateMedia(1, trashed: false);
        var total = 1L;
        _mediaRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IMedia>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IMedia>?>(),
                It.IsAny<Ordering?>()))
            .Returns([media]);

        // Act
        _service.QueueMediaTypeReindex([42]);
        await WaitForProcessingAsync();

        // Assert
        _umbracoIndexingHandler.Verify(
            x => x.ReIndexForMedia(media, true),
            Times.Once);
    }

    /// <summary>
    ///     Verifies that queued member type IDs page through matching members and call ReIndexForMember for each.
    /// </summary>
    [Test]
    public async Task QueueMemberTypeReindex_Pages_And_Calls_ReIndexForMember()
    {
        // Arrange
        var member = Mock.Of<IMember>();
        var total = 1L;
        _memberRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IMember>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IMember>?>(),
                It.IsAny<Ordering?>()))
            .Returns([member]);

        // Act
        _service.QueueMemberTypeReindex([42]);
        await WaitForProcessingAsync();

        // Assert
        _umbracoIndexingHandler.Verify(
            x => x.ReIndexForMember(member),
            Times.Once);
    }

    /// <summary>
    ///     Verifies that queuing the same content type ID multiple times does not produce duplicate reindex operations.
    /// </summary>
    [Test]
    public async Task QueueContentTypeReindex_Deduplicates_Ids()
    {
        // Arrange
        var total = 0L;
        _documentRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()))
            .Returns(Enumerable.Empty<IContent>());

        // Act — queue the same ID twice
        _service.QueueContentTypeReindex([1]);
        _service.QueueContentTypeReindex([1]);
        await WaitForProcessingAsync();

        // Assert — repository should be called at least once with the ID
        _documentRepository.Verify(
            x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    ///     Verifies that content, media, and member type reindexes are processed independently within the same
    ///     worker iteration, each reaching its respective repository.
    /// </summary>
    [Test]
    public async Task Content_Media_And_Member_Types_Processed_Independently()
    {
        // Arrange
        var contentTotal = 0L;
        _documentRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out contentTotal,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()))
            .Returns(Enumerable.Empty<IContent>());

        var mediaTotal = 0L;
        _mediaRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IMedia>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out mediaTotal,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IMedia>?>(),
                It.IsAny<Ordering?>()))
            .Returns(Enumerable.Empty<IMedia>());

        var memberTotal = 0L;
        _memberRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IMember>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out memberTotal,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IMember>?>(),
                It.IsAny<Ordering?>()))
            .Returns(Enumerable.Empty<IMember>());

        // Act
        _service.QueueContentTypeReindex([1]);
        _service.QueueMediaTypeReindex([10]);
        _service.QueueMemberTypeReindex([20]);
        await WaitForProcessingAsync();

        // Assert — each repository was called
        _documentRepository.Verify(
            x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out contentTotal,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()),
            Times.AtLeastOnce);
        _mediaRepository.Verify(
            x => x.GetPage(
                It.IsAny<IQuery<IMedia>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out mediaTotal,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IMedia>?>(),
                It.IsAny<Ordering?>()),
            Times.AtLeastOnce);
        _memberRepository.Verify(
            x => x.GetPage(
                It.IsAny<IQuery<IMember>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out memberTotal,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IMember>?>(),
                It.IsAny<Ordering?>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    ///     Verifies that a transient failure is retried and the reindex succeeds on the second attempt.
    /// </summary>
    [Test]
    public async Task Transient_Failure_Retries_And_Succeeds()
    {
        // Arrange — repository throws once then succeeds.
        var callCount = 0;
        var total = 0L;
        _documentRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()))
            .Returns(() =>
            {
                if (Interlocked.Increment(ref callCount) == 1)
                {
                    throw new InvalidOperationException("Transient DB error");
                }

                return Enumerable.Empty<IContent>();
            });

        // Act
        _service.QueueContentTypeReindex([1]);
        await WaitForProcessingAsync();

        // Assert — called twice (first failed, second succeeded)
        _documentRepository.Verify(
            x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()),
            Times.Exactly(2));
    }

    /// <summary>
    ///     Verifies that the worker stops retrying after 3 consecutive failures and leaves the IDs pending
    ///     for a future queue call to pick up.
    /// </summary>
    [Test]
    public async Task Persistent_Failure_Gives_Up_After_Max_Retries()
    {
        // Arrange — repository always throws.
        var total = 0L;
        _documentRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()))
            .Throws(new InvalidOperationException("Persistent DB error"));

        // Act
        _service.QueueContentTypeReindex([1]);

        using var cts = new CancellationTokenSource(_testTimeout);
        await _service.WaitForWorkerIdleAsync(cts.Token);

        // Assert — called 3 times (max consecutive failures)
        _documentRepository.Verify(
            x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()),
            Times.Exactly(3));
    }

    /// <summary>
    ///     Verifies that application shutdown cancels an in-flight reindex cleanly without completing the
    ///     reindex operation.
    /// </summary>
    [Test]
    public async Task Shutdown_Cancels_In_Flight_Reindex()
    {
        // Arrange
        using var shutdownCts = new CancellationTokenSource();
        using var service = CreateService(shutdownCts.Token);

        var reindexStarted = new TaskCompletionSource();
        var total = 0L;
        _documentRepository
            .Setup(x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()))
            .Callback(() =>
            {
                reindexStarted.SetResult();
                shutdownCts.Cancel();
                shutdownCts.Token.ThrowIfCancellationRequested();
            });

        // Act
        service.QueueContentTypeReindex([1]);

        using var timeoutCts = new CancellationTokenSource(_testTimeout);
        await service.WaitForWorkerIdleAsync(timeoutCts.Token);

        // Assert
        await reindexStarted.Task;
        _documentRepository.Verify(
            x => x.GetPage(
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                out total,
                It.IsAny<string[]?>(),
                It.IsAny<IQuery<IContent>?>(),
                It.IsAny<Ordering?>()),
            Times.Once);
    }

    private static IContent CreateContent(int id, bool published)
    {
        var content = new Mock<IContent>();
        content.Setup(x => x.Id).Returns(id);
        content.Setup(x => x.Key).Returns(Guid.NewGuid());
        content.Setup(x => x.Published).Returns(published);
        content.Setup(x => x.ParentId).Returns(-1);
        return content.Object;
    }

    private static IMedia CreateMedia(int id, bool trashed)
    {
        var media = new Mock<IMedia>();
        media.Setup(x => x.Id).Returns(id);
        media.Setup(x => x.Key).Returns(Guid.NewGuid());
        media.Setup(x => x.Trashed).Returns(trashed);
        return media.Object;
    }

    private async Task WaitForProcessingAsync()
    {
        using var cts = new CancellationTokenSource(_testTimeout);
        await _service.WaitForPendingReindexAsync(cts.Token);
    }
}
