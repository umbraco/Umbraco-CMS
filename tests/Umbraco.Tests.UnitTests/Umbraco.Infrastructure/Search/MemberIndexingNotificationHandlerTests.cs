// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Search;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Search;

[TestFixture]
public class MemberIndexingNotificationHandlerTests
{
    private Mock<IUmbracoIndexingHandler> _mockIndexingHandler = null!;
    private Mock<IMemberService> _mockMemberService = null!;
    private MemberIndexingNotificationHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockIndexingHandler = new Mock<IUmbracoIndexingHandler>();
        _mockIndexingHandler.Setup(x => x.Enabled).Returns(true);
        _mockMemberService = new Mock<IMemberService>();

        _sut = new MemberIndexingNotificationHandler(_mockIndexingHandler.Object, _mockMemberService.Object);
    }

    [Test]
    public void GivenRefreshByPayload_WhenMemberNotRemoved_ThenReIndexIsCalled()
    {
        // Arrange
        var member = Mock.Of<IMember>();
        _mockMemberService.Setup(x => x.GetById(42)).Returns(member);

        var payload = new[] { new MemberCacheRefresher.JsonPayload(42, "test", removed: false) };
        var notification = new MemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockIndexingHandler.Verify(x => x.ReIndexForMember(member), Times.Once);
        _mockIndexingHandler.Verify(x => x.DeleteIndexForEntity(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void GivenRefreshByPayload_WhenMemberRemoved_ThenDeleteIsCalled()
    {
        // Arrange
        var payload = new[] { new MemberCacheRefresher.JsonPayload(42, "test", removed: true) };
        var notification = new MemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockIndexingHandler.Verify(x => x.DeleteIndexForEntity(42, false), Times.Once);
        _mockIndexingHandler.Verify(x => x.ReIndexForMember(It.IsAny<IMember>()), Times.Never);
        _mockMemberService.Verify(x => x.GetById(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public void GivenRefreshByPayload_WhenIndexingDisabled_ThenNothingHappens()
    {
        // Arrange
        _mockIndexingHandler.Setup(x => x.Enabled).Returns(false);

        var payload = new[] { new MemberCacheRefresher.JsonPayload(42, "test", removed: false) };
        var notification = new MemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockMemberService.Verify(x => x.GetById(It.IsAny<int>()), Times.Never);
        _mockIndexingHandler.Verify(x => x.ReIndexForMember(It.IsAny<IMember>()), Times.Never);
        _mockIndexingHandler.Verify(x => x.DeleteIndexForEntity(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void GivenRefreshByPayload_WhenMemberLookupReturnsNull_ThenReIndexNotCalled()
    {
        // Arrange
        _mockMemberService.Setup(x => x.GetById(It.IsAny<int>())).Returns((IMember?)null);

        var payload = new[] { new MemberCacheRefresher.JsonPayload(42, "test", removed: false) };
        var notification = new MemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockIndexingHandler.Verify(x => x.ReIndexForMember(It.IsAny<IMember>()), Times.Never);
        _mockIndexingHandler.Verify(x => x.DeleteIndexForEntity(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void GivenRefreshByPayload_WhenIndexableFieldsUnchanged_ThenReIndexNotCalled()
    {
        // Arrange
        var payload = new[]
        {
            new MemberCacheRefresher.JsonPayload(42, "test", removed: false, indexableFieldsChanged: false),
        };
        var notification = new MemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockMemberService.Verify(x => x.GetById(It.IsAny<int>()), Times.Never);
        _mockIndexingHandler.Verify(x => x.ReIndexForMember(It.IsAny<IMember>()), Times.Never);
        _mockIndexingHandler.Verify(x => x.DeleteIndexForEntity(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void GivenRefreshByPayload_WithMultiplePayloads_ThenEachIsProcessed()
    {
        // Arrange
        var member1 = Mock.Of<IMember>();
        var member3 = Mock.Of<IMember>();
        _mockMemberService.Setup(x => x.GetById(1)).Returns(member1);
        _mockMemberService.Setup(x => x.GetById(3)).Returns(member3);

        var payload = new[]
        {
            new MemberCacheRefresher.JsonPayload(1, "one", removed: false),
            new MemberCacheRefresher.JsonPayload(2, "two", removed: true),
            new MemberCacheRefresher.JsonPayload(3, "three", removed: false),
        };
        var notification = new MemberCacheRefresherNotification(payload, MessageType.RefreshByPayload);

        // Act
        _sut.Handle(notification);

        // Assert
        _mockIndexingHandler.Verify(x => x.ReIndexForMember(member1), Times.Once);
        _mockIndexingHandler.Verify(x => x.ReIndexForMember(member3), Times.Once);
        _mockIndexingHandler.Verify(x => x.DeleteIndexForEntity(2, false), Times.Once);
    }
}
