// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OpenIddict.Abstractions;
using Umbraco.Cms.Api.Management.Handlers;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Handlers;

[TestFixture]
public class RevokeUserAuthenticationTokensNotificationHandlerTests
{
    private const string BackOfficeApplicationId = "back-office-application-id";
    private const string OtherApplicationId = "other-application-id";

    private readonly Guid _userKey = Guid.NewGuid();
    private readonly object _backOfficeToken = new();
    private readonly object _otherClientToken = new();

    private Mock<IUserService> _userService = null!;
    private Mock<IOpenIddictTokenManager> _tokenManager = null!;
    private Mock<IOpenIddictApplicationManager> _applicationManager = null!;

    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>();
        _userService.Setup(s => s.GetUserById(1234)).Returns(Mock.Of<IUser>(u => u.Id == 1234 && u.Key == _userKey));

        var backOfficeApplication = new object();
        _applicationManager = new Mock<IOpenIddictApplicationManager>();
        _applicationManager
            .Setup(m => m.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync(backOfficeApplication);
        _applicationManager
            .Setup(m => m.GetIdAsync(backOfficeApplication, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BackOfficeApplicationId);

        _tokenManager = new Mock<IOpenIddictTokenManager>();
        _tokenManager
            .Setup(m => m.FindBySubjectAsync(_userKey.ToString(), It.IsAny<CancellationToken>()))
            .Returns(new[] { _backOfficeToken, _otherClientToken }.ToAsyncEnumerable());
        _tokenManager
            .Setup(m => m.FindAsync(_userKey.ToString(), BackOfficeApplicationId, null, null, It.IsAny<CancellationToken>()))
            .Returns(new[] { _backOfficeToken }.ToAsyncEnumerable());
        _tokenManager
            .Setup(m => m.FindAsync(_userKey.ToString(), OtherApplicationId, null, null, It.IsAny<CancellationToken>()))
            .Returns(new[] { _otherClientToken }.ToAsyncEnumerable());
        _tokenManager
            .Setup(m => m.FindAsync(It.IsAny<string?>(), It.IsNotIn(BackOfficeApplicationId, OtherApplicationId), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Array.Empty<object>().ToAsyncEnumerable());
    }

    [Test]
    public async Task Logout_Revokes_Only_Back_Office_Tokens()
    {
        await CreateHandler(new SecuritySettings())
            .HandleAsync(new UserLogoutSuccessNotification("127.0.0.1", "1234", "1234"), CancellationToken.None);

        AssertOnlyBackOfficeTokenRevoked();
    }

    [Test]
    public async Task Login_Without_Concurrent_Logins_Revokes_Tokens_For_All_Clients()
    {
        await CreateHandler(new SecuritySettings { UserAllowConcurrentLogins = false })
            .HandleAsync(new UserLoginSuccessNotification("127.0.0.1", "1234", "1234"), CancellationToken.None);

        AssertAllTokensRevoked();
    }

    [Test]
    public async Task Login_With_Concurrent_Logins_Does_Not_Revoke_Tokens()
    {
        await CreateHandler(new SecuritySettings { UserAllowConcurrentLogins = true })
            .HandleAsync(new UserLoginSuccessNotification("127.0.0.1", "1234", "1234"), CancellationToken.None);

        _tokenManager.Verify(m => m.DeleteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Locking_Out_User_Revokes_Tokens_For_All_Clients()
    {
        IUser user = Mock.Of<IUser>(u => u.Id == 1234 && u.Key == _userKey && u.IsLockedOut == true && u.IsApproved == true);

        await CreateHandler(new SecuritySettings())
            .HandleAsync(new UserSavedNotification(user, new EventMessages()), CancellationToken.None);

        AssertAllTokensRevoked();
    }

    [Test]
    public async Task Logout_Does_Not_Revoke_Tokens_When_Back_Office_Application_Is_Missing()
    {
        _applicationManager
            .Setup(m => m.FindByClientIdAsync(Constants.OAuthClientIds.BackOffice, It.IsAny<CancellationToken>()))
            .ReturnsAsync((object?)null);

        await CreateHandler(new SecuritySettings())
            .HandleAsync(new UserLogoutSuccessNotification("127.0.0.1", "1234", "1234"), CancellationToken.None);

        _tokenManager.Verify(m => m.DeleteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Can_Ignore_Logout_Of_Unknown_User()
    {
        await CreateHandler(new SecuritySettings())
            .HandleAsync(new UserLogoutSuccessNotification("127.0.0.1", null, "1234"), CancellationToken.None);

        _tokenManager.Verify(m => m.DeleteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private void AssertOnlyBackOfficeTokenRevoked()
    {
        _tokenManager.Verify(m => m.DeleteAsync(_backOfficeToken, It.IsAny<CancellationToken>()), Times.Once);
        _tokenManager.Verify(m => m.DeleteAsync(_otherClientToken, It.IsAny<CancellationToken>()), Times.Never);
    }

    private void AssertAllTokensRevoked()
    {
        _tokenManager.Verify(m => m.DeleteAsync(_backOfficeToken, It.IsAny<CancellationToken>()), Times.Once);
        _tokenManager.Verify(m => m.DeleteAsync(_otherClientToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    private RevokeUserAuthenticationTokensNotificationHandler CreateHandler(SecuritySettings securitySettings)
        => new(
            _userService.Object,
            _tokenManager.Object,
            _applicationManager.Object,
            NullLogger<RevokeUserAuthenticationTokensNotificationHandler>.Instance,
            Options.Create(securitySettings));
}
