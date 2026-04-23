// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.ActionResults;
using Umbraco.Cms.Web.Website.Controllers;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

[TestFixture]
public class UmbTwoFactorLoginControllerTests
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
    public async Task Get2FAProviders_NoUserInFlow_ReturnsNotFound()
    {
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync((MemberIdentityUser?)null);

        var controller = CreateController();

        ActionResult<IEnumerable<string>> result = await controller.Get2FAProviders();

        Assert.IsInstanceOf<NotFoundResult>(result.Result);
    }

    [Test]
    public async Task Get2FAProviders_ReturnsProviderList()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync(user);
        _memberManagerMock
            .Setup(x => x.GetValidTwoFactorProvidersAsync(user))
            .ReturnsAsync(["ProviderA", "ProviderB"]);

        var controller = CreateController();

        ActionResult<IEnumerable<string>> result = await controller.Get2FAProviders();

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.That((IList<string>)objectResult!.Value!, Is.EquivalentTo(new[] { "ProviderA", "ProviderB" }));
    }

    [Test]
    public async Task Verify2FACode_NoUserInFlow_ReturnsNotFound()
    {
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync((MemberIdentityUser?)null);

        var controller = CreateController();

        IActionResult result = await controller.Verify2FACode(CreateVerifyModel(), "/return");

        Assert.IsInstanceOf<NotFoundResult>(result);
    }

    [Test]
    public async Task Verify2FACode_Success_RedirectsToLocalUrl()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.TwoFactorSignInAsync("Provider", "123456", false, false))
            .ReturnsAsync(SignInResult.Success);

        var controller = CreateController();

        IActionResult result = await controller.Verify2FACode(CreateVerifyModel(), "/after-2fa");

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/after-2fa", redirect!.Url);
    }

    [Test]
    public async Task Verify2FACode_Success_WithNonLocalReturnUrl_FallsBackToCurrentPage()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.TwoFactorSignInAsync("Provider", "123456", false, false))
            .ReturnsAsync(SignInResult.Success);

        var controller = CreateController();

        IActionResult result = await controller.Verify2FACode(CreateVerifyModel(), "https://evil.com");

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task ValidateAndSaveSetup_Success_WithNonLocalReturnUrl_FallsBackToCurrentPage()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _memberManagerMock.Setup(x => x.GetCurrentMemberAsync()).ReturnsAsync(user);
        _twoFactorLoginServiceMock
            .Setup(x => x.ValidateTwoFactorSetup("Provider", "secret", "123456"))
            .Returns(true);

        var controller = CreateController();

        IActionResult result = await controller.ValidateAndSaveSetup("Provider", "secret", "123456", "https://evil.com");

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task Disable_Success_WithNonLocalReturnUrl_FallsBackToCurrentPage()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _memberManagerMock.Setup(x => x.GetCurrentMemberAsync()).ReturnsAsync(user);
        _twoFactorLoginServiceMock
            .Setup(x => x.DisableAsync(user.Key, "Provider"))
            .ReturnsAsync(true);

        var controller = CreateController();

        IActionResult result = await controller.Disable("Provider", "https://evil.com");

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task Verify2FACode_LockedOut_AddsModelError()
    {
        SetupVerifyFailure(SignInResult.LockedOut);

        var controller = CreateController();

        IActionResult result = await controller.Verify2FACode(CreateVerifyModel());

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState["Code"]!.Errors.Any(e => e.ErrorMessage == "Member is locked out"));
    }

    [Test]
    public async Task Verify2FACode_NotAllowed_AddsModelError()
    {
        SetupVerifyFailure(SignInResult.NotAllowed);

        var controller = CreateController();

        IActionResult result = await controller.Verify2FACode(CreateVerifyModel());

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState["Code"]!.Errors.Any(e => e.ErrorMessage == "Member is not allowed"));
    }

    [Test]
    public async Task Verify2FACode_Failed_AddsInvalidCodeModelError()
    {
        SetupVerifyFailure(SignInResult.Failed);

        var controller = CreateController();

        IActionResult result = await controller.Verify2FACode(CreateVerifyModel());

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState["Code"]!.Errors.Any(e => e.ErrorMessage == "Invalid code"));
    }

    [Test]
    public async Task ValidateAndSaveSetup_InvalidCode_AddsModelError()
    {
        _memberManagerMock
            .Setup(x => x.GetCurrentMemberAsync())
            .ReturnsAsync(new MemberIdentityUser { Key = Guid.NewGuid() });
        _twoFactorLoginServiceMock
            .Setup(x => x.ValidateTwoFactorSetup("Provider", "secret", "000000"))
            .Returns(false);

        var controller = CreateController();

        IActionResult result = await controller.ValidateAndSaveSetup("Provider", "secret", "000000");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState["code"]!.Errors.Any(e => e.ErrorMessage == "Invalid Code"));
        _twoFactorLoginServiceMock.Verify(x => x.SaveAsync(It.IsAny<TwoFactorLogin>()), Times.Never);
    }

    [Test]
    public async Task ValidateAndSaveSetup_NoCurrentMember_AddsModelError()
    {
        _memberManagerMock
            .Setup(x => x.GetCurrentMemberAsync())
            .ReturnsAsync((MemberIdentityUser?)null);
        _twoFactorLoginServiceMock
            .Setup(x => x.ValidateTwoFactorSetup("Provider", "secret", "123456"))
            .Returns(true);

        var controller = CreateController();

        IActionResult result = await controller.ValidateAndSaveSetup("Provider", "secret", "123456");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        _twoFactorLoginServiceMock.Verify(x => x.SaveAsync(It.IsAny<TwoFactorLogin>()), Times.Never);
    }

    [Test]
    public async Task ValidateAndSaveSetup_Success_SavesAndRedirects()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _memberManagerMock.Setup(x => x.GetCurrentMemberAsync()).ReturnsAsync(user);
        _twoFactorLoginServiceMock
            .Setup(x => x.ValidateTwoFactorSetup("Provider", "secret", "123456"))
            .Returns(true);

        var controller = CreateController();

        IActionResult result = await controller.ValidateAndSaveSetup("Provider", "secret", "123456", "/done");

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/done", redirect!.Url);
        _twoFactorLoginServiceMock.Verify(
            x => x.SaveAsync(It.Is<TwoFactorLogin>(t =>
                t.Confirmed &&
                t.Secret == "secret" &&
                t.ProviderName == "Provider" &&
                t.UserOrMemberKey == user.Key)),
            Times.Once);
    }

    [Test]
    public async Task Disable_WhenServiceDisables_RedirectsToReturnUrl()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _memberManagerMock.Setup(x => x.GetCurrentMemberAsync()).ReturnsAsync(user);
        _twoFactorLoginServiceMock
            .Setup(x => x.DisableAsync(user.Key, "Provider"))
            .ReturnsAsync(true);

        var controller = CreateController();

        IActionResult result = await controller.Disable("Provider", "/settings");

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/settings", redirect!.Url);
    }

    [Test]
    public async Task Disable_WhenServiceRefuses_ReturnsCurrentPage()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _memberManagerMock.Setup(x => x.GetCurrentMemberAsync()).ReturnsAsync(user);
        _twoFactorLoginServiceMock
            .Setup(x => x.DisableAsync(user.Key, "Provider"))
            .ReturnsAsync(false);

        var controller = CreateController();

        IActionResult result = await controller.Disable("Provider");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
    }

    [Test]
    public async Task Disable_NoCurrentMember_ReturnsCurrentPage()
    {
        _memberManagerMock.Setup(x => x.GetCurrentMemberAsync()).ReturnsAsync((MemberIdentityUser?)null);

        var controller = CreateController();

        IActionResult result = await controller.Disable("Provider");

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        _twoFactorLoginServiceMock.Verify(x => x.DisableAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    private void SetupVerifyFailure(SignInResult failure)
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync(user);
        _signInManagerMock
            .Setup(x => x.TwoFactorSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(failure);
        _twoFactorLoginServiceMock
            .Setup(x => x.GetEnabledTwoFactorProviderNamesAsync(user.Key))
            .ReturnsAsync(["Provider"]);
    }

    private static Verify2FACodeModel CreateVerifyModel() =>
        new() { Provider = "Provider", Code = "123456" };

    private UmbTwoFactorLoginController CreateController()
    {
        var controller = new UmbTwoFactorLoginController(
            NullLogger<UmbTwoFactorLoginController>.Instance,
            new TestUmbracoContextAccessor(),
            null!,
            ServiceContext.CreatePartial(),
            AppCaches.Disabled,
            Mock.Of<IProfilingLogger>(),
            Mock.Of<IPublishedUrlProvider>(),
            _signInManagerMock.Object,
            _memberManagerMock.Object,
            _twoFactorLoginServiceMock.Object);

        SurfaceControllerTestHelpers.ConfigureControllerContext(controller, isAuthenticated: true);

        return controller;
    }
}
