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
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.ActionResults;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

[TestFixture]
public class UmbRegisterControllerTests
{
    private Mock<IMemberManager> _memberManagerMock = null!;
    private Mock<IMemberService> _memberServiceMock = null!;
    private Mock<IMemberSignInManager> _signInManagerMock = null!;
    private Mock<ICoreScopeProvider> _scopeProviderMock = null!;

    [SetUp]
    public void SetUp()
    {
        _memberManagerMock = new Mock<IMemberManager>();
        _memberServiceMock = new Mock<IMemberService>();
        _signInManagerMock = new Mock<IMemberSignInManager>();
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
    public async Task HandleRegisterMember_InvalidModelState_ReturnsCurrentPage()
    {
        var controller = CreateController();
        controller.ModelState.AddModelError("registerModel", "invalid");

        IActionResult result = await controller.HandleRegisterMember(new RegisterModel());

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        _memberManagerMock.Verify(
            x => x.CreateAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task HandleRegisterMember_Success_WithLocalRedirectUrl_RedirectsAndSetsSuccessTempData()
    {
        SetupSuccessfulMemberCreation();

        var controller = CreateController();

        IActionResult result = await controller.HandleRegisterMember(new RegisterModel
        {
            Email = "new@example.com",
            Password = "Password1",
            Name = "New Member",
            AutomaticLogIn = false,
            RedirectUrl = "/welcome",
        });

        var redirect = result as RedirectResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("/welcome", redirect!.Url);
        Assert.AreEqual(true, controller.TempData["FormSuccess"]);
    }

    [Test]
    public async Task HandleRegisterMember_Success_NoRedirectUrl_RedirectsToCurrentUmbracoPage()
    {
        SetupSuccessfulMemberCreation();

        var controller = CreateController();

        IActionResult result = await controller.HandleRegisterMember(new RegisterModel
        {
            Email = "new@example.com",
            Password = "Password1",
            Name = "New Member",
            AutomaticLogIn = false,
        });

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task HandleRegisterMember_Success_WithNonLocalRedirectUrl_FallsBackToCurrentPage()
    {
        SetupSuccessfulMemberCreation();

        var controller = CreateController();

        IActionResult result = await controller.HandleRegisterMember(new RegisterModel
        {
            Email = "new@example.com",
            Password = "Password1",
            Name = "New Member",
            AutomaticLogIn = false,
            RedirectUrl = "https://evil.com/phish",
        });

        Assert.IsInstanceOf<RedirectToUmbracoPageResult>(result);
    }

    [Test]
    public async Task HandleRegisterMember_Success_WithAutomaticLogIn_SignsInTheNewMember()
    {
        SetupSuccessfulMemberCreation();

        var controller = CreateController();

        await controller.HandleRegisterMember(new RegisterModel
        {
            Email = "new@example.com",
            Password = "Password1",
            Name = "New Member",
            AutomaticLogIn = true,
        });

        _signInManagerMock.Verify(x => x.SignInAsync(It.IsAny<MemberIdentityUser>(), false, null), Times.Once);
    }

    [Test]
    public async Task HandleRegisterMember_Success_WithoutAutomaticLogIn_DoesNotSignIn()
    {
        SetupSuccessfulMemberCreation();

        var controller = CreateController();

        await controller.HandleRegisterMember(new RegisterModel
        {
            Email = "new@example.com",
            Password = "Password1",
            Name = "New Member",
            AutomaticLogIn = false,
        });

        _signInManagerMock.Verify(x => x.SignInAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<bool>(), It.IsAny<string?>()), Times.Never);
    }

    [Test]
    public async Task HandleRegisterMember_IdentityError_AddsModelError()
    {
        _memberManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too short" }));

        var controller = CreateController();

        IActionResult result = await controller.HandleRegisterMember(new RegisterModel
        {
            Email = "new@example.com",
            Password = "x",
            Name = "New Member",
        });

        Assert.IsInstanceOf<UmbracoPageResult>(result);
        Assert.IsTrue(controller.ModelState["registerModel"]!.Errors
            .Any(e => e.ErrorMessage == "Password too short"));
        _memberServiceMock.Verify(x => x.Save(It.IsAny<IMember>(), It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task HandleRegisterMember_UsesEmailAsName_WhenNameIsEmpty()
    {
        SetupSuccessfulMemberCreation();

        MemberIdentityUser? createdUser = null;
        _memberManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>()))
            .Callback<MemberIdentityUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController();

        await controller.HandleRegisterMember(new RegisterModel
        {
            Email = "fallback@example.com",
            Password = "Password1",
            Name = string.Empty,
        });

        Assert.IsNotNull(createdUser);
        Assert.AreEqual("fallback@example.com", createdUser!.Name);
    }

    [Test]
    public async Task HandleRegisterMember_RegistersWithConfiguredMemberType()
    {
        SetupSuccessfulMemberCreation();

        MemberIdentityUser? createdUser = null;
        _memberManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>()))
            .Callback<MemberIdentityUser, string>((user, _) => createdUser = user)
            .ReturnsAsync(IdentityResult.Success);

        var controller = CreateController();

        await controller.HandleRegisterMember(new RegisterModel
        {
            Email = "alias@example.com",
            Password = "Password1",
            Name = "Alias Member",
            MemberTypeAlias = "customMemberType",
        });

        Assert.AreEqual("customMemberType", createdUser!.MemberTypeAlias);
    }

    private void SetupSuccessfulMemberCreation()
    {
        _memberManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<MemberIdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var member = new Mock<IMember>();
        member.Setup(x => x.Properties).Returns(new PropertyCollection());
        _memberServiceMock.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(member.Object);
    }

    private UmbRegisterController CreateController()
    {
        var controller = new UmbRegisterController(
            _memberManagerMock.Object,
            _memberServiceMock.Object,
            new TestUmbracoContextAccessor(),
            null!,
            ServiceContext.CreatePartial(),
            AppCaches.Disabled,
            Mock.Of<IProfilingLogger>(),
            Mock.Of<IPublishedUrlProvider>(),
            _signInManagerMock.Object,
            _scopeProviderMock.Object);

        SurfaceControllerTestHelpers.ConfigureControllerContext(controller);

        return controller;
    }
}
