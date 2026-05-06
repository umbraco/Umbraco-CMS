// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class CacheRefreshingNotificationHandlerTests
{
    private Mock<IDocumentCacheService> _documentCacheService = null!;
    private Mock<IMediaCacheService> _mediaCacheService = null!;
    private Mock<IPublishedContentTypeCache> _publishedContentTypeCache = null!;
    private CacheRefreshingNotificationHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _documentCacheService = new Mock<IDocumentCacheService>(MockBehavior.Strict);
        _mediaCacheService = new Mock<IMediaCacheService>(MockBehavior.Strict);
        _publishedContentTypeCache = new Mock<IPublishedContentTypeCache>();

        _handler = new CacheRefreshingNotificationHandler(
            _documentCacheService.Object,
            _mediaCacheService.Object,
            _publishedContentTypeCache.Object);
    }

    [Test]
    public async Task Structural_Content_Type_Change_Triggers_Rebuild()
    {
        // Arrange
        var contentType = CreateContentType(100);
        var notification = new ContentTypeRefreshedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshMain),
            new EventMessages());

        _documentCacheService
            .Setup(x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))));

        // Act
        await _handler.HandleAsync(notification, CancellationToken.None);

        // Assert — structural changes trigger Rebuild.
        _documentCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))),
            Times.Once);

        // Assert — no converted cache clear for structural-only changes.
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
    }

    [Test]
    public async Task Non_Structural_Content_Type_Change_Selectively_Clears_Converted_Cache()
    {
        // Arrange
        var contentType = CreateContentType(100);
        var notification = new ContentTypeRefreshedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshOther),
            new EventMessages());

        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))));

        // Act
        await _handler.HandleAsync(notification, CancellationToken.None);

        // Assert — selective clear by content type ID (not full clear).
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))),
            Times.Once);
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);

        // Assert — no rebuild for non-structural changes.
        _documentCacheService.Verify(
            x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
    }

    [Test]
    public async Task Non_Structural_Content_Type_Change_Never_Does_Full_Clear()
    {
        // Verifies that the notification handler always uses selective clearing,
        // never a full clear, since no model factory reset occurs in this handler.
        var contentType = CreateContentType(100);
        var notification = new ContentTypeRefreshedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshOther),
            new EventMessages());

        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache(It.IsAny<IReadOnlyCollection<int>>()));

        // Act
        await _handler.HandleAsync(notification, CancellationToken.None);

        // Assert — full clear must never be called from this handler.
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
    }

    [Test]
    public async Task Structural_Media_Type_Change_Triggers_Rebuild()
    {
        // Arrange
        var mediaType = CreateMediaType(200);
        var notification = new MediaTypeRefreshedNotification(
            new ContentTypeChange<IMediaType>(mediaType, ContentTypeChangeTypes.RefreshMain),
            new EventMessages());

        _mediaCacheService
            .Setup(x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))));

        // Act
        await _handler.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mediaCacheService.Verify(
            x => x.Rebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))),
            Times.Once);
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
    }

    [Test]
    public async Task Non_Structural_Media_Type_Change_Selectively_Clears_Converted_Cache()
    {
        // Arrange
        var mediaType = CreateMediaType(200);
        var notification = new MediaTypeRefreshedNotification(
            new ContentTypeChange<IMediaType>(mediaType, ContentTypeChangeTypes.RefreshOther),
            new EventMessages());

        _mediaCacheService
            .Setup(x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))));

        // Act
        await _handler.HandleAsync(notification, CancellationToken.None);

        // Assert — selective clear by media type ID (not full clear).
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))),
            Times.Once);
        _mediaCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
        _mediaCacheService.Verify(
            x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()),
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
