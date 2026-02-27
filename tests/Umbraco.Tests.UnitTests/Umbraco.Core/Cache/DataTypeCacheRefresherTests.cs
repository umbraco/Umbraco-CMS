// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class DataTypeCacheRefresherTests
{
    private Mock<IDocumentCacheService> _documentCacheService = null!;
    private Mock<IMediaCacheService> _mediaCacheService = null!;
    private Mock<IPublishedContentTypeCache> _publishedContentTypeCache = null!;

    private DataTypeCacheRefresher CreateRefresher(IPublishedModelFactory publishedModelFactory)
    {
        var publishedContentTypeFactory = new Mock<IPublishedContentTypeFactory>();
        var notificationFactory = new Mock<ICacheRefresherNotificationFactory>();

        notificationFactory
            .Setup(x => x.Create<DataTypeCacheRefresherNotification>(It.IsAny<object>(), It.IsAny<MessageType>()))
            .Returns(new DataTypeCacheRefresherNotification(new object(), MessageType.RefreshByPayload));

        var appCaches = new AppCaches(
            Mock.Of<IAppPolicyCache>(),
            Mock.Of<IRequestCache>(),
            new IsolatedCaches(_ => Mock.Of<IAppPolicyCache>()));

        return new DataTypeCacheRefresher(
            appCaches,
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IIdKeyMap>(),
            Mock.Of<IEventAggregator>(),
            notificationFactory.Object,
            publishedModelFactory,
            publishedContentTypeFactory.Object,
            _publishedContentTypeCache.Object,
            _documentCacheService.Object,
            _mediaCacheService.Object,
            Mock.Of<IContentTypeCommonRepository>());
    }

    [SetUp]
    public void SetUp()
    {
        _documentCacheService = new Mock<IDocumentCacheService>(MockBehavior.Strict);
        _mediaCacheService = new Mock<IMediaCacheService>(MockBehavior.Strict);
        _publishedContentTypeCache = new Mock<IPublishedContentTypeCache>();
    }

    [Test]
    public void Document_Type_Change_Rebuilds_Memory_Cache()
    {
        // Arrange — a data type change that affects a document content type.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        SetupClearByDataTypeReturning(42, CreatePublishedContentType(100, PublishedItemType.Content));

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)))
            .Returns(Task.CompletedTask);

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)),
            Times.Once);
    }

    [Test]
    public void Media_Type_Change_Rebuilds_Memory_Cache()
    {
        // Arrange — a data type change that affects a media content type.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        SetupClearByDataTypeReturning(42, CreatePublishedContentType(200, PublishedItemType.Media));

        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)))
            .Returns(Task.CompletedTask);

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert
        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)),
            Times.Once);
    }

    [Test]
    public void Non_Auto_Factory_Does_Not_Full_Clear_Converted_Cache()
    {
        // Arrange — non-auto factory: no full clear expected after rebuild.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        SetupClearByDataTypeReturning(42, CreatePublishedContentType(100, PublishedItemType.Content));

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)))
            .Returns(Task.CompletedTask);

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert — rebuild is called (which internally does selective clear),
        // but no additional full clear since model types are stable in non-auto mode.
        _documentCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Never);
    }

    [Test]
    public void Auto_Factory_Document_Change_Clears_All_Converted_Content()
    {
        // In auto models builder mode, the factory reset invalidates ALL compiled model types.
        // A full clear is needed to prevent stale instances of other types (e.g. Model.Parent<T>()).
        var refresher = CreateRefresher(CreateAutoFactory());
        SetupClearByDataTypeReturning(42, CreatePublishedContentType(100, PublishedItemType.Content));

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)))
            .Returns(Task.CompletedTask);
        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache());

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert — full clear called due to auto factory reset.
        _documentCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Once);
    }

    [Test]
    public void Auto_Factory_Media_Change_Clears_All_Converted_Content()
    {
        var refresher = CreateRefresher(CreateAutoFactory());
        SetupClearByDataTypeReturning(42, CreatePublishedContentType(200, PublishedItemType.Media));

        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)))
            .Returns(Task.CompletedTask);
        _mediaCacheService
            .Setup(x => x.ClearConvertedContentCache());

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert — full clear called due to auto factory reset.
        _mediaCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Once);
    }

    [Test]
    public void Auto_Factory_No_Affected_Types_Does_Not_Clear()
    {
        // If ClearByDataTypeId returns no affected content types, no clearing should occur.
        var refresher = CreateRefresher(CreateAutoFactory());

        // ClearByDataTypeId returns empty — no content types use this data type.
        _publishedContentTypeCache
            .Setup(x => x.ClearByDataTypeId(42))
            .Returns(Enumerable.Empty<IPublishedContentType>());

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert — no cache operations since no content types were affected.
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()),
            Times.Never);
        _documentCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Never);
        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()),
            Times.Never);
        _mediaCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Never);
    }

    [Test]
    public void Mixed_Document_And_Media_Types_Both_Handled()
    {
        // A data type change that affects both document and media content types.
        var refresher = CreateRefresher(CreateAutoFactory());

        var contentType = CreatePublishedContentType(100, PublishedItemType.Content);
        var mediaType = CreatePublishedContentType(200, PublishedItemType.Media);
        _publishedContentTypeCache
            .Setup(x => x.ClearByDataTypeId(42))
            .Returns(new[] { contentType, mediaType });

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)))
            .Returns(Task.CompletedTask);
        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache());

        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)))
            .Returns(Task.CompletedTask);
        _mediaCacheService
            .Setup(x => x.ClearConvertedContentCache());

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert — both document and media types rebuilt and fully cleared.
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)),
            Times.Once);
        _documentCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Once);

        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)),
            Times.Once);
        _mediaCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Once);
    }

    private void SetupClearByDataTypeReturning(int dataTypeId, params IPublishedContentType[] contentTypes)
    {
        _publishedContentTypeCache
            .Setup(x => x.ClearByDataTypeId(dataTypeId))
            .Returns(contentTypes);
    }

    private static IPublishedContentType CreatePublishedContentType(int id, PublishedItemType itemType)
    {
        var mock = new Mock<IPublishedContentType>();
        mock.Setup(x => x.Id).Returns(id);
        mock.Setup(x => x.ItemType).Returns(itemType);
        return mock.Object;
    }

    private static IAutoPublishedModelFactory CreateAutoFactory()
    {
        var mock = new Mock<IAutoPublishedModelFactory>();
        mock.Setup(x => x.SyncRoot).Returns(new object());
        return mock.Object;
    }
}
