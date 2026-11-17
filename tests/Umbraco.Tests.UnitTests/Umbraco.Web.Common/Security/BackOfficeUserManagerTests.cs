// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Security;

[TestFixture]
public class BackOfficeUserManagerTests
{
    private const string TestEmail = "test@example.com";

    private Mock<IEventAggregator> _eventAggregator = null!;
    private Mock<IUserLockoutStore<BackOfficeIdentityUser>> _userStore = null!;

    [SetUp]
    public void SetUp()
    {
        _eventAggregator = new Mock<IEventAggregator>();
        _userStore = new Mock<IUserLockoutStore<BackOfficeIdentityUser>>();
        _userStore
            .Setup(x => x.GetLockoutEnabledAsync(It.IsAny<BackOfficeIdentityUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userStore
            .Setup(x => x.UpdateAsync(It.IsAny<BackOfficeIdentityUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success);
    }

    [Test]
    public async Task Can_Publish_Login_Failed_Notification_Asynchronously()
    {
        // Arrange
        BackOfficeUserManager sut = CreateSut();

        // Act
        await sut.AccessFailedAsync(CreateUser());

        // Assert
        VerifyPublishedAsynchronously<UserLoginFailedNotification>();
    }

    [Test]
    public async Task Can_Publish_Account_Locked_Notification_Asynchronously()
    {
        // Arrange
        BackOfficeUserManager sut = CreateSut();

        // Act
        await sut.SetLockoutEndDateAsync(CreateUser(), DateTimeOffset.UtcNow.AddHours(1));

        // Assert
        VerifyPublishedAsynchronously<UserLockedNotification>();
    }

    [Test]
    public async Task Can_Publish_Account_Unlocked_Notification_Asynchronously()
    {
        // Arrange
        BackOfficeUserManager sut = CreateSut();

        // Act
        await sut.SetLockoutEndDateAsync(CreateUser(), DateTimeOffset.UtcNow.AddMinutes(-1));

        // Assert
        VerifyPublishedAsynchronously<UserUnlockedNotification>();
    }

    [Test]
    public async Task Can_Publish_Reset_Access_Failed_Count_Notification_Asynchronously()
    {
        // Arrange
        BackOfficeUserManager sut = CreateSut();

        // Act
        await sut.ResetAccessFailedCountAsync(CreateUser());

        // Assert
        VerifyPublishedAsynchronously<UserResetAccessFailedCountNotification>();
    }

    [Test]
    public async Task Can_Publish_Logout_Success_Notification_Asynchronously()
    {
        // Arrange
        BackOfficeUserManager sut = CreateSut();
        const string signOutRedirectUrl = "https://example.com/signed-out";
        _eventAggregator
            .Setup(x => x.PublishAsync(It.IsAny<UserLogoutSuccessNotification>(), It.IsAny<CancellationToken>()))
            .Callback<UserLogoutSuccessNotification, CancellationToken>((notification, _) => notification.SignOutRedirectUrl = signOutRedirectUrl)
            .Returns(Task.CompletedTask);

        // Act
        SignOutSuccessResult result = await sut.NotifyLogoutSuccessAsync(new ClaimsPrincipal(), "1");

        // Assert
        VerifyPublishedAsynchronously<UserLogoutSuccessNotification>();
        Assert.AreEqual(signOutRedirectUrl, result.SignOutRedirectUrl);
    }

    private void VerifyPublishedAsynchronously<TNotification>()
        where TNotification : INotification
        => _eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<TNotification>(), It.IsAny<CancellationToken>()),
            Times.Once);

    private BackOfficeUserManager CreateSut()
        => new(
            Mock.Of<IIpResolver>(),
            _userStore.Object,
            Options.Create(new BackOfficeIdentityOptions()),
            Mock.Of<IPasswordHasher<BackOfficeIdentityUser>>(),
            Enumerable.Empty<IUserValidator<BackOfficeIdentityUser>>(),
            Enumerable.Empty<IPasswordValidator<BackOfficeIdentityUser>>(),
            new Mock<BackOfficeErrorDescriber>(Mock.Of<ILocalizedTextService>()).Object,
            Mock.Of<IServiceProvider>(),
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<ILogger<UserManager<BackOfficeIdentityUser>>>(),
            Options.Create(new UserPasswordConfigurationSettings()),
            _eventAggregator.Object,
            Mock.Of<IBackOfficeUserPasswordChecker>(),
            Options.Create(new GlobalSettings()));

    private static BackOfficeIdentityUser CreateUser()
        => BackOfficeIdentityUser.CreateNew(new GlobalSettings(), TestEmail, TestEmail, "en-US", "Test User");
}
