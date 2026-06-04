// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Website.ActionResults;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

[TestFixture]
public class UmbProfileControllerTests
{
    private Mock<IMemberManager> _memberManagerMock = null!;
    private Mock<IMemberService> _memberServiceMock = null!;
    private Mock<IMemberTypeService> _memberTypeServiceMock = null!;
    private Mock<ICoreScopeProvider> _scopeProviderMock = null!;

    [SetUp]
    public void SetUp()
    {
        _memberManagerMock = new Mock<IMemberManager>();
        _memberServiceMock = new Mock<IMemberService>();
        _memberTypeServiceMock = new Mock<IMemberTypeService>();
        _scopeProviderMock = new Mock<ICoreScopeProvider>();
        _scopeProviderMock
            .Setup(x => x.CreateCoreScope(
                It.IsAny<System.Data.IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<global::Umbraco.Cms.Core.Events.IEventDispatcher>(),
                It.IsAny<global::Umbraco.Cms.Core.Events.IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());
    }

    [Test]
    public async Task HandleUpdateProfile_InvalidModelState_ReturnsCurrentPage()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("profileModel", "invalid");

        IActionResult result = await controller.HandleUpdateProfile(CreateModel());

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        _memberManagerMock.Verify(x => x.UpdateAsync(It.IsAny<MemberIdentityUser>()), Times.Never);
    }

    [Test]
    public async Task HandleUpdateProfile_NoCurrentMember_RedirectsToCurrentPage()
    {
        _memberManagerMock
            .Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((MemberIdentityUser?)null);

        var controller = CreateController();

        IActionResult result = await controller.HandleUpdateProfile(CreateModel());

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
        _memberManagerMock.Verify(x => x.UpdateAsync(It.IsAny<MemberIdentityUser>()), Times.Never);
    }

    [Test]
    public async Task HandleUpdateProfile_UpdateFails_AddsModelErrors()
    {
        var user = new MemberIdentityUser { Key = Guid.NewGuid() };
        _memberManagerMock
            .Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _memberManagerMock
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid email" }));

        var controller = CreateController();

        IActionResult result = await controller.HandleUpdateProfile(CreateModel());

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState["profileModel"]!.Errors.Any(e => e.ErrorMessage == "Invalid email"));
    }

    [Test]
    public async Task HandleUpdateProfile_Success_NoRedirectUrl_RedirectsToCurrentPage()
    {
        SetupSuccessfulUpdate(out _);

        var controller = CreateController();

        IActionResult result = await controller.HandleUpdateProfile(CreateModel());

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
        Assert.AreEqual(true, controller.TempData["FormSuccess"]);
    }

    [Test]
    public async Task HandleUpdateProfile_Success_WithLocalRedirectUrl_RedirectsToUrl()
    {
        SetupSuccessfulUpdate(out _);

        var controller = CreateController();

        IActionResult result = await controller.HandleUpdateProfile(CreateModel(redirectUrl: "/profile-saved"));

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/profile-saved", redirect!.Url);
    }

    [Test]
    public async Task HandleUpdateProfile_Success_WithNonLocalRedirectUrl_FallsBackToCurrentPage()
    {
        SetupSuccessfulUpdate(out _);

        var controller = CreateController();

        IActionResult result = await controller.HandleUpdateProfile(CreateModel(redirectUrl: "https://evil.com"));

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task HandleUpdateProfile_Success_CopiesEditableFieldsToIdentityUser()
    {
        SetupSuccessfulUpdate(out var user);

        var controller = CreateController();

        await controller.HandleUpdateProfile(new ProfileModel
        {
            Name = "Updated Name",
            Email = "updated@example.com",
            UserName = "updated-username",
            Comments = "Updated comments",
        });

        Assert.AreEqual("Updated Name", user.Name);
        Assert.AreEqual("updated@example.com", user.Email);
        Assert.AreEqual("updated-username", user.UserName);
        Assert.AreEqual("Updated comments", user.Comments);
    }

    private static ProfileModel CreateModel(string? redirectUrl = null) =>
        new()
        {
            Email = "member@example.com",
            Name = "Member",
            UserName = "member",
            RedirectUrl = redirectUrl,
        };

    private void SetupSuccessfulUpdate(out MemberIdentityUser user)
    {
        user = new MemberIdentityUser
        {
            Key = Guid.NewGuid(),
            Email = "old@example.com",
            Name = "Old",
            UserName = "old",
        };
        _memberManagerMock
            .Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _memberManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<MemberIdentityUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var member = new Mock<IMember>();
        member.Setup(x => x.ContentTypeId).Returns(1234);
        member.Setup(x => x.Properties).Returns(new PropertyCollection());
        _memberServiceMock.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(member.Object);

        var memberType = new Mock<IMemberType>();
        _memberTypeServiceMock.Setup(x => x.Get(It.IsAny<int>())).Returns(memberType.Object);
    }

    private UmbProfileController CreateController()
    {
        var controller = new UmbProfileController(
            new TestUmbracoContextAccessor(),
            null!,
            ServiceContext.CreatePartial(),
            AppCaches.Disabled,
            Mock.Of<IProfilingLogger>(),
            Mock.Of<IPublishedUrlProvider>(),
            _memberManagerMock.Object,
            _memberServiceMock.Object,
            _memberTypeServiceMock.Object,
            _scopeProviderMock.Object);

        SurfaceControllerTestHelpers.ConfigureControllerContext(controller, isAuthenticated: true);

        return controller;
    }
}
