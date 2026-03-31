// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Security;

[TestFixture]
public class UriProviderTests
{
    private Mock<ICoreBackOfficeUserManager> _userManager = null!;
    private Mock<IHostingEnvironment> _hostingEnvironment = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessor = null!;
    private Mock<IUser> _user = null!;

    [SetUp]
    public void SetUp()
    {
        _userManager = new Mock<ICoreBackOfficeUserManager>();
        _hostingEnvironment = new Mock<IHostingEnvironment>();
        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());
        _user = new Mock<IUser>();
        _user.Setup(u => u.Key).Returns(Guid.NewGuid());
    }

    [Test]
    public async Task ForgotPasswordUri_WithApplicationMainUrl_ReturnsAbsoluteUri()
    {
        var appUrl = new Uri("https://my-site.com");
        _hostingEnvironment.Setup(h => h.ApplicationMainUrl).Returns(appUrl);
        _userManager
            .Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<IUser>()))
            .ReturnsAsync(Attempt.SucceedWithStatus(UserOperationStatus.Success, "test-token"));

        var sut = new ForgotPasswordUriProvider(
            _userManager.Object,
            _hostingEnvironment.Object,
            _httpContextAccessor.Object);

        Attempt<Uri, UserOperationStatus> result = await sut.CreateForgotPasswordUriAsync(_user.Object);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Result.IsAbsoluteUri);
        Assert.AreEqual("https", result.Result.Scheme);
        Assert.AreEqual("my-site.com", result.Result.Host);
        Assert.That(result.Result.AbsolutePath, Does.StartWith("/umbraco/login"));
        Assert.That(result.Result.Query, Does.Contain("flow=reset-password"));
    }

    [Test]
    public async Task ForgotPasswordUri_WithoutApplicationMainUrl_FailsWithApplicationUrlNotConfigured()
    {
        _hostingEnvironment.Setup(h => h.ApplicationMainUrl).Returns((Uri?)null);

        var sut = new ForgotPasswordUriProvider(
            _userManager.Object,
            _hostingEnvironment.Object,
            _httpContextAccessor.Object);

        Attempt<Uri, UserOperationStatus> result = await sut.CreateForgotPasswordUriAsync(_user.Object);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserOperationStatus.ApplicationUrlNotConfigured, result.Status);
        _userManager.Verify(m => m.GeneratePasswordResetTokenAsync(It.IsAny<IUser>()), Times.Never());
    }

    [Test]
    public async Task InviteUri_WithApplicationMainUrl_ReturnsAbsoluteUri()
    {
        var appUrl = new Uri("https://my-site.com");
        _hostingEnvironment.Setup(h => h.ApplicationMainUrl).Returns(appUrl);
        _userManager
            .Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<IUser>()))
            .ReturnsAsync(Attempt.SucceedWithStatus(UserOperationStatus.Success, "test-token"));

        var sut = new InviteUriProvider(
            _userManager.Object,
            _httpContextAccessor.Object,
            _hostingEnvironment.Object);

        Attempt<Uri, UserOperationStatus> result = await sut.CreateInviteUriAsync(_user.Object);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Result.IsAbsoluteUri);
        Assert.AreEqual("https", result.Result.Scheme);
        Assert.AreEqual("my-site.com", result.Result.Host);
        Assert.That(result.Result.AbsolutePath, Does.StartWith("/umbraco/login"));
        Assert.That(result.Result.Query, Does.Contain("flow=invite-user"));
    }

    [Test]
    public async Task InviteUri_WithoutApplicationMainUrl_FailsWithApplicationUrlNotConfigured()
    {
        _hostingEnvironment.Setup(h => h.ApplicationMainUrl).Returns((Uri?)null);

        var sut = new InviteUriProvider(
            _userManager.Object,
            _httpContextAccessor.Object,
            _hostingEnvironment.Object);

        Attempt<Uri, UserOperationStatus> result = await sut.CreateInviteUriAsync(_user.Object);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(UserOperationStatus.ApplicationUrlNotConfigured, result.Status);
        _userManager.Verify(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<IUser>()), Times.Never());
    }
}
