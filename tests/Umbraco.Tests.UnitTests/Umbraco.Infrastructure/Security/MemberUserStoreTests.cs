using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

[TestFixture]
public class MemberUserStoreTests
{
    private Mock<IMemberService> _mockMemberService;
    private Mock<IExternalMemberService> _mockExternalMemberService;

    public MemberUserStore CreateSut()
    {
        _mockMemberService = new Mock<IMemberService>();
        _mockExternalMemberService = new Mock<IExternalMemberService>();
        var mockScopeProvider = TestHelper.ScopeProvider;

        return new MemberUserStore(
            _mockMemberService.Object,
            new UmbracoMapper(new MapDefinitionCollection(() => new List<IMapDefinition>()), mockScopeProvider, NullLogger<UmbracoMapper>.Instance),
            mockScopeProvider,
            new IdentityErrorDescriber(),
            Mock.Of<IExternalLoginWithKeyService>(),
            Mock.Of<ITwoFactorLoginService>(),
            Mock.Of<IPublishedMemberCache>(),
            _mockExternalMemberService.Object);
    }

    [Test]
    public async Task GivenISetNormalizedUserName_ThenIShouldGetASuccessResult()
    {
        // Arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser { UserName = "MyName" };

        // Act
        await sut.SetNormalizedUserNameAsync(fakeUser, "NewName", CancellationToken.None);

        // Assert
        Assert.AreEqual("NewName", fakeUser.UserName);
        Assert.AreEqual("NewName", await sut.GetNormalizedUserNameAsync(fakeUser, CancellationToken.None));
    }

    [Test]
    public async Task GivenICreateUser_AndTheUserIsNull_ThenIShouldGetAFailedResultAsync()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.CreateAsync(null);

        // Assert
        Assert.IsFalse(actual.Succeeded);
        Assert.IsTrue(actual.Errors.Any(x =>
            x.Code == "IdentityErrorUserStore" && x.Description == "Value cannot be null. (Parameter 'user')"));
        _mockMemberService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenICreateUser_AndTheUserDoesNotHaveIdentity_ThenIShouldGetAFailedResultAsync()
    {
        // Arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser();

        IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);
        var mockMember = Mock.Of<IMember>(m =>
            m.Name == "fakeName" &&
            m.Email == "fakeemail@umbraco.com" &&
            m.Username == "fakeUsername" &&
            m.RawPasswordValue == "fakePassword" &&
            m.ContentTypeAlias == fakeMemberType.Alias &&
            m.HasIdentity == false);

