// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Handlers;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Handlers;

[TestFixture]
public class AuditNotificationsHandlerMemberTests
{
    private Mock<IAuditEntryService> _mockAuditEntryService = null!;
    private Mock<IMemberService> _mockMemberService = null!;
    private AuditNotificationsHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockAuditEntryService = new Mock<IAuditEntryService>();
        _mockMemberService = new Mock<IMemberService>();

        var mockIpResolver = new Mock<IIpResolver>();
        mockIpResolver.Setup(x => x.GetCurrentRequestIpAddress()).Returns("127.0.0.1");

        var mockBackOfficeSecurity = new Mock<IBackOfficeSecurity>();
        mockBackOfficeSecurity.Setup(x => x.CurrentUser).Returns((IUser?)null);
        var mockBackOfficeSecurityAccessor = new Mock<IBackOfficeSecurityAccessor>();
        mockBackOfficeSecurityAccessor.Setup(x => x.BackOfficeSecurity).Returns(mockBackOfficeSecurity.Object);

        _sut = new AuditNotificationsHandler(
            _mockAuditEntryService.Object,
            Mock.Of<IUserService>(),
            Mock.Of<IEntityService>(),
            mockIpResolver.Object,
            mockBackOfficeSecurityAccessor.Object,
            _mockMemberService.Object,
            Mock.Of<IUserGroupService>());
    }

    [Test]
    public async Task GivenAContentMember_WhenSaved_ThenAuditEntryWrittenWithMemberSaveEventType()
    {
        // Arrange
        IMemberType memberType = new MemberType(new MockShortStringHelper(), 77);
        var member = new Member("Test Member", "test@example.com", "test", memberType) { Id = 123 };
        var notification = new MemberSavedNotification(member, new EventMessages());

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "127.0.0.1",
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.Is<string>(s => s.Contains("Member") && s.Contains("Test Member") && s.Contains("test@example.com")),
                "umbraco/member/save",
                It.Is<string>(s => s.StartsWith("updating"))),
            Times.Once);
    }

    [Test]
    public async Task GivenAContentMember_WhenDeleted_ThenAuditEntryWrittenWithMemberDeleteEventType()
    {
        // Arrange
        IMemberType memberType = new MemberType(new MockShortStringHelper(), 77);
        var member = new Member("Deleted Member", "deleted@example.com", "deleted", memberType) { Id = 456 };
        var notification = new MemberDeletedNotification(member, new EventMessages());

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "127.0.0.1",
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.Is<string>(s => s.Contains("Member") && s.Contains("Deleted Member")),
                "umbraco/member/delete",
                It.Is<string>(s => s.Contains("delete member"))),
            Times.Once);
    }

    [Test]
    public async Task GivenAContentMember_WhenRolesAssigned_ThenAuditEntryWrittenWithRolesAssignedEventType()
    {
        // Arrange
        IMemberType memberType = new MemberType(new MockShortStringHelper(), 77);
        var member = new Member("Role Member", "role@example.com", "role", memberType) { Id = 789 };
        _mockMemberService.Setup(x => x.GetAllMembers(It.IsAny<int[]>())).Returns(new[] { (IMember)member });

        var notification = new AssignedMemberRolesNotification(new[] { 789 }, new[] { "Editors" });

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "127.0.0.1",
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.Is<string>(s => s.Contains("Member") && s.Contains("789")),
                "umbraco/member/roles/assigned",
                It.Is<string>(s => s.Contains("Editors"))),
            Times.Once);
    }

    [Test]
    public async Task GivenAContentMember_WhenRolesRemoved_ThenAuditEntryWrittenWithRolesRemovedEventType()
    {
        // Arrange
        IMemberType memberType = new MemberType(new MockShortStringHelper(), 77);
        var member = new Member("Role Member", "role@example.com", "role", memberType) { Id = 789 };
        _mockMemberService.Setup(x => x.GetAllMembers(It.IsAny<int[]>())).Returns(new[] { (IMember)member });

        var notification = new RemovedMemberRolesNotification(new[] { 789 }, new[] { "Editors" });

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "127.0.0.1",
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.Is<string>(s => s.Contains("Member")),
                "umbraco/member/roles/removed",
                It.Is<string>(s => s.Contains("Editors"))),
            Times.Once);
    }

    [Test]
    public async Task GivenAContentMember_WhenSaved_ThenAffectedDetailsStartsWithMember()
    {
        // Arrange
        IMemberType memberType = new MemberType(new MockShortStringHelper(), 77);
        var member = new Member("Prefix Check", "prefix@example.com", "prefix", memberType) { Id = 100 };
        var notification = new MemberSavedNotification(member, new EventMessages());

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert — content member affected details start with "Member", not "External member".
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.Is<string>(s => s.StartsWith("Member ") && !s.StartsWith("External")),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task GivenAnExternalMember_WhenSaved_ThenAuditEntryWrittenWithMemberSaveEventType()
    {
        // Arrange
        var member = new ExternalMemberIdentity
        {
            Key = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "test@example.com",
            Name = "Test External",
            IsApproved = true,
            CreateDate = DateTime.UtcNow,
        };

        var notification = new ExternalMemberSavedNotification(member, new EventMessages());

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "127.0.0.1",
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.Is<string>(s => s.Contains("External member") && s.Contains("Test External") && s.Contains("test@example.com")),
                "umbraco/member/save",
                It.Is<string>(s => s.Contains("updating external member"))),
            Times.Once);
    }

    [Test]
    public async Task GivenAnExternalMember_WhenDeleted_ThenAuditEntryWrittenWithMemberDeleteEventType()
    {
        // Arrange
        var member = new ExternalMemberIdentity
        {
            Key = Guid.NewGuid(),
            Email = "deleted@example.com",
            UserName = "deleted@example.com",
            Name = "Deleted External",
            CreateDate = DateTime.UtcNow,
        };

        var notification = new ExternalMemberDeletedNotification(member, new EventMessages());

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "127.0.0.1",
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.Is<string>(s => s.Contains("External member") && s.Contains("Deleted External")),
                "umbraco/member/delete",
                It.Is<string>(s => s.Contains("delete external member"))),
            Times.Once);
    }

    [Test]
    public async Task GivenAnExternalMember_WhenRolesAssigned_ThenAuditEntryWrittenWithRolesAssignedEventType()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var notification = new AssignedExternalMemberRolesNotification(
            new[] { memberKey },
            new[] { "Editors", "Writers" });

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "127.0.0.1",
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.Is<string>(s => s.Contains("External member") && s.Contains(memberKey.ToString())),
                "umbraco/member/roles/assigned",
                It.Is<string>(s => s.Contains("Editors") && s.Contains("Writers"))),
            Times.Once);
    }

    [Test]
    public async Task GivenAnExternalMember_WhenRolesRemoved_ThenAuditEntryWrittenWithRolesRemovedEventType()
    {
        // Arrange
        var memberKey = Guid.NewGuid();
        var notification = new RemovedExternalMemberRolesNotification(
            new[] { memberKey },
            new[] { "Editors" });

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "127.0.0.1",
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.Is<string>(s => s.Contains("External member")),
                "umbraco/member/roles/removed",
                It.Is<string>(s => s.Contains("Editors"))),
            Times.Once);
    }

    [Test]
    public async Task GivenAnExternalMember_WhenSaved_ThenAffectedDetailsStartsWithExternalMember()
    {
        // Arrange
        var member = new ExternalMemberIdentity
        {
            Key = Guid.NewGuid(),
            Email = "prefix@example.com",
            UserName = "prefix@example.com",
            Name = "Prefix Test",
            CreateDate = DateTime.UtcNow,
        };

        var notification = new ExternalMemberSavedNotification(member, new EventMessages());

        // Act
        await _sut.HandleAsync(notification, CancellationToken.None);

        // Assert
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
            It.IsAny<Guid?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<Guid?>(),
            It.Is<string>(s => s.StartsWith("External member")),
            It.IsAny<string>(),
            It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public async Task ContentAndExternalMembers_UseSameEventTypeForSave()
    {
        // Arrange
        IMemberType memberType = new MemberType(new MockShortStringHelper(), 77);
        var contentMember = new Member("Content", "content@example.com", "content", memberType) { Id = 1 };
        var externalMember = new ExternalMemberIdentity
        {
            Key = Guid.NewGuid(),
            Email = "external@example.com",
            UserName = "external@example.com",
            Name = "External",
            CreateDate = DateTime.UtcNow,
        };

        // Act
        await _sut.HandleAsync(new MemberSavedNotification(contentMember, new EventMessages()), CancellationToken.None);
        await _sut.HandleAsync(new ExternalMemberSavedNotification(externalMember, new EventMessages()), CancellationToken.None);

        // Assert — both use the same event type so audit queries for "umbraco/member/save" return both.
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "umbraco/member/save",
                It.IsAny<string>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task ContentAndExternalMembers_UseSameEventTypeForDelete()
    {
        // Arrange
        IMemberType memberType = new MemberType(new MockShortStringHelper(), 77);
        var contentMember = new Member("Content", "content@example.com", "content", memberType) { Id = 1 };
        var externalMember = new ExternalMemberIdentity
        {
            Key = Guid.NewGuid(),
            Email = "external@example.com",
            UserName = "external@example.com",
            Name = "External",
            CreateDate = DateTime.UtcNow,
        };

        // Act
        await _sut.HandleAsync(new MemberDeletedNotification(contentMember, new EventMessages()), CancellationToken.None);
        await _sut.HandleAsync(new ExternalMemberDeletedNotification(externalMember, new EventMessages()), CancellationToken.None);

        // Assert — both use "umbraco/member/delete".
        _mockAuditEntryService.Verify(
            x => x.WriteAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                "umbraco/member/delete",
                It.IsAny<string>()),
            Times.Exactly(2));
    }
}
