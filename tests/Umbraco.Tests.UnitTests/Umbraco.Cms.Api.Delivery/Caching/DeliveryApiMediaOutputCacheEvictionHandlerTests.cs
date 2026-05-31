using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Caching;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Delivery.Caching;

[TestFixture]
public class DeliveryApiMediaOutputCacheEvictionHandlerTests
{
    private Mock<IOutputCacheStore> _storeMock = null!;
    private Mock<IRelationService> _relationServiceMock = null!;
    private Mock<IIdKeyMap> _idKeyMapMock = null!;
    private DeliveryApiMediaOutputCacheEvictionHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _storeMock = new Mock<IOutputCacheStore>();
        _storeMock
            .Setup(s => s.EvictByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _relationServiceMock = new Mock<IRelationService>();
        _relationServiceMock
            .Setup(r => r.GetByChildId(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Enumerable.Empty<IRelation>());

        _idKeyMapMock = new Mock<IIdKeyMap>();

        _handler = new DeliveryApiMediaOutputCacheEvictionHandler(
            _storeMock.Object,
            _relationServiceMock.Object,
            _idKeyMapMock.Object,
            NullLogger<DeliveryApiMediaOutputCacheEvictionHandler>.Instance);
    }

    [Test]
    public async Task HandleAsync_RefreshNode_EvictsMediaKey()
    {
        var key = Guid.NewGuid();
        var notification = CreateNotification(new MediaCacheRefresher.JsonPayload(1, key, TreeChangeTypes.RefreshNode));

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-media-{key}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_RefreshAll_EvictsAllTag()
    {
        var notification = CreateNotification(new MediaCacheRefresher.JsonPayload(1, Guid.NewGuid(), TreeChangeTypes.RefreshAll));

        await _handler.HandleAsync(notification, CancellationToken.None);

        // RefreshAll evicts everything (not just media) because content responses may reference media.
        _storeMock.Verify(
            s => s.EvictByTagAsync("umb-dapi-all", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_MultiplePayloads_EvictsAll()
    {
        var key1 = Guid.NewGuid();
        var key2 = Guid.NewGuid();
        var notification = CreateNotification(
            new MediaCacheRefresher.JsonPayload(1, key1, TreeChangeTypes.RefreshNode),
            new MediaCacheRefresher.JsonPayload(2, key2, TreeChangeTypes.RefreshNode));

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-media-{key1}", It.IsAny<CancellationToken>()),
            Times.Once);
        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-media-{key2}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_EvictsDocumentsReferencingChangedMedia()
    {
        const int mediaId = 10;
        const int parentDocumentId = 20;
        var parentDocumentKey = Guid.NewGuid();

        var relation = new Mock<IRelation>();
        relation.Setup(r => r.ParentId).Returns(parentDocumentId);

        _relationServiceMock
            .Setup(r => r.GetByChildId(mediaId, Constants.Conventions.RelationTypes.RelatedMediaAlias))
            .Returns(new[] { relation.Object });

        _idKeyMapMock
            .Setup(m => m.GetKeyForId(parentDocumentId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(parentDocumentKey));

        var notification = CreateNotification(new MediaCacheRefresher.JsonPayload(mediaId, Guid.NewGuid(), TreeChangeTypes.RefreshNode));

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{parentDocumentKey}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static MediaCacheRefresherNotification CreateNotification(params MediaCacheRefresher.JsonPayload[] payloads)
        => new(payloads, MessageType.RefreshByPayload);
}