        _mockMemberService
            .Setup(x => x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(mockMember);
        _mockMemberService.Setup(x => x.Save(mockMember, Constants.Security.SuperUserId));

        // Act
        var actual = await sut.CreateAsync(null);

        // Assert
        Assert.IsFalse(actual.Succeeded);
        Assert.IsTrue(actual.Errors.Any(x =>
            x.Code == "IdentityErrorUserStore" && x.Description == "Value cannot be null. (Parameter 'user')"));
        _mockMemberService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenICreateANewUser_AndTheUserIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
    {
        // Arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser();

        IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);
        var mockMember = Mock.Of<IMember>(m =>
            m.Name == "fakeName" &&
            m.Email == "fakeemail@umbraco.com" &&
            m.Username == "fakeUsername" &&
            m.RawPasswordValue == "fakePassword" &&
            m.Comments == "hello" &&
            m.ContentTypeAlias == fakeMemberType.Alias &&
            m.HasIdentity == true);

        _mockMemberService
            .Setup(x => x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(mockMember);
        _mockMemberService
            .Setup(x => x.Save(mockMember, PublishNotificationSaveOptions.Saving, Constants.Security.SuperUserId))
            .Returns(Attempt.Succeed<OperationResult?>(null));
        // Act
        var identityResult = await sut.CreateAsync(fakeUser, CancellationToken.None);

        // Assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());
        _mockMemberService.Verify(x =>
            x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        _mockMemberService.Verify(x => x.Save(mockMember, PublishNotificationSaveOptions.Saving, Constants.Security.SuperUserId));
    }

    [Test]
    public async Task GivenIUpdateAUser_ThenIShouldGetASuccessResultAsync()
    {
        // Arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser
        {
            Id = "123",
            Name = "fakeName",
            Email = "fakeemail@umbraco.com",
            UserName = "fakeUsername",
            Comments = "hello",
            LastLoginDate = DateTime.UtcNow,
            LastPasswordChangeDate = DateTime.UtcNow,
            EmailConfirmed = true,
            AccessFailedCount = 3,
            LockoutEnd = DateTime.UtcNow.AddDays(10),
            IsApproved = true,
            PasswordHash = "abcde",
            SecurityStamp = "abc",
        };
        fakeUser.Roles.Add(new IdentityUserRole<string> { RoleId = "role1", UserId = "123" });
        fakeUser.Roles.Add(new IdentityUserRole<string> { RoleId = "role2", UserId = "123" });

        IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);
        var mockMember = Mock.Of<IMember>(m =>
            m.Id == 123 &&
            m.Name == "a" &&
            m.Email == "a@b.com" &&
            m.Username == "c" &&
            m.RawPasswordValue == "d" &&
            m.Comments == "e" &&
            m.ContentTypeAlias == fakeMemberType.Alias &&
            m.HasIdentity == true &&
            m.EmailConfirmedDate == DateTime.MinValue &&
            m.FailedPasswordAttempts == 0 &&
            m.LastLockoutDate == DateTime.MinValue &&
            m.IsApproved == false &&
            m.RawPasswordValue == "xyz" &&
            m.SecurityStamp == "xyz");

        _mockMemberService.Setup(x => x.Save(mockMember, Constants.Security.SuperUserId));
        _mockMemberService.Setup(x => x.GetById(123)).Returns(mockMember);

        // Act
        var identityResult = await sut.UpdateAsync(fakeUser, CancellationToken.None);

        // Assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());

        Assert.AreEqual(fakeUser.Name, mockMember.Name);
        Assert.AreEqual(fakeUser.Email, mockMember.Email);
        Assert.AreEqual(fakeUser.UserName, mockMember.Username);
        Assert.AreEqual(fakeUser.Comments, mockMember.Comments);
        Assert.AreEqual(fakeUser.LastPasswordChangeDate, mockMember.LastPasswordChangeDate);
        Assert.AreEqual(fakeUser.LastLoginDate, mockMember.LastLoginDate);
        Assert.AreEqual(fakeUser.AccessFailedCount, mockMember.FailedPasswordAttempts);
        Assert.AreEqual(fakeUser.IsLockedOut, mockMember.IsLockedOut);
        Assert.AreEqual(fakeUser.IsApproved, mockMember.IsApproved);
        Assert.AreEqual(fakeUser.PasswordHash, mockMember.RawPasswordValue);
        Assert.AreEqual(fakeUser.SecurityStamp, mockMember.SecurityStamp);
        Assert.AreNotEqual(DateTime.MinValue, mockMember.EmailConfirmedDate.Value);

        _mockMemberService.Verify(x => x.Save(mockMember, Constants.Security.SuperUserId));
        _mockMemberService.Verify(x => x.GetById(123));
        _mockMemberService.Verify(x => x.ReplaceRoles(new[] { 123 }, new[] { "role1", "role2" }));
    }

    [Test]
    public async Task GivenIUpdateAUsersLoginPropertiesOnly_ThenIShouldGetASuccessResultAsync()
    {
        // Arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser
        {
            Id = "123",
            Name = "a",
            Email = "a@b.com",
            UserName = "c",
            Comments = "e",
            LastLoginDate = DateTime.UtcNow,
            SecurityStamp = "abc",
        };

        IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);
        var mockMember = Mock.Of<IMember>(m =>
            m.Id == 123 &&
            m.Name == "a" &&
            m.Email == "a@b.com" &&
            m.Username == "c" &&
            m.Comments == "e" &&
            m.ContentTypeAlias == fakeMemberType.Alias &&
            m.HasIdentity == true &&
            m.EmailConfirmedDate == DateTime.MinValue &&
            m.FailedPasswordAttempts == 0 &&
            m.LastLockoutDate == DateTime.MinValue &&
            m.IsApproved == false &&
            m.RawPasswordValue == "xyz" &&
            m.SecurityStamp == "xyz");

        _mockMemberService.Setup(x => x.UpdateLoginPropertiesAsync(mockMember));
        _mockMemberService.Setup(x => x.GetById(123)).Returns(mockMember);

        // Act
        var identityResult = await sut.UpdateAsync(fakeUser, CancellationToken.None);

        // Assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());

        Assert.AreEqual(fakeUser.Name, mockMember.Name);
        Assert.AreEqual(fakeUser.Email, mockMember.Email);
        Assert.AreEqual(fakeUser.UserName, mockMember.Username);
        Assert.AreEqual(fakeUser.Comments, mockMember.Comments);
        Assert.IsFalse(fakeUser.LastPasswordChangeDate.HasValue);
        Assert.AreEqual(fakeUser.LastLoginDate.Value, mockMember.LastLoginDate);
        Assert.AreEqual(fakeUser.AccessFailedCount, mockMember.FailedPasswordAttempts);
        Assert.AreEqual(fakeUser.IsLockedOut, mockMember.IsLockedOut);
        Assert.AreEqual(fakeUser.IsApproved, mockMember.IsApproved);
        Assert.AreEqual(fakeUser.SecurityStamp, mockMember.SecurityStamp);

        _mockMemberService.Verify(x => x.Save(mockMember, Constants.Security.SuperUserId), Times.Never);
        _mockMemberService.Verify(x => x.UpdateLoginPropertiesAsync(mockMember));
        _mockMemberService.Verify(x => x.GetById(123));
    }

