// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
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

    /// <summary>
    ///     Verifies the handler still queues a rebuild when the notification is dispatched through a
    ///     distributed-cache-only publisher (e.g. the publisher Umbraco Deploy installs on restore/import
    ///     scopes). This is the dispatch-filtering path that the direct <see cref="DeferredCacheRebuildNotificationHandler.Handle(ContentTypeChangedNotification)" />
    ///     tests above cannot exercise, since <see cref="EventAggregator" /> filters handlers by
    ///     <see cref="IDistributedCacheNotificationHandler" /> before invoking them.
    /// </summary>
    [Test]
    public void Content_Type_Change_Via_Distributed_Cache_Publisher_Queues_Deferred_Rebuild()
    {
        // Arrange
        _deferredCacheRebuildService
            .Setup(x => x.QueueContentTypeRebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))));

        IScopedNotificationPublisher publisher = CreateDistributedCacheOnlyPublisher();
        var notification = new ContentTypeChangedNotification(
            new ContentTypeChange<IContentType>(CreateContentType(100), ContentTypeChangeTypes.RefreshMain),
            new EventMessages());

        // Act — publish through the distributed-cache-only publisher, then complete the scope to flush it.
        publisher.Publish(notification);
        publisher.ScopeExit(true);

        // Assert
        _deferredCacheRebuildService.Verify(
            x => x.QueueContentTypeRebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(100))),
            Times.Once);
    }

    /// <summary>
    ///     As <see cref="Content_Type_Change_Via_Distributed_Cache_Publisher_Queues_Deferred_Rebuild" />, but for media type changes.
    /// </summary>
    [Test]
    public void Media_Type_Change_Via_Distributed_Cache_Publisher_Queues_Deferred_Rebuild()
    {
        // Arrange
        _deferredCacheRebuildService
            .Setup(x => x.QueueMediaTypeRebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))));

        IScopedNotificationPublisher publisher = CreateDistributedCacheOnlyPublisher();
        var notification = new MediaTypeChangedNotification(
            new ContentTypeChange<IMediaType>(CreateMediaType(200), ContentTypeChangeTypes.RefreshMain),
            new EventMessages());

        // Act
        publisher.Publish(notification);
        publisher.ScopeExit(true);

        // Assert
        _deferredCacheRebuildService.Verify(
            x => x.QueueMediaTypeRebuild(It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 1 && ids.Contains(200))),
            Times.Once);
    }

    private ScopedNotificationPublisher<IDistributedCacheNotificationHandler> CreateDistributedCacheOnlyPublisher()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_deferredCacheRebuildService.Object);
        services.AddSingleton(Options.Create(new CacheSettings { ContentTypeRebuildMode = ContentTypeRebuildMode.Deferred }));
        services.AddTransient<INotificationHandler<ContentTypeChangedNotification>, DeferredCacheRebuildNotificationHandler>();
        services.AddTransient<INotificationHandler<MediaTypeChangedNotification>, DeferredCacheRebuildNotificationHandler>();

        ServiceProvider provider = services.BuildServiceProvider();
        var eventAggregator = new EventAggregator(type => provider.GetService(type)!);
        return new ScopedNotificationPublisher<IDistributedCacheNotificationHandler>(eventAggregator);
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
