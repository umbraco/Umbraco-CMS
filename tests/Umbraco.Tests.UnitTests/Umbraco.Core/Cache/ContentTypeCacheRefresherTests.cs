// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class ContentTypeCacheRefresherTests
{
    private Mock<IDocumentCacheService> _documentCacheService = null!;
    private Mock<IMediaCacheService> _mediaCacheService = null!;

    private ContentTypeCacheRefresher CreateRefresher(IPublishedModelFactory publishedModelFactory)
    {
        var publishedContentTypeCache = new Mock<IPublishedContentTypeCache>();
        var publishedContentTypeFactory = new Mock<IPublishedContentTypeFactory>();
        var notificationFactory = new Mock<ICacheRefresherNotificationFactory>();

        // NotificationFactory.Create is called by base.Refresh — return a dummy notification.
        notificationFactory
            .Setup(x => x.Create<ContentTypeCacheRefresherNotification>(It.IsAny<object>(), It.IsAny<MessageType>()))
            .Returns(new ContentTypeCacheRefresherNotification(new object(), MessageType.RefreshByPayload));

        var appCaches = new AppCaches(
            Mock.Of<IAppPolicyCache>(),
            Mock.Of<IRequestCache>(),
            new IsolatedCaches(_ => Mock.Of<IAppPolicyCache>()));

        return new ContentTypeCacheRefresher(
            appCaches,
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIdKeyMap>(),
            Mock.Of<IContentTypeCommonRepository>(),
            Mock.Of<IEventAggregator>(),
            notificationFactory.Object,
            publishedModelFactory,
            publishedContentTypeFactory.Object,
            _documentCacheService.Object,
            publishedContentTypeCache.Object,
            _mediaCacheService.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _documentCacheService = new Mock<IDocumentCacheService>(MockBehavior.Strict);
        _mediaCacheService = new Mock<IMediaCacheService>(MockBehavior.Strict);
    }

    [Test]
    public void Structural_Document_Change_Triggers_Full_Memory_Cache_Rebuild()
    {
        // Arrange — non-auto factory: no full clear expected.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        var payloads = new[]
        {
            new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), 100, ContentTypeChangeTypes.RefreshMain),
        };

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)))
            .Returns(Task.CompletedTask);

        // Act
        refresher.Refresh(payloads);

        // Assert
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)),
            Times.Once);
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
    }

    [Test]
    public void Structural_Media_Change_Triggers_Full_Memory_Cache_Rebuild()
    {
        // Arrange — non-auto factory: no full clear expected.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        var payloads = new[]
        {
            new ContentTypeCacheRefresher.JsonPayload(nameof(IMediaType), 200, ContentTypeChangeTypes.RefreshMain),
        };

        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)))
            .Returns(Task.CompletedTask);

        // Act
        refresher.Refresh(payloads);

        // Assert
        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)),
            Times.Once);
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
    }

    [Test]
    public void Non_Structural_Document_Change_Selectively_Clears_Converted_Cache()
    {
        // Arrange — non-auto factory: selective clear by content type ID.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        var payloads = new[]
        {
            new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), 100, ContentTypeChangeTypes.RefreshOther),
        };

        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))));

        // Act
        refresher.Refresh(payloads);

        // Assert — selective clear called, full clear NOT called.
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))),
            Times.Once);
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()),
            Times.Never);
    }

    [Test]
    public void Non_Structural_Media_Change_Selectively_Clears_Converted_Cache()
    {
        // Arrange — non-auto factory: selective clear by media type ID.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        var payloads = new[]
        {
            new ContentTypeCacheRefresher.JsonPayload(nameof(IMediaType), 200, ContentTypeChangeTypes.RefreshOther),
        };

        _mediaCacheService
            .Setup(x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))));

        // Act
        refresher.Refresh(payloads);

        // Assert — selective clear called, full clear NOT called.
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))),
            Times.Once);
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()),
            Times.Never);
    }

    [Test]
    public void Combined_Structural_And_Non_Structural_Change_Uses_Rebuild_Not_Clear()
    {
        // When a payload has both RefreshMain and RefreshOther, it should be treated as structural only.
        // IsStructuralChange returns true, IsNonStructuralChange returns false.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        var payloads = new[]
        {
            new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), 100, ContentTypeChangeTypes.RefreshMain | ContentTypeChangeTypes.RefreshOther),
        };

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)))
            .Returns(Task.CompletedTask);

        // Act
        refresher.Refresh(payloads);

        // Assert — should rebuild, not clear.
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)),
            Times.Once);
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
    }

    [Test]
    public void Mixed_Payloads_Route_Each_Type_Correctly()
    {
        // A mix of structural document, non-structural document, structural media, and non-structural media.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        var payloads = new[]
        {
            new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), 100, ContentTypeChangeTypes.RefreshMain),
            new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), 101, ContentTypeChangeTypes.RefreshOther),
            new ContentTypeCacheRefresher.JsonPayload(nameof(IMediaType), 200, ContentTypeChangeTypes.RefreshMain),
            new ContentTypeCacheRefresher.JsonPayload(nameof(IMediaType), 201, ContentTypeChangeTypes.RefreshOther),
        };

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)))
            .Returns(Task.CompletedTask);
        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(101))));

        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)))
            .Returns(Task.CompletedTask);
        _mediaCacheService
            .Setup(x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(201))));

        // Act
        refresher.Refresh(payloads);

        // Assert — structural types get rebuild.
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)),
            Times.Once);
        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)),
            Times.Once);

        // Assert — non-structural types get selective clear only (non-auto factory).
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(101))),
            Times.Once);
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(201))),
            Times.Once);

        // Assert — no full clear in non-auto mode.
        _documentCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Never);
        _mediaCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Never);
    }

    [Test]
    public void Remove_Change_Does_Not_Trigger_Rebuild_Or_Clear()
    {
        // Remove changes should not trigger any cache service calls.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        var payloads = new[]
        {
            new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), 100, ContentTypeChangeTypes.Remove),
            new ContentTypeCacheRefresher.JsonPayload(nameof(IMediaType), 200, ContentTypeChangeTypes.Remove),
        };

        // Act
        refresher.Refresh(payloads);

        // Assert — no cache operations.
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()),
            Times.Never);
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()),
            Times.Never);
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
    }

    private static IAutoPublishedModelFactory CreateAutoFactory()
    {
        var mock = new Mock<IAutoPublishedModelFactory>();
        mock.Setup(x => x.SyncRoot).Returns(new object());
        return mock.Object;
    }

    [Test]
    public void Auto_Factory_Non_Structural_Change_Clears_All_Converted_Content()
    {
        // In auto models builder mode, the factory reset invalidates ALL compiled model types.
        // A single full clear is used instead of a selective clear.
        var refresher = CreateRefresher(CreateAutoFactory());
        var payloads = new[]
        {
            new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), 100, ContentTypeChangeTypes.RefreshOther),
        };

        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache());

        // Act
        refresher.Refresh(payloads);

        // Assert — full clear called (not selective) due to auto factory reset.
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Once);
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()),
            Times.Never);
    }

    [Test]
    public void Auto_Factory_Structural_Change_Clears_All_Converted_Content()
    {
        // In auto models builder mode, even structural changes need a full converted cache clear
        // because the factory reset invalidates all model types, not just the changed ones.
        var refresher = CreateRefresher(CreateAutoFactory());
        var payloads = new[]
        {
            new ContentTypeCacheRefresher.JsonPayload(nameof(IContentType), 100, ContentTypeChangeTypes.RefreshMain),
        };

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)))
            .Returns(Task.CompletedTask);
        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache());

        // Act
        refresher.Refresh(payloads);

        // Assert — rebuild for structural change.
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)),
            Times.Once);

        // Assert — full clear also called due to auto factory reset.
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Once);
    }
}