    [Test]
    public async Task GivenIDeleteUser_AndTheUserIsNotPresent_ThenIShouldGetAFailedResultAsync()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var actual = await sut.DeleteAsync(null);

        // Assert
        Assert.IsTrue(actual.Succeeded == false);
        Assert.IsTrue(actual.Errors.Any(x =>
            x.Code == "IdentityErrorUserStore" && x.Description == "Value cannot be null. (Parameter 'user')"));
        _mockMemberService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenIDeleteUser_AndTheUserIsDeletedCorrectly_ThenIShouldGetASuccessResultAsync()
    {
        // Arrange
        var memberKey = new Guid("4B003A55-1DE9-4DEB-95A0-352FFC693D8F");
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser(777) { Key = memberKey };
        var fakeCancellationToken = CancellationToken.None;

        IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);
        IMember mockMember = new Member(fakeMemberType)
        {
            Id = 777,
            Key = memberKey,
            Name = "fakeName",
            Email = "fakeemail@umbraco.com",
            Username = "fakeUsername",
            RawPasswordValue = "fakePassword",
        };

        _mockMemberService.Setup(x => x.GetById(mockMember.Id)).Returns(mockMember);
        _mockMemberService.Setup(x => x.GetById(mockMember.Key)).Returns(mockMember);
        _mockMemberService.Setup(x => x.Delete(mockMember, Constants.Security.SuperUserId));

        // Act
        var identityResult = await sut.DeleteAsync(fakeUser, fakeCancellationToken);

        // Assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());
        _mockMemberService.Verify(x => x.GetById(mockMember.Key));
        _mockMemberService.Verify(x => x.Delete(mockMember, Constants.Security.SuperUserId));
        _mockMemberService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenAnExternalOnlyMember_WhenFindByEmail_ThenExternalMemberServiceIsUsed()
    {
        // Arrange
        var sut = CreateSut();
        var email = "external@test.com";
        _mockMemberService.Setup(x => x.GetByEmail(email)).Returns((IMember?)null);

        var externalMember = new ExternalMemberIdentity
        {
            Id = 999,
            Key = Guid.NewGuid(),
            Email = email,
            UserName = email,
            Name = "External Test",
            IsApproved = true,
            CreateDate = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        _mockExternalMemberService.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(externalMember);

        // Act
        var result = await sut.FindByEmailAsync(email, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result!.IsExternalOnly);
        Assert.AreEqual(email, result.Email);
        _mockExternalMemberService.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Test]
    public async Task GivenAnExternalOnlyMember_WhenFindByName_ThenExternalMemberServiceIsUsed()
    {
        // Arrange
        var sut = CreateSut();
        var userName = "external@test.com";
        _mockMemberService.Setup(x => x.GetByUsername(userName)).Returns((IMember?)null);

        var externalMember = new ExternalMemberIdentity
        {
            Id = 999,
            Key = Guid.NewGuid(),
            Email = userName,
            UserName = userName,
            Name = "External Test",
            IsApproved = true,
            CreateDate = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString(),
        };
        _mockExternalMemberService.Setup(x => x.GetByUsernameAsync(userName)).ReturnsAsync(externalMember);

        // Act
        var result = await sut.FindByNameAsync(userName, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result!.IsExternalOnly);
        Assert.AreEqual(userName, result.UserName);
        _mockExternalMemberService.Verify(x => x.GetByUsernameAsync(userName), Times.Once);
    }

    [Test]
    public async Task GivenAKeyNotInExternalStore_WhenFindByEmail_ThenReturnsNull()
    {
        // Arrange
        var sut = CreateSut();
        var email = "nobody@test.com";
        _mockMemberService.Setup(x => x.GetByEmail(email)).Returns((IMember?)null);
        _mockExternalMemberService.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((ExternalMemberIdentity?)null);

        // Act
        var result = await sut.FindByEmailAsync(email, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void GivenAnExternalOnlyMember_WhenGetPublishedMember_ThenReturnsNull()
    {
        // Arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser { IsExternalOnly = true, Key = Guid.NewGuid() };

        // Act
        var result = sut.GetPublishedMember(fakeUser);

        // Assert
        Assert.IsNull(result);
        _mockMemberService.Verify(x => x.GetById(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task GivenAnExternalOnlyMember_WhenCreateAsync_ThenExternalMemberServiceCreateIsCalled()
    {
        // Arrange
        var sut = CreateSut();
        var memberKey = Guid.NewGuid();
        var fakeUser = new MemberIdentityUser
        {
            UserName = "external@test.com",
            Email = "external@test.com",
            Name = "External Test",
            IsApproved = true,
            IsExternalOnly = true,
            Key = memberKey,
        };

        var createdIdentity = new ExternalMemberIdentity
        {
            Id = 999,
            Key = memberKey,
            Email = fakeUser.Email,
            UserName = fakeUser.UserName,
            Name = fakeUser.Name,
            IsApproved = true,
            CreateDate = DateTime.UtcNow,
        };

        _mockExternalMemberService
            .Setup(x => x.CreateAsync(It.IsAny<ExternalMemberIdentity>(), null))
            .ReturnsAsync(Attempt.SucceedWithStatus<ExternalMemberIdentity, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.Success, createdIdentity));

        // Act
        var identityResult = await sut.CreateAsync(fakeUser, CancellationToken.None);

        // Assert
        Assert.IsTrue(identityResult.Succeeded);
        _mockExternalMemberService.Verify(x => x.CreateAsync(It.IsAny<ExternalMemberIdentity>(), null), Times.Once);
        _mockMemberService.Verify(
            x => x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task GivenAnExternalOnlyMember_WhenDeleteAsync_ThenExternalMemberServiceDeleteIsCalled()
    {
        // Arrange
        var sut = CreateSut();
        var memberKey = Guid.NewGuid();
        var fakeUser = new MemberIdentityUser(999) { IsExternalOnly = true, Key = memberKey };

        _mockExternalMemberService
            .Setup(x => x.DeleteAsync(memberKey))
            .ReturnsAsync(Attempt.SucceedWithStatus<ExternalMemberIdentity?, ExternalMemberOperationStatus>(ExternalMemberOperationStatus.Success, null));

        // Act
        var identityResult = await sut.DeleteAsync(fakeUser, CancellationToken.None);

        // Assert
        Assert.IsTrue(identityResult.Succeeded);
        _mockExternalMemberService.Verify(x => x.DeleteAsync(memberKey), Times.Once);
        _mockMemberService.Verify(x => x.GetById(It.IsAny<Guid>()), Times.Never);
        _mockMemberService.Verify(x => x.Delete(It.IsAny<IMember>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task GivenAnExternalOnlyMember_WhenUpdateAsync_WithLoginPropertiesOnly_ThenLoginTimestampUsed()
    {
        // Arrange
        var sut = CreateSut();
        var memberKey = Guid.NewGuid();
        var fakeUser = new MemberIdentityUser
        {
            Id = "999",
            Key = memberKey,
            UserName = "external@test.com",
            Email = "external@test.com",
            Name = "External Test",
            IsExternalOnly = true,

            // Set LastLoginDate to make it dirty (tracked by BeingDirty).
            LastLoginDate = DateTime.UtcNow,
        };

        _mockExternalMemberService
            .Setup(x => x.UpdateLoginTimestampAsync(memberKey, It.IsAny<DateTime>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var identityResult = await sut.UpdateAsync(fakeUser, CancellationToken.None);

        // Assert
        Assert.IsTrue(identityResult.Succeeded);
        _mockExternalMemberService.Verify(
            x => x.UpdateLoginTimestampAsync(memberKey, It.IsAny<DateTime>(), It.IsAny<string>()),
            Times.Once);
        _mockExternalMemberService.Verify(
            x => x.UpdateAsync(It.IsAny<ExternalMemberIdentity>()),
            Times.Never);
    }
}
