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
public class DeliveryApiElementOutputCacheEvictionHandlerTests
{
    private Mock<IOutputCacheStore> _storeMock = null!;
    private Mock<IRelationService> _relationServiceMock = null!;
    private Mock<IIdKeyMap> _idKeyMapMock = null!;
    private DeliveryApiElementOutputCacheEvictionHandler _handler = null!;

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

        _handler = new DeliveryApiElementOutputCacheEvictionHandler(
            _storeMock.Object,
            _relationServiceMock.Object,
            _idKeyMapMock.Object,
            NullLogger<DeliveryApiElementOutputCacheEvictionHandler>.Instance);
    }

    [Test]
    public async Task HandleAsync_RefreshAll_EvictsAllTag()
    {
        var notification = CreateNotification(new ElementCacheRefresher.JsonPayload(1, Guid.NewGuid(), TreeChangeTypes.RefreshAll));

        await _handler.HandleAsync(notification, CancellationToken.None);

        // RefreshAll evicts everything because content responses may reference elements.
        _storeMock.Verify(
            s => s.EvictByTagAsync("umb-dapi-all", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_EvictsDocumentsReferencingChangedElement()
    {
        const int elementId = 10;
        const int parentDocumentId = 20;
        var parentDocumentKey = Guid.NewGuid();

        var relation = new Mock<IRelation>();
        relation.Setup(r => r.ParentId).Returns(parentDocumentId);

        _relationServiceMock
            .Setup(r => r.GetByChildId(elementId, Constants.Conventions.RelationTypes.RelatedElementAlias))
            .Returns(new[] { relation.Object });

        _idKeyMapMock
            .Setup(m => m.GetKeyForId(parentDocumentId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(parentDocumentKey));

        var notification = CreateNotification(new ElementCacheRefresher.JsonPayload(elementId, Guid.NewGuid(), TreeChangeTypes.RefreshNode));

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{parentDocumentKey}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WhenNoRelations_DoesNotEvict()
    {
        var notification = CreateNotification(new ElementCacheRefresher.JsonPayload(10, Guid.NewGuid(), TreeChangeTypes.RefreshNode));

        await _handler.HandleAsync(notification, CancellationToken.None);

        _storeMock.Verify(
            s => s.EvictByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task HandleAsync_DeduplicatesParentDocuments()
    {
        const int parentDocumentId = 20;
        var parentDocumentKey = Guid.NewGuid();

        var relation = new Mock<IRelation>();
        relation.Setup(r => r.ParentId).Returns(parentDocumentId);

        // Both elements reference the same parent document.
        _relationServiceMock
            .Setup(r => r.GetByChildId(It.IsAny<int>(), Constants.Conventions.RelationTypes.RelatedElementAlias))
            .Returns(new[] { relation.Object });

        _idKeyMapMock
            .Setup(m => m.GetKeyForId(parentDocumentId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(parentDocumentKey));

        var notification = CreateNotification(
            new ElementCacheRefresher.JsonPayload(10, Guid.NewGuid(), TreeChangeTypes.RefreshNode),
            new ElementCacheRefresher.JsonPayload(11, Guid.NewGuid(), TreeChangeTypes.RefreshNode));

        await _handler.HandleAsync(notification, CancellationToken.None);

        // Parent should only be evicted once despite being referenced by both elements.
        _storeMock.Verify(
            s => s.EvictByTagAsync($"umb-dapi-content-{parentDocumentKey}", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static ElementCacheRefresherNotification CreateNotification(params ElementCacheRefresher.JsonPayload[] payloads)
        => new(payloads, MessageType.RefreshByPayload);
}
