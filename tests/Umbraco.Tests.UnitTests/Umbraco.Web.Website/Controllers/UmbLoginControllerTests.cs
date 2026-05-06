// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.UnitTests.TestHelpers.Objects;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.ActionResults;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

[TestFixture]
public class UmbLoginControllerTests
{
    private Mock<IMemberSignInManager> _signInManagerMock = null!;
    private Mock<IMemberManager> _memberManagerMock = null!;
    private Mock<ITwoFactorLoginService> _twoFactorLoginServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _signInManagerMock = new Mock<IMemberSignInManager>();
        _memberManagerMock = new Mock<IMemberManager>();
        _twoFactorLoginServiceMock = new Mock<ITwoFactorLoginService>();
    }

    [Test]
    public async Task HandleLogin_InvalidModelState_ReturnsCurrentPage()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("loginModel", "invalid");

        IActionResult result = await controller.HandleLogin(new LoginModel());

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        _signInManagerMock.Verify(
            x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Test]
    public async Task HandleLogin_Success_WithLocalRedirectUrl_RedirectsAndSetsSuccessTempData()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("user", "pass", false, true))
            .ReturnsAsync(SignInResult.Success);

        var controller = CreateController();

        IActionResult result = await controller.HandleLogin(new LoginModel
        {
            Username = "user",
            Password = "pass",
            RedirectUrl = "/after-login",
        });

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/after-login", redirect!.Url);
        Assert.AreEqual(true, controller.TempData["LoginSuccess"]);
    }

    [Test]
    public async Task HandleLogin_Success_NoRedirectUrl_RedirectsToCurrentUmbracoUrl()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("user", "pass", false, true))
            .ReturnsAsync(SignInResult.Success);

        var controller = CreateController();

        IActionResult result = await controller.HandleLogin(new LoginModel { Username = "user", Password = "pass" });

        Assert.IsInstanceOf<RedirectToUmbracoUrlResult>(result);
        Assert.AreEqual(true, controller.TempData["LoginSuccess"]);
    }

    [Test]
    public async Task HandleLogin_Success_WithNonLocalRedirectUrl_FallsBackToAncestorUrl()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("user", "pass", false, true))
            .ReturnsAsync(SignInResult.Success);

        var currentPage = new Mock<IPublishedContent>();
        currentPage.Setup(x => x.Level).Returns(1);
        currentPage.Setup(x => x.Id).Returns(1234);
        currentPage.Setup(x => x.ContentType).Returns(Mock.Of<IPublishedContentType>(t => t.ItemType == PublishedItemType.Content));

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(x => x.GetUrl(currentPage.Object, UrlMode.Default, null, null))
            .Returns("/root-page");

        var controller = CreateController(currentPage.Object, publishedUrlProvider.Object);

        IActionResult result = await controller.HandleLogin(new LoginModel
        {
            Username = "user",
            Password = "pass",
            RedirectUrl = "https://evil.com",
        });

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/root-page", redirect!.Url);
    }

    [Test]
    public async Task HandleLogin_RedirectUrlFromRouteData_OverridesModelValue()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("user", "pass", false, true))
            .ReturnsAsync(SignInResult.Success);

        var controller = CreateController();
        controller.RouteData.Values[nameof(LoginModel.RedirectUrl)] = "/from-route";

        IActionResult result = await controller.HandleLogin(new LoginModel
        {
            Username = "user",
            Password = "pass",
            RedirectUrl = "/from-model",
        });

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/from-route", redirect!.Url);
    }

    [Test]
    public async Task HandleLogin_TwoFactorRequired_WithKnownUser_PopulatesProviderNames()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid(), UserName = "user" };
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("user", "pass", false, true))
            .ReturnsAsync(SignInResult.TwoFactorRequired);
        _memberManagerMock.Setup(x => x.FindByNameAsync("user")).ReturnsAsync(user);
        _twoFactorLoginServiceMock
            .Setup(x => x.GetEnabledTwoFactorProviderNamesAsync(user.Key))
            .ReturnsAsync(["UmbracoMemberAppAuthenticator"]);

        var controller = CreateController();

        IActionResult result = await controller.HandleLogin(new LoginModel { Username = "user", Password = "pass" });

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ViewData.TryGetTwoFactorProviderNames(out var providerNames));
        Assert.That(providerNames, Is.EquivalentTo(new[] { "UmbracoMemberAppAuthenticator" }));
    }

    [Test]
    public async Task HandleLogin_TwoFactorRequired_WithUnknownUser_ReturnsBadRequest()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("user", "pass", false, true))
            .ReturnsAsync(SignInResult.TwoFactorRequired);
        _memberManagerMock.Setup(x => x.FindByNameAsync("user")).ReturnsAsync((MemberIdentityUser?)null);

        var controller = CreateController();

        IActionResult result = await controller.HandleLogin(new LoginModel { Username = "user", Password = "pass" });

        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task HandleLogin_LockedOut_AddsModelError()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("user", "pass", false, true))
            .ReturnsAsync(SignInResult.LockedOut);

        var controller = CreateController();

        IActionResult result = await controller.HandleLogin(new LoginModel { Username = "user", Password = "pass" });

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState["loginModel"]!.Errors.Any(e => e.ErrorMessage == "Member is locked out"));
    }

    [Test]
    public async Task HandleLogin_NotAllowed_AddsModelError()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("user", "pass", false, true))
            .ReturnsAsync(SignInResult.NotAllowed);

        var controller = CreateController();

        IActionResult result = await controller.HandleLogin(new LoginModel { Username = "user", Password = "pass" });

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState["loginModel"]!.Errors.Any(e => e.ErrorMessage == "Member is not allowed"));
    }

    [Test]
    public async Task HandleLogin_Failed_AddsInvalidCredentialsModelError()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("user", "wrong", false, true))
            .ReturnsAsync(SignInResult.Failed);

        var controller = CreateController();

        IActionResult result = await controller.HandleLogin(new LoginModel { Username = "user", Password = "wrong" });

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState["loginModel"]!.Errors
            .Any(e => e.ErrorMessage == "Invalid username or password"));
    }

    private UmbLoginController CreateController(
        IPublishedContent? currentPage = null,
        IPublishedUrlProvider? publishedUrlProvider = null)
    {
        var umbracoContextAccessor = new TestUmbracoContextAccessor();
        TestUmbracoContextFactory.Create(umbracoContextAccessor).EnsureUmbracoContext();

        var controller = new UmbLoginController(
            umbracoContextAccessor,
            null!,
            ServiceContext.CreatePartial(),
            AppCaches.Disabled,
            Mock.Of<IProfilingLogger>(),
            publishedUrlProvider ?? Mock.Of<IPublishedUrlProvider>(),
            _signInManagerMock.Object,
            _memberManagerMock.Object,
            _twoFactorLoginServiceMock.Object,
            Mock.Of<IDocumentNavigationQueryService>(),
            Mock.Of<IPublishedContentStatusFilteringService>());

        SurfaceControllerTestHelpers.ConfigureControllerContext(controller, currentPage: currentPage);

        return controller;
    }
}
