using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

[TestFixture]
public class MemberUserStoreTests
{
    private Mock<IMemberService> _mockMemberService;

    public MemberUserStore CreateSut()
    {
        _mockMemberService = new Mock<IMemberService>();
        var mockScope = new Mock<IScope>();
        var mockScopeProvider = new Mock<IScopeProvider>();
        mockScopeProvider
            .Setup(x => x.CreateScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(mockScope.Object);

        return new MemberUserStore(
            _mockMemberService.Object,
            new UmbracoMapper(new MapDefinitionCollection(() => new List<IMapDefinition>()), mockScopeProvider.Object),
            mockScopeProvider.Object,
            new IdentityErrorDescriber(),
            Mock.Of<IPublishedSnapshotAccessor>(),
            Mock.Of<IExternalLoginWithKeyService>(),
            Mock.Of<ITwoFactorLoginService>());
    }

    [Test]
    public async Task GivenISetNormalizedUserName_ThenIShouldGetASuccessResult()
    {
        // arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser { UserName = "MyName" };

        // act
        await sut.SetNormalizedUserNameAsync(fakeUser, "NewName", CancellationToken.None);

        // assert
        Assert.AreEqual("NewName", fakeUser.UserName);
        Assert.AreEqual("NewName", await sut.GetNormalizedUserNameAsync(fakeUser, CancellationToken.None));
    }

    [Test]
    public async Task GivenICreateUser_AndTheUserIsNull_ThenIShouldGetAFailedResultAsync()
    {
        // arrange
        var sut = CreateSut();

        // act
        var actual = await sut.CreateAsync(null);

        // assert
        Assert.IsFalse(actual.Succeeded);
        Assert.IsTrue(actual.Errors.Any(x =>
            x.Code == "IdentityErrorUserStore" && x.Description == "Value cannot be null. (Parameter 'user')"));
        _mockMemberService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenICreateUser_AndTheUserDoesNotHaveIdentity_ThenIShouldGetAFailedResultAsync()
    {
        // arrange
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
        _mockMemberService.Setup(x => x.Save(mockMember));

        // act
        var actual = await sut.CreateAsync(null);

        // assert
        Assert.IsFalse(actual.Succeeded);
        Assert.IsTrue(actual.Errors.Any(x =>
            x.Code == "IdentityErrorUserStore" && x.Description == "Value cannot be null. (Parameter 'user')"));
        _mockMemberService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenICreateANewUser_AndTheUserIsPopulatedCorrectly_ThenIShouldGetASuccessResultAsync()
    {
        // arrange
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
        _mockMemberService.Setup(x => x.Save(mockMember));

        // act
        var identityResult = await sut.CreateAsync(fakeUser, CancellationToken.None);

        // assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());
        _mockMemberService.Verify(x =>
            x.CreateMember(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        _mockMemberService.Verify(x => x.Save(mockMember));
    }

    [Test]
    public async Task GivenIUpdateAUser_ThenIShouldGetASuccessResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser
        {
            Id = "123",
            Name = "fakeName",
            Email = "fakeemail@umbraco.com",
            UserName = "fakeUsername",
            Comments = "hello",
            LastLoginDateUtc = DateTime.UtcNow,
            LastPasswordChangeDateUtc = DateTime.UtcNow,
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

        _mockMemberService.Setup(x => x.Save(mockMember));
        _mockMemberService.Setup(x => x.GetById(123)).Returns(mockMember);

        // act
        var identityResult = await sut.UpdateAsync(fakeUser, CancellationToken.None);

        // assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());

        Assert.AreEqual(fakeUser.Name, mockMember.Name);
        Assert.AreEqual(fakeUser.Email, mockMember.Email);
        Assert.AreEqual(fakeUser.UserName, mockMember.Username);
        Assert.AreEqual(fakeUser.Comments, mockMember.Comments);
        Assert.AreEqual(fakeUser.LastPasswordChangeDateUtc.Value.ToLocalTime(), mockMember.LastPasswordChangeDate);
        Assert.AreEqual(fakeUser.LastLoginDateUtc.Value.ToLocalTime(), mockMember.LastLoginDate);
        Assert.AreEqual(fakeUser.AccessFailedCount, mockMember.FailedPasswordAttempts);
        Assert.AreEqual(fakeUser.IsLockedOut, mockMember.IsLockedOut);
        Assert.AreEqual(fakeUser.IsApproved, mockMember.IsApproved);
        Assert.AreEqual(fakeUser.PasswordHash, mockMember.RawPasswordValue);
        Assert.AreEqual(fakeUser.SecurityStamp, mockMember.SecurityStamp);
        Assert.AreNotEqual(DateTime.MinValue, mockMember.EmailConfirmedDate.Value);

        _mockMemberService.Verify(x => x.Save(mockMember));
        _mockMemberService.Verify(x => x.GetById(123));
        _mockMemberService.Verify(x => x.ReplaceRoles(new[] { 123 }, new[] { "role1", "role2" }));
    }

    [Test]
    public async Task GivenIDeleteUser_AndTheUserIsNotPresent_ThenIShouldGetAFailedResultAsync()
    {
        // arrange
        var sut = CreateSut();

        // act
        var actual = await sut.DeleteAsync(null);

        // assert
        Assert.IsTrue(actual.Succeeded == false);
        Assert.IsTrue(actual.Errors.Any(x =>
            x.Code == "IdentityErrorUserStore" && x.Description == "Value cannot be null. (Parameter 'user')"));
        _mockMemberService.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GivenIDeleteUser_AndTheUserIsDeletedCorrectly_ThenIShouldGetASuccessResultAsync()
    {
        // arrange
        var sut = CreateSut();
        var fakeUser = new MemberIdentityUser(777);
        var fakeCancellationToken = CancellationToken.None;

        IMemberType fakeMemberType = new MemberType(new MockShortStringHelper(), 77);
        IMember mockMember = new Member(fakeMemberType)
        {
            Id = 777,
            Name = "fakeName",
            Email = "fakeemail@umbraco.com",
            Username = "fakeUsername",
            RawPasswordValue = "fakePassword",
        };

        _mockMemberService.Setup(x => x.GetById(mockMember.Id)).Returns(mockMember);
        _mockMemberService.Setup(x => x.Delete(mockMember));

        // act
        var identityResult = await sut.DeleteAsync(fakeUser, fakeCancellationToken);

        // assert
        Assert.IsTrue(identityResult.Succeeded);
        Assert.IsTrue(!identityResult.Errors.Any());
        _mockMemberService.Verify(x => x.GetById(mockMember.Id));
        _mockMemberService.Verify(x => x.Delete(mockMember));
        _mockMemberService.VerifyNoOtherCalls();
    }
}
