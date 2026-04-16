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
public class DeliveryApiDocumentOutputCacheEvictionHandlerTests
{
    private Mock<IOutputCacheStore> _storeMock = null!;
    private Mock<IRelationService> _relationServiceMock = null!;
    private Mock<IIdKeyMap> _idKeyMapMock = null!;
    private List<Mock<IDeliveryApiOutputCacheEvictionProvider>> _evictionProviderMocks = null!;
    private DeliveryApiDocumentOutputCacheEvictionHandler _handler = null!;

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

        _evictionProviderMocks = new List<Mock<IDeliveryApiOutputCacheEvictionProvider>>();

        RebuildHandler();
    }

    [Test]
    public async Task HandleAsync_RefreshNode_EvictsContentKey()
    {
        var key = Guid.NewGuid();
        var notification = CreateNotification(new ContentCacheRefresher.JsonPayload
        {
            Key = key,
            ChangeTypes = TreeChangeTypes.RefreshNode,
        });

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{key}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_Remove_EvictsContentKey()
    {
        var key = Guid.NewGuid();
        var notification = CreateNotification(new ContentCacheRefresher.JsonPayload
        {
            Key = key,
            ChangeTypes = TreeChangeTypes.Remove,
        });

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{key}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_RefreshBranch_EvictsContentKeyAndAncestorTag()
    {
        var key = Guid.NewGuid();
        var notification = CreateNotification(new ContentCacheRefresher.JsonPayload
        {
            Key = key,
            ChangeTypes = TreeChangeTypes.RefreshBranch,
        });

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{key}", It.IsAny<CancellationToken>()),
            Times.Once);
        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-ancestor-{key}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_RefreshAll_EvictsAllTag()
    {
        var notification = CreateNotification(new ContentCacheRefresher.JsonPayload
        {
            Key = Guid.NewGuid(),
            ChangeTypes = TreeChangeTypes.RefreshAll,
        });

        await _handler.HandleAsync(notification, CancellationToken.None);

        // RefreshAll evicts everything (not just content) because media responses may reference content.
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
            new ContentCacheRefresher.JsonPayload { Key = key1, ChangeTypes = TreeChangeTypes.RefreshNode },
            new ContentCacheRefresher.JsonPayload { Key = key2, ChangeTypes = TreeChangeTypes.RefreshNode });

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{key1}", It.IsAny<CancellationToken>()),
            Times.Once);
        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{key2}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_InvokesEvictionProviders()
    {
        var providerMock = new Mock<IDeliveryApiOutputCacheEvictionProvider>();
        providerMock
            .Setup(p => p.GetAdditionalEvictionTagsAsync(It.IsAny<OutputCacheContentChangedContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "blog-category-123" });

        _evictionProviderMocks.Add(providerMock);
        RebuildHandler();

        var notification = CreateNotification(new ContentCacheRefresher.JsonPayload
        {
            Key = Guid.NewGuid(),
            ChangeTypes = TreeChangeTypes.RefreshNode,
        });

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync("blog-category-123", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_MultipleProviders_EvictsAllReturnedTags()
    {
        var provider1Mock = new Mock<IDeliveryApiOutputCacheEvictionProvider>();
        provider1Mock
            .Setup(p => p.GetAdditionalEvictionTagsAsync(It.IsAny<OutputCacheContentChangedContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "tag-a" });

        var provider2Mock = new Mock<IDeliveryApiOutputCacheEvictionProvider>();
        provider2Mock
            .Setup(p => p.GetAdditionalEvictionTagsAsync(It.IsAny<OutputCacheContentChangedContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "tag-b" });

        _evictionProviderMocks.Add(provider1Mock);
        _evictionProviderMocks.Add(provider2Mock);
        RebuildHandler();

        var notification = CreateNotification(new ContentCacheRefresher.JsonPayload
        {
            Key = Guid.NewGuid(),
            ChangeTypes = TreeChangeTypes.RefreshNode,
        });

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(s => s.EvictByTagAsync("tag-a", It.IsAny<CancellationToken>()), Times.Once);
        _storeMock.Verify(s => s.EvictByTagAsync("tag-b", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task HandleAsync_WhenPayloadKeyNull_SkipsEviction()
    {
        var notification = CreateNotification(new ContentCacheRefresher.JsonPayload
        {
            Key = null,
            ChangeTypes = TreeChangeTypes.RefreshNode,
        });

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task HandleAsync_SkipsBlueprints()
    {
        var notification = CreateNotification(new ContentCacheRefresher.JsonPayload
        {
            Key = Guid.NewGuid(),
            ChangeTypes = TreeChangeTypes.RefreshNode,
            Blueprint = true,
        });

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task HandleAsync_EvictsRelatedDocuments()
    {
        const int changedId = 10;
        const int parentId = 20;
        var parentKey = Guid.NewGuid();

        var relation = new Mock<IRelation>();
        relation.Setup(r => r.ParentId).Returns(parentId);

        _relationServiceMock
            .Setup(r => r.GetByChildId(changedId, Constants.Conventions.RelationTypes.RelatedDocumentAlias))
            .Returns(new[] { relation.Object });

        _idKeyMapMock
            .Setup(m => m.GetKeyForId(parentId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(parentKey));

        var notification = CreateNotification(new ContentCacheRefresher.JsonPayload
        {
            Id = changedId,
            Key = Guid.NewGuid(),
            ChangeTypes = TreeChangeTypes.RefreshNode,
        });

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{parentKey}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_DeduplicatesRelatedDocuments()
    {
        const int parentId = 20;
        var parentKey = Guid.NewGuid();

        var relation = new Mock<IRelation>();
        relation.Setup(r => r.ParentId).Returns(parentId);

        // Both changed items reference the same parent.
        _relationServiceMock
            .Setup(r => r.GetByChildId(It.IsAny<int>(), Constants.Conventions.RelationTypes.RelatedDocumentAlias))
            .Returns(new[] { relation.Object });

        _idKeyMapMock
            .Setup(m => m.GetKeyForId(parentId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(parentKey));

        var notification = CreateNotification(
            new ContentCacheRefresher.JsonPayload { Id = 10, Key = Guid.NewGuid(), ChangeTypes = TreeChangeTypes.RefreshNode },
            new ContentCacheRefresher.JsonPayload { Id = 11, Key = Guid.NewGuid(), ChangeTypes = TreeChangeTypes.RefreshNode });

        await _handler.HandleAsync(notification, CancellationToken.None);

        // Parent should only be evicted once despite being referenced by both payloads.
        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{parentKey}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void RebuildHandler()
    {
        _handler = new DeliveryApiDocumentOutputCacheEvictionHandler(
            _storeMock.Object,
            _relationServiceMock.Object,
            _idKeyMapMock.Object,
            _evictionProviderMocks.Select(m => m.Object),
            NullLogger<DeliveryApiDocumentOutputCacheEvictionHandler>.Instance);
    }

    private static ContentCacheRefresherNotification CreateNotification(params ContentCacheRefresher.JsonPayload[] payloads)
        => new(payloads, MessageType.RefreshByPayload);
}
