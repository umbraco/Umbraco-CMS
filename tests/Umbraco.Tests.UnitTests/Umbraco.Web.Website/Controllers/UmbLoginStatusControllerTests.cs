// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.ActionResults;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

[TestFixture]
public class UmbLoginStatusControllerTests
{
    private Mock<IMemberSignInManager> _signInManagerMock = null!;

    [SetUp]
    public void SetUp() => _signInManagerMock = new Mock<IMemberSignInManager>();

    [Test]
    public async Task HandleLogout_InvalidModelState_ReturnsCurrentPage()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("logoutModel", "invalid");

        IActionResult result = await controller.HandleLogout(new PostRedirectModel());

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Never);
    }

    [Test]
    public async Task HandleLogout_WhenAuthenticated_SignsOutAndRedirectsToCurrentPage()
    {
        var controller = CreateController(isAuthenticated: true);

        IActionResult result = await controller.HandleLogout(new PostRedirectModel());

        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
        Assert.AreEqual(true, controller.TempData["LogoutSuccess"]);
    }

    [Test]
    public async Task HandleLogout_WhenNotAuthenticated_DoesNotSignOut()
    {
        var controller = CreateController(isAuthenticated: false);

        IActionResult result = await controller.HandleLogout(new PostRedirectModel());

        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Never);
        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
        Assert.AreEqual(true, controller.TempData["LogoutSuccess"]);
    }

    [Test]
    public async Task HandleLogout_WithLocalRedirectUrl_RedirectsToThatUrl()
    {
        var controller = CreateController(isAuthenticated: true);

        IActionResult result = await controller.HandleLogout(new PostRedirectModel { RedirectUrl = "/goodbye" });

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/goodbye", redirect!.Url);
    }

    [Test]
    public async Task HandleLogout_WithNonLocalRedirectUrl_RedirectsToCurrentPage()
    {
        var controller = CreateController(isAuthenticated: true);

        IActionResult result = await controller.HandleLogout(new PostRedirectModel { RedirectUrl = "https://evil.com" });

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task HandleLogout_RedirectUrlFromRouteData_OverridesModelValue()
    {
        var controller = CreateController(isAuthenticated: true);
        controller.RouteData.Values[nameof(PostRedirectModel.RedirectUrl)] = "/from-route";

        IActionResult result = await controller.HandleLogout(new PostRedirectModel { RedirectUrl = "/from-model" });

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/from-route", redirect!.Url);
    }

    private UmbLoginStatusController CreateController(bool isAuthenticated = false)
    {
        var controller = new UmbLoginStatusController(
            new TestUmbracoContextAccessor(),
            null!,
            ServiceContext.CreatePartial(),
            AppCaches.Disabled,
            Mock.Of<IProfilingLogger>(),
            Mock.Of<IPublishedUrlProvider>(),
            _signInManagerMock.Object);

        SurfaceControllerTestHelpers.ConfigureControllerContext(controller, isAuthenticated);

        return controller;
    }
}
