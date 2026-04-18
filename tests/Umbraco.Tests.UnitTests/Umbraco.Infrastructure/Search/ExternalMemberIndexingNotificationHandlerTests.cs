// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Search;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Search;

[TestFixture]
public class ExternalMemberIndexingNotificationHandlerTests
{
    private Mock<IUmbracoIndexingHandler> _mockIndexingHandler = null!;
    private Mock<IExternalMemberService> _mockExternalMemberService = null!;
    private ExternalMemberIndexingNotificationHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockIndexingHandler = new Mock<IUmbracoIndexingHandler>();
        _mockIndexingHandler.Setup(x => x.Enabled).Returns(true);
        _mockExternalMemberService = new Mock<IExternalMemberService>();

        _sut = new ExternalMemberIndexingNotificationHandler(
            _mockExternalMemberService.Object,
            _mockIndexingHandler.Object);
    }

    [Test]
    public void GivenRefreshByPayload_WhenMemberNotRemoved_ThenReIndexIsCalled()
    {
        // Arrange
        var key = Guid.NewGuid();
        var member = new ExternalMemberIdentity { Key = key, Email = "test@example.com", UserName = "test", Name = "Test" };
        _mockExternalMemberService.Setup(x => x.GetByKeyAsync(key)).ReturnsAsync(member);

        var payload = new[] { new ExternalMemberCacheRefresher.JsonPayload(42, key, removed: false) };
        var notification = new ExternalMemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockIndexingHandler.Verify(x => x.ReIndexForExternalMember(member), Times.Once);
        _mockIndexingHandler.Verify(x => x.DeleteExternalMemberFromIndex(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void GivenRefreshByPayload_WhenMemberRemoved_ThenDeleteIsCalled()
    {
        // Arrange
        var payload = new[] { new ExternalMemberCacheRefresher.JsonPayload(42, Guid.NewGuid(), removed: true) };
        var notification = new ExternalMemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockIndexingHandler.Verify(x => x.DeleteExternalMemberFromIndex(42), Times.Once);
        _mockIndexingHandler.Verify(x => x.ReIndexForExternalMember(It.IsAny<ExternalMemberIdentity>()), Times.Never);
        _mockExternalMemberService.Verify(x => x.GetByKeyAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void GivenRefreshByPayload_WhenIndexingDisabled_ThenNothingHappens()
    {
        // Arrange
        _mockIndexingHandler.Setup(x => x.Enabled).Returns(false);

        var payload = new[] { new ExternalMemberCacheRefresher.JsonPayload(42, Guid.NewGuid(), removed: false) };
        var notification = new ExternalMemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockExternalMemberService.Verify(x => x.GetByKeyAsync(It.IsAny<Guid>()), Times.Never);
        _mockIndexingHandler.Verify(x => x.ReIndexForExternalMember(It.IsAny<ExternalMemberIdentity>()), Times.Never);
        _mockIndexingHandler.Verify(x => x.DeleteExternalMemberFromIndex(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void GivenRefreshByPayload_WhenMemberLookupReturnsNull_ThenReIndexNotCalled()
    {
        // Arrange
        _mockExternalMemberService.Setup(x => x.GetByKeyAsync(It.IsAny<Guid>())).ReturnsAsync((ExternalMemberIdentity?)null);

        var payload = new[] { new ExternalMemberCacheRefresher.JsonPayload(42, Guid.NewGuid(), removed: false) };
        var notification = new ExternalMemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockIndexingHandler.Verify(x => x.ReIndexForExternalMember(It.IsAny<ExternalMemberIdentity>()), Times.Never);
        _mockIndexingHandler.Verify(x => x.DeleteExternalMemberFromIndex(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void GivenRefreshByPayload_WithMultiplePayloads_ThenEachIsProcessed()
    {
        // Arrange
        var key1 = Guid.NewGuid();
        var key3 = Guid.NewGuid();
        var member1 = new ExternalMemberIdentity { Key = key1, Email = "one@example.com", UserName = "one", Name = "One" };
        var member3 = new ExternalMemberIdentity { Key = key3, Email = "three@example.com", UserName = "three", Name = "Three" };
        _mockExternalMemberService.Setup(x => x.GetByKeyAsync(key1)).ReturnsAsync(member1);
        _mockExternalMemberService.Setup(x => x.GetByKeyAsync(key3)).ReturnsAsync(member3);

        var payload = new[]
        {
            new ExternalMemberCacheRefresher.JsonPayload(1, key1, removed: false),
            new ExternalMemberCacheRefresher.JsonPayload(2, Guid.NewGuid(), removed: true),
            new ExternalMemberCacheRefresher.JsonPayload(3, key3, removed: false),
        };
        var notification = new ExternalMemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockIndexingHandler.Verify(x => x.ReIndexForExternalMember(member1), Times.Once);
        _mockIndexingHandler.Verify(x => x.ReIndexForExternalMember(member3), Times.Once);
        _mockIndexingHandler.Verify(x => x.DeleteExternalMemberFromIndex(2), Times.Once);
    }
}
