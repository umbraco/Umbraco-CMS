// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.ActionResults;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

[TestFixture]
public class UmbExternalLoginControllerTests
{
    private Mock<IMemberManager> _memberManagerMock = null!;
    private Mock<IMemberSignInManager> _signInManagerMock = null!;
    private Mock<ITwoFactorLoginService> _twoFactorLoginServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _memberManagerMock = new Mock<IMemberManager>();
        _signInManagerMock = new Mock<IMemberSignInManager>();
        _twoFactorLoginServiceMock = new Mock<ITwoFactorLoginService>();
    }

    [Test]
    public async Task ExternalLoginCallback_NoLoginInfo_ReturnsCurrentPageWithError()
    {
        _signInManagerMock
            .Setup(x => x.GetExternalLoginInfoAsync(It.IsAny<string?>()))
            .ReturnsAsync((ExternalLoginInfo?)null);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLoginCallback("/after");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        var errors = controller.ViewData.GetExternalSignInProviderErrors();
        Assert.IsNotNull(errors);
        Assert.Contains("Invalid response from the login provider", errors!.Errors.ToList());
    }

    [Test]
    public async Task ExternalLoginCallback_Success_RedirectsToReturnUrl()
    {
        var loginInfo = CreateLoginInfo();
        _signInManagerMock
            .Setup(x => x.GetExternalLoginInfoAsync(It.IsAny<string?>()))
            .ReturnsAsync(loginInfo);
        _signInManagerMock
            .Setup(x => x.ExternalLoginSignInAsync(loginInfo, false, It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLoginCallback("/after");

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/after", redirect!.Url);
        _signInManagerMock.Verify(x => x.UpdateExternalAuthenticationTokensAsync(loginInfo), Times.Once);
    }

    [Test]
    public async Task ExternalLoginCallback_Success_WithNonLocalReturnUrl_FallsBackToCurrentPage()
    {
        var loginInfo = CreateLoginInfo();
        _signInManagerMock
            .Setup(x => x.GetExternalLoginInfoAsync(It.IsAny<string?>()))
            .ReturnsAsync(loginInfo);
        _signInManagerMock
            .Setup(x => x.ExternalLoginSignInAsync(loginInfo, false, It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLoginCallback("https://evil.com");

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task ExternalLinkLoginCallback_Success_WithNonLocalReturnUrl_FallsBackToCurrentPage()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid(), Id = "1" };
        var loginInfo = CreateLoginInfo();
        _memberManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _memberManagerMock.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync("1");
        _signInManagerMock.Setup(x => x.GetExternalLoginInfoAsync("1")).ReturnsAsync(loginInfo);
        _memberManagerMock.Setup(x => x.AddLoginAsync(user, loginInfo)).ReturnsAsync(IdentityResult.Success);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLinkLoginCallback("https://evil.com");

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task Disassociate_Success_WithNonLocalReturnUrl_FallsBackToCurrentPage()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid(), Id = "1" };
        _memberManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _memberManagerMock
            .Setup(x => x.RemoveLoginAsync(user, "Provider", "key"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userIdClaim: "1");

        IActionResult result = await controller.Disassociate("Provider", "key", "https://evil.com");

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task ExternalLoginCallback_TwoFactorRequired_UnknownLocalUser_ReturnsBadRequest()
    {
        var loginInfo = CreateLoginInfo();
        _signInManagerMock
            .Setup(x => x.GetExternalLoginInfoAsync(It.IsAny<string?>()))
            .ReturnsAsync(loginInfo);
        _signInManagerMock
            .Setup(x => x.ExternalLoginSignInAsync(loginInfo, false, It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.TwoFactorRequired);
        _memberManagerMock
            .Setup(x => x.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey))
            .ReturnsAsync((MemberIdentityUser?)null);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLoginCallback("/after");

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task ExternalLoginCallback_TwoFactorRequired_PopulatesProviderNames()
    {
        var loginInfo = CreateLoginInfo();
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _signInManagerMock
            .Setup(x => x.GetExternalLoginInfoAsync(It.IsAny<string?>()))
            .ReturnsAsync(loginInfo);
        _signInManagerMock
            .Setup(x => x.ExternalLoginSignInAsync(loginInfo, false, It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.TwoFactorRequired);
        _memberManagerMock
            .Setup(x => x.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey))
            .ReturnsAsync(user);
        _twoFactorLoginServiceMock
            .Setup(x => x.GetEnabledTwoFactorProviderNamesAsync(user.Key))
            .ReturnsAsync(["AppAuth"]);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLoginCallback("/after");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ViewData.TryGetTwoFactorProviderNames(out var names));
        Assert.That(names, Is.EquivalentTo(new[] { "AppAuth" }));
    }

    [Test]
    public async Task ExternalLoginCallback_LockedOut_ReturnsCurrentPageWithError()
    {
        var loginInfo = CreateLoginInfo();
        _signInManagerMock
            .Setup(x => x.GetExternalLoginInfoAsync(It.IsAny<string?>()))
            .ReturnsAsync(loginInfo);
        _signInManagerMock
            .Setup(x => x.ExternalLoginSignInAsync(loginInfo, false, It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.LockedOut);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLoginCallback("/after");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        var errors = controller.ViewData.GetExternalSignInProviderErrors();
        Assert.IsNotNull(errors);
        Assert.That(errors!.Errors.Single(), Does.Contain("is locked out"));
    }

    [Test]
    public async Task ExternalLoginCallback_Failed_ReturnsCurrentPageWithError()
    {
        var loginInfo = CreateLoginInfo();
        _signInManagerMock
            .Setup(x => x.GetExternalLoginInfoAsync(It.IsAny<string?>()))
            .ReturnsAsync(loginInfo);
        _signInManagerMock
            .Setup(x => x.ExternalLoginSignInAsync(loginInfo, false, It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLoginCallback("/after");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        var errors = controller.ViewData.GetExternalSignInProviderErrors();
        Assert.IsNotNull(errors);
        Assert.That(errors!.Errors.Single(), Does.Contain("has not been linked to an account"));
    }

    [Test]
    public async Task ExternalLinkLoginCallback_NoLocalUser_ReturnsCurrentPageWithError()
    {
        _memberManagerMock
            .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((MemberIdentityUser?)null);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLinkLoginCallback("/after");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        var errors = controller.ViewData.GetExternalSignInProviderErrors();
        Assert.IsNotNull(errors);
        Assert.Contains("Local user does not exist", errors!.Errors.ToList());
    }

    [Test]
    public async Task ExternalLinkLoginCallback_NoExternalInfo_ReturnsCurrentPageWithError()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid(), Id = "1" };
        _memberManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _memberManagerMock.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync("1");
        _signInManagerMock.Setup(x => x.GetExternalLoginInfoAsync("1")).ReturnsAsync((ExternalLoginInfo?)null);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLinkLoginCallback("/after");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        var errors = controller.ViewData.GetExternalSignInProviderErrors();
        Assert.IsNotNull(errors);
        Assert.Contains("An error occurred, could not get external login info", errors!.Errors.ToList());
    }

    [Test]
    public async Task ExternalLinkLoginCallback_Success_RedirectsToReturnUrl()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid(), Id = "1" };
        var loginInfo = CreateLoginInfo();
        _memberManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _memberManagerMock.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync("1");
        _signInManagerMock.Setup(x => x.GetExternalLoginInfoAsync("1")).ReturnsAsync(loginInfo);
        _memberManagerMock.Setup(x => x.AddLoginAsync(user, loginInfo)).ReturnsAsync(IdentityResult.Success);

        var controller = CreateController();

        IActionResult result = await controller.ExternalLinkLoginCallback("/after");

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/after", redirect!.Url);
        _signInManagerMock.Verify(x => x.UpdateExternalAuthenticationTokensAsync(loginInfo), Times.Once);
    }

    [Test]
    public async Task ExternalLinkLoginCallback_AddLoginFails_ReturnsCurrentPageWithErrors()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid(), Id = "1" };
        var loginInfo = CreateLoginInfo();
        _memberManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
        _memberManagerMock.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync("1");
        _signInManagerMock.Setup(x => x.GetExternalLoginInfoAsync("1")).ReturnsAsync(loginInfo);
        _memberManagerMock
            .Setup(x => x.AddLoginAsync(user, loginInfo))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Already linked" }));

        var controller = CreateController();

        IActionResult result = await controller.ExternalLinkLoginCallback("/after");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        var errors = controller.ViewData.GetExternalSignInProviderErrors();
        Assert.IsNotNull(errors);
        Assert.Contains("Already linked", errors!.Errors.ToList());
    }

    [Test]
    public async Task Disassociate_Success_RedirectsToReturnUrl()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid(), Id = "1" };
        _memberManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _memberManagerMock
            .Setup(x => x.RemoveLoginAsync(user, "Provider", "key"))
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController(userIdClaim: "1");

        IActionResult result = await controller.Disassociate("Provider", "key", "/after");

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/after", redirect!.Url);
        _signInManagerMock.Verify(x => x.SignInAsync(user, false, null), Times.Once);
    }

    [Test]
    public async Task Disassociate_RemoveLoginFails_ReturnsCurrentPageWithModelErrors()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid(), Id = "1" };
        _memberManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        _memberManagerMock
            .Setup(x => x.RemoveLoginAsync(user, "Provider", "key"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Cannot remove last login" }));

        var controller = CreateController(userIdClaim: "1");

        IActionResult result = await controller.Disassociate("Provider", "key", "/after");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState[string.Empty]!.Errors.Any(e => e.ErrorMessage == "Cannot remove last login"));
    }

    [Test]
    public async Task Disassociate_NoUserIdClaim_ReturnsCurrentPage()
    {
        var controller = CreateController();

        IActionResult result = await controller.Disassociate("Provider", "key");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        _memberManagerMock.Verify(x => x.RemoveLoginAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Disassociate_UserNotFound_ReturnsCurrentPage()
    {
        _memberManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((MemberIdentityUser?)null);

        var controller = CreateController(userIdClaim: "1");

        IActionResult result = await controller.Disassociate("Provider", "key");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        _memberManagerMock.Verify(x => x.RemoveLoginAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    private static ExternalLoginInfo CreateLoginInfo()
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "ext-1"),
                new Claim(ClaimTypes.Email, "ext@example.com"),
                new Claim(ClaimTypes.Name, "External User"),
            },
            "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        return new ExternalLoginInfo(principal, "TestProvider", "ext-1", "Test Provider");
    }

    private UmbExternalLoginController CreateController(string? userIdClaim = null)
    {
        var controller = new UmbExternalLoginController(
            NullLogger<UmbExternalLoginController>.Instance,
            new TestUmbracoContextAccessor(),
            null!,
            ServiceContext.CreatePartial(),
            AppCaches.Disabled,
            Mock.Of<IProfilingLogger>(),
            Mock.Of<IPublishedUrlProvider>(),
            _signInManagerMock.Object,
            _memberManagerMock.Object,
            _twoFactorLoginServiceMock.Object,
            Options.Create(new SecuritySettings()));

        SurfaceControllerTestHelpers.ConfigureControllerContext(controller, isAuthenticated: true, userIdClaim: userIdClaim);

        return controller;
    }
}
