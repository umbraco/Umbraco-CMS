// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class DeferredCacheRebuildNotificationHandlerTests
{
    private Mock<IDeferredCacheRebuildService> _deferredCacheRebuildService = null!;

    [SetUp]
    public void SetUp()
        => _deferredCacheRebuildService = new Mock<IDeferredCacheRebuildService>(MockBehavior.Strict);

    private DeferredCacheRebuildNotificationHandler CreateHandler(ContentTypeRebuildMode mode) =>
        new(
            _deferredCacheRebuildService.Object,
            Options.Create(new CacheSettings { ContentTypeRebuildMode = mode }));

    /// <summary>
    ///     Verifies that a structural content type change in deferred mode queues the rebuild via the deferred service.
    /// </summary>
    [Test]
    public void Structural_Content_Type_Change_Queues_Deferred_Rebuild()
    {
        // Arrange
        var handler = CreateHandler(ContentTypeRebuildMode.Deferred);
        var contentType = CreateContentType(100);
        var notification = new ContentTypeChangedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshMain),
            new EventMessages());

        _deferredCacheRebuildService
            .Setup(x => x.QueueContentTypeRebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))));

        // Act
        handler.Handle(notification);

        // Assert
        _deferredCacheRebuildService.Verify(
            x => x.QueueContentTypeRebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))),
            Times.Once);
    }

    /// <summary>
    ///     Verifies that a structural media type change in deferred mode queues the rebuild via the deferred service.
    /// </summary>
    [Test]
    public void Structural_Media_Type_Change_Queues_Deferred_Rebuild()
    {
        // Arrange
        var handler = CreateHandler(ContentTypeRebuildMode.Deferred);
        var mediaType = CreateMediaType(200);
        var notification = new MediaTypeChangedNotification(
            new ContentTypeChange<IMediaType>(mediaType, ContentTypeChangeTypes.RefreshMain),
            new EventMessages());

        _deferredCacheRebuildService
            .Setup(x => x.QueueMediaTypeRebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))));

        // Act
        handler.Handle(notification);

        // Assert
        _deferredCacheRebuildService.Verify(
            x => x.QueueMediaTypeRebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))),
            Times.Once);
    }

    /// <summary>
    ///     Verifies that immediate mode causes this handler to be a no-op, since the rebuild is handled synchronously elsewhere.
    /// </summary>
    [Test]
    public void Immediate_Mode_Does_Not_Queue()
    {
        // Arrange — in immediate mode, this handler is a no-op.
        var handler = CreateHandler(ContentTypeRebuildMode.Immediate);
        var contentType = CreateContentType(100);
        var notification = new ContentTypeChangedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshMain),
            new EventMessages());

        // Act
        handler.Handle(notification);

        // Assert
        _deferredCacheRebuildService.Verify(
            x => x.QueueContentTypeRebuild(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
    }

    /// <summary>
    ///     Verifies that non-structural changes (e.g. name or icon) do not queue a deferred rebuild.
    /// </summary>
    [Test]
    public void Non_Structural_Change_Does_Not_Queue()
    {
        // Arrange
        var handler = CreateHandler(ContentTypeRebuildMode.Deferred);
        var contentType = CreateContentType(100);
        var notification = new ContentTypeChangedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshOther),
            new EventMessages());

        // Act
        handler.Handle(notification);

        // Assert
        _deferredCacheRebuildService.Verify(
            x => x.QueueContentTypeRebuild(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
    }

    private static IContentType CreateContentType(int id)
    {
        var mock = new Mock<IContentType>();
        mock.Setup(x => x.Id).Returns(id);
        return mock.Object;
    }

    private static IMediaType CreateMediaType(int id)
    {
        var mock = new Mock<IMediaType>();
        mock.Setup(x => x.Id).Returns(id);
        return mock.Object;
    }
}
