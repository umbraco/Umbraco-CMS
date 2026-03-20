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

        _handler = CreateHandler(ContentTypeRebuildMode.Immediate);
    }

    private CacheRefreshingNotificationHandler CreateHandler(ContentTypeRebuildMode mode) =>
        new(
            _documentCacheService.Object,
            _mediaCacheService.Object,
            _publishedContentTypeCache.Object,
            Options.Create(new CacheSettings { ContentTypeRebuildMode = mode }));

    /// <summary>
    ///     Verifies that a structural content type change in immediate mode triggers a synchronous database cache rebuild.
    /// </summary>
    [Test]
    public async Task Structural_Content_Type_Change_Triggers_Rebuild()
    {
        // Arrange
        var contentType = CreateContentType(100);
#pragma warning disable CS0618 // Type or member is obsolete
        var notification = new ContentTypeRefreshedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshMain),
            new EventMessages());
#pragma warning restore CS0618 // Type or member is obsolete

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

    /// <summary>
    ///     Verifies that a non-structural content type change clears the converted cache selectively by type ID, not a full clear.
    /// </summary>
    [Test]
    public async Task Non_Structural_Content_Type_Change_Selectively_Clears_Converted_Cache()
    {
        // Arrange
        var contentType = CreateContentType(100);
#pragma warning disable CS0618 // Type or member is obsolete
        var notification = new ContentTypeRefreshedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshOther),
            new EventMessages());
#pragma warning restore CS0618 // Type or member is obsolete

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

    /// <summary>
    ///     Verifies that non-structural changes never trigger a full converted cache clear from this handler.
    /// </summary>
    [Test]
    public async Task Non_Structural_Content_Type_Change_Never_Does_Full_Clear()
    {
        // Verifies that the notification handler always uses selective clearing,
        // never a full clear, since no model factory reset occurs in this handler.
        var contentType = CreateContentType(100);
#pragma warning disable CS0618 // Type or member is obsolete
        var notification = new ContentTypeRefreshedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshOther),
            new EventMessages());
#pragma warning restore CS0618 // Type or member is obsolete

        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache(It.IsAny<IReadOnlyCollection<int>>()));

        // Act
        await _handler.HandleAsync(notification, CancellationToken.None);

        // Assert — full clear must never be called from this handler.
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(),
            Times.Never);
    }

    /// <summary>
    ///     Verifies that a structural media type change in immediate mode triggers a synchronous database cache rebuild.
    /// </summary>
    [Test]
    public async Task Structural_Media_Type_Change_Triggers_Rebuild()
    {
        // Arrange
        var mediaType = CreateMediaType(200);
#pragma warning disable CS0618 // Type or member is obsolete
        var notification = new MediaTypeRefreshedNotification(
            new ContentTypeChange<IMediaType>(mediaType, ContentTypeChangeTypes.RefreshMain),
            new EventMessages());
#pragma warning restore CS0618 // Type or member is obsolete

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

    /// <summary>
    ///     Verifies that a non-structural media type change clears the converted cache selectively by type ID, not a full clear.
    /// </summary>
    [Test]
    public async Task Non_Structural_Media_Type_Change_Selectively_Clears_Converted_Cache()
    {
        // Arrange
        var mediaType = CreateMediaType(200);
#pragma warning disable CS0618 // Type or member is obsolete
        var notification = new MediaTypeRefreshedNotification(
            new ContentTypeChange<IMediaType>(mediaType, ContentTypeChangeTypes.RefreshOther),
            new EventMessages());
#pragma warning restore CS0618 // Type or member is obsolete

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

    /// <summary>
    ///     Verifies that in deferred mode, a structural content type change does not trigger a rebuild from this handler.
    /// </summary>
    [Test]
    public async Task Deferred_Mode_Structural_Content_Type_Change_Skips_Rebuild()
    {
        // Arrange — in deferred mode, the rebuild is queued by DeferredCacheRebuildNotificationHandler
        // (on ContentTypeChangedNotification), not here. This handler should do neither.
        var handler = CreateHandler(ContentTypeRebuildMode.Deferred);
        var contentType = CreateContentType(100);
#pragma warning disable CS0618 // Type or member is obsolete
        var notification = new ContentTypeRefreshedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshMain),
            new EventMessages());
#pragma warning restore CS0618 // Type or member is obsolete

        // Act
        await handler.HandleAsync(notification, CancellationToken.None);

        // Assert — no rebuild called from this handler in deferred mode.
        _documentCacheService.Verify(
            x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
    }

    /// <summary>
    ///     Verifies that in deferred mode, a structural media type change does not trigger a rebuild from this handler.
    /// </summary>
    [Test]
    public async Task Deferred_Mode_Structural_Media_Type_Change_Skips_Rebuild()
    {
        // Arrange — in deferred mode, the rebuild is queued by DeferredCacheRebuildNotificationHandler
        // (on MediaTypeChangedNotification), not here.
        var handler = CreateHandler(ContentTypeRebuildMode.Deferred);
        var mediaType = CreateMediaType(200);
#pragma warning disable CS0618 // Type or member is obsolete
        var notification = new MediaTypeRefreshedNotification(
            new ContentTypeChange<IMediaType>(mediaType, ContentTypeChangeTypes.RefreshMain),
            new EventMessages());
#pragma warning restore CS0618 // Type or member is obsolete

        // Act
        await handler.HandleAsync(notification, CancellationToken.None);

        // Assert — no rebuild called from this handler in deferred mode.
        _mediaCacheService.Verify(
            x => x.Rebuild(It.IsAny<IReadOnlyCollection<int>>()),
            Times.Never);
    }

    /// <summary>
    ///     Verifies that non-structural changes still clear the converted content cache in deferred mode.
    /// </summary>
    [Test]
    public async Task Deferred_Mode_Non_Structural_Change_Still_Clears_Converted_Cache()
    {
        // Arrange
        var handler = CreateHandler(ContentTypeRebuildMode.Deferred);
        var contentType = CreateContentType(100);
#pragma warning disable CS0618 // Type or member is obsolete
        var notification = new ContentTypeRefreshedNotification(
            new ContentTypeChange<IContentType>(contentType, ContentTypeChangeTypes.RefreshOther),
            new EventMessages());
#pragma warning restore CS0618 // Type or member is obsolete

        _documentCacheService
            .Setup(x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))));

        // Act
        await handler.HandleAsync(notification, CancellationToken.None);

        // Assert — non-structural changes always clear converted cache, regardless of mode.
        _documentCacheService.Verify(
            x => x.ClearConvertedContentCache(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))),
            Times.Once);
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
