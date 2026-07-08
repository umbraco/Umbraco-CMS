using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Search;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Search;

[TestFixture]
public class ElementIndexingNotificationHandlerTests
{
    private Mock<IDeferredSearchReindexService> _mockReindexService = null!;
    private Mock<IUmbracoIndexingHandler> _mockIndexingHandler = null!;
    private Mock<IOptionsMonitor<IndexingSettings>> _mockIndexingSettings = null!;
    private ElementIndexingNotificationHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockReindexService = new Mock<IDeferredSearchReindexService>();
        _mockIndexingHandler = new Mock<IUmbracoIndexingHandler>();
        _mockIndexingHandler.Setup(x => x.Enabled).Returns(true);
        _mockIndexingSettings = new Mock<IOptionsMonitor<IndexingSettings>>();
        _mockIndexingSettings.Setup(x => x.CurrentValue).Returns(new IndexingSettings { IndexExternalBlockElements = true });
        _sut = new ElementIndexingNotificationHandler(
            _mockReindexService.Object,
            _mockIndexingHandler.Object,
            _mockIndexingSettings.Object);
    }

    [Test]
    public void Handle_QueuesPublishedAndUnpublishedElementIds()
    {
        var notification = new ElementCacheRefresherNotification(
            new[]
            {
                new ElementCacheRefresher.JsonPayload(30, Guid.NewGuid(), TreeChangeTypes.RefreshNode)
                {
                    PublishedCultures = ["*"],
                },
                new ElementCacheRefresher.JsonPayload(40, Guid.NewGuid(), TreeChangeTypes.RefreshNode)
                {
                    UnpublishedCultures = ["*"],
                },
            },
            MessageType.RefreshByPayload);

        _sut.Handle(notification);

        _mockReindexService.Verify(
            s => s.QueueReindexOnElementChange(It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { 30, 40 }))),
            Times.Once);
    }

    [Test]
    public void Handle_QueuesRemovedElementIds()
    {
        var notification = new ElementCacheRefresherNotification(
            new[] { new ElementCacheRefresher.JsonPayload(50, Guid.NewGuid(), TreeChangeTypes.Remove) },
            MessageType.RefreshByPayload);

        _sut.Handle(notification);

        _mockReindexService.Verify(
            s => s.QueueReindexOnElementChange(It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { 50 }))),
            Times.Once);
    }

    [Test]
    public void Handle_DraftSave_DoesNotQueue()
    {
        // A draft save comes through as RefreshNode with no published/unpublished cultures. Draft values never enter
        // the external index, so it must not trigger a reindex of referencing documents.
        var notification = new ElementCacheRefresherNotification(
            new[] { new ElementCacheRefresher.JsonPayload(70, Guid.NewGuid(), TreeChangeTypes.RefreshNode) },
            MessageType.RefreshByPayload);

        _sut.Handle(notification);

        _mockReindexService.Verify(s => s.QueueReindexOnElementChange(It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }

    [Test]
    public void Handle_WhenIndexingDisabled_DoesNotQueue()
    {
        _mockIndexingHandler.Setup(x => x.Enabled).Returns(false);
        var notification = new ElementCacheRefresherNotification(
            new[]
            {
                new ElementCacheRefresher.JsonPayload(60, Guid.NewGuid(), TreeChangeTypes.RefreshNode)
                {
                    PublishedCultures = ["*"],
                },
            },
            MessageType.RefreshByPayload);

        _sut.Handle(notification);

        _mockReindexService.Verify(s => s.QueueReindexOnElementChange(It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }

    [Test]
    public void Handle_WhenExternalBlockElementIndexingDisabled_DoesNotQueue()
    {
        _mockIndexingSettings.Setup(x => x.CurrentValue).Returns(new IndexingSettings { IndexExternalBlockElements = false });
        var notification = new ElementCacheRefresherNotification(
            new[]
            {
                new ElementCacheRefresher.JsonPayload(90, Guid.NewGuid(), TreeChangeTypes.RefreshNode)
                {
                    PublishedCultures = ["*"],
                },
            },
            MessageType.RefreshByPayload);

        _sut.Handle(notification);

        _mockReindexService.Verify(s => s.QueueReindexOnElementChange(It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }

    [Test]
    public void Handle_RefreshAll_DoesNotQueue()
    {
        var notification = new ElementCacheRefresherNotification(
            new[] { new ElementCacheRefresher.JsonPayload(0, Guid.Empty, TreeChangeTypes.RefreshAll) },
            MessageType.RefreshByPayload);

        _sut.Handle(notification);

        _mockReindexService.Verify(s => s.QueueReindexOnElementChange(It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }
}
