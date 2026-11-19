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
    private Mock<IElementCacheService> _elementCacheService = null!;
    private Mock<IPublishedContentTypeCache> _publishedContentTypeCache = null!;
    private Mock<IDataTypeConfigurationCache> _dataTypeConfigurationCache = null!;

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
            Mock.Of<IContentTypeCommonRepository>(),
            _dataTypeConfigurationCache.Object,
            _elementCacheService.Object);
    }

    [SetUp]
    public void SetUp()
    {
        _documentCacheService = new Mock<IDocumentCacheService>(MockBehavior.Strict);
        _mediaCacheService = new Mock<IMediaCacheService>(MockBehavior.Strict);
        _elementCacheService = new Mock<IElementCacheService>(MockBehavior.Strict);
        _publishedContentTypeCache = new Mock<IPublishedContentTypeCache>();
        _dataTypeConfigurationCache = new Mock<IDataTypeConfigurationCache>();
    }

    [Test]
    public void RefreshInternal_Clears_The_Data_Type_Configuration_Cache()
    {
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        Guid[] keys = [Guid.NewGuid(), Guid.NewGuid()];
        DataTypeCacheRefresher.JsonPayload[] payloads = keys.Select((key, index) => new DataTypeCacheRefresher.JsonPayload(index + 1, key, false)).ToArray();

        refresher.RefreshInternal(payloads);

        _dataTypeConfigurationCache.Verify(x => x.ClearCache(It.Is<IEnumerable<Guid>>(cleared => cleared.SequenceEqual(keys))), Times.Once);
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
    public void Element_Type_Change_Rebuilds_Memory_Cache()
    {
        // Arrange — a data type change that affects an element type.
        var refresher = CreateRefresher(Mock.Of<IPublishedModelFactory>());
        SetupClearByDataTypeReturning(42, CreatePublishedContentType(300, PublishedItemType.Element));

        _elementCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 300)))
            .Returns(Task.CompletedTask);

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert
        _elementCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 300)),
            Times.Once);
    }

    [TestCase(PublishedItemType.Content)]
    [TestCase(PublishedItemType.Media)]
    [TestCase(PublishedItemType.Element)]
    [TestCase(null)]
    public void Auto_Factory_Clears_Every_Converted_Content_Cache_Whatever_Types_Use_The_Data_Type(PublishedItemType? affectedItemType)
    {
        // The auto factory reset discards every generated model type, so converted items of any kind can hold
        // discarded models, whichever types (if any) use the changed data type.
        var refresher = CreateRefresher(CreateAutoFactory());
        if (affectedItemType is null)
        {
            SetupClearByDataTypeReturning(42);
        }
        else
        {
            SetupClearByDataTypeReturning(42, CreatePublishedContentType(100, affectedItemType.Value));
        }

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);
        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);
        _elementCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.IsAny<IEnumerable<int>>()))
            .Returns(Task.CompletedTask);
        SetupAllConvertedContentCacheClears();

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert
        VerifyAllConvertedContentCachesClearedOnce();
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
    public void Mixed_Document_Media_And_Element_Types_Are_All_Rebuilt()
    {
        // A data type change that affects document, media and element types at once.
        var refresher = CreateRefresher(CreateAutoFactory());
        SetupClearByDataTypeReturning(
            42,
            CreatePublishedContentType(100, PublishedItemType.Content),
            CreatePublishedContentType(200, PublishedItemType.Media),
            CreatePublishedContentType(300, PublishedItemType.Element));

        _documentCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)))
            .Returns(Task.CompletedTask);
        _mediaCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)))
            .Returns(Task.CompletedTask);
        _elementCacheService
            .Setup(x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 300)))
            .Returns(Task.CompletedTask);
        SetupAllConvertedContentCacheClears();

        var payloads = new[] { new DataTypeCacheRefresher.JsonPayload(42, Guid.NewGuid(), false) };

        // Act
        refresher.Refresh(payloads);

        // Assert — each kind is rebuilt for its own types, and every converted cache is cleared.
        _documentCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 100)),
            Times.Once);
        _mediaCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 200)),
            Times.Once);
        _elementCacheService.Verify(
            x => x.RebuildMemoryCacheByContentTypeAsync(It.Is<int[]>(ids => ids.Length == 1 && ids[0] == 300)),
            Times.Once);
        VerifyAllConvertedContentCachesClearedOnce();
    }

    private void SetupAllConvertedContentCacheClears()
    {
        _documentCacheService.Setup(x => x.ClearConvertedContentCache());
        _mediaCacheService.Setup(x => x.ClearConvertedContentCache());
        _elementCacheService.Setup(x => x.ClearConvertedContentCache());
    }

    private void VerifyAllConvertedContentCachesClearedOnce()
    {
        _documentCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Once);
        _mediaCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Once);
        _elementCacheService.Verify(x => x.ClearConvertedContentCache(), Times.Once);
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
