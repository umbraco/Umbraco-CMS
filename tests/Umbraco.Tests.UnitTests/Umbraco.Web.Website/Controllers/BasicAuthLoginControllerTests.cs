using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website.Controllers;

[TestFixture]
public class BasicAuthLoginControllerTests
{
    private Mock<IBackOfficeSignInManager> _signInManagerMock = null!;
    private Mock<ITwoFactorLoginService> _twoFactorServiceMock = null!;
    private BasicAuthLoginController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _signInManagerMock = new Mock<IBackOfficeSignInManager>();
        _twoFactorServiceMock = new Mock<ITwoFactorLoginService>();

        var services = new ServiceCollection();
        services.AddSingleton(_signInManagerMock.Object);
        services.AddSingleton(_twoFactorServiceMock.Object);
        services.AddAntiforgery();
        services.AddLogging();
        services.AddControllersWithViews();
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

        _controller = new BasicAuthLoginController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            },

            // UrlHelper needs an action context to resolve Url.IsLocalUrl.
            Url = new UrlHelper(new ActionContext(httpContext, new RouteData(), new ActionDescriptor())),
        };
    }

    /// <summary>
    /// Verifies that GET Login returns the login view with the return path set.
    /// </summary>
    [Test]
    public void Login_Get_ReturnsView()
    {
        IActionResult result = _controller.Login("/some-page");

        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);
        Assert.That(viewResult!.ViewName, Is.EqualTo("/umbraco/BasicAuthLogin/Login.cshtml"));

        var model = viewResult.Model as BasicAuthLoginModel;
        Assert.IsNotNull(model);
        Assert.That(model!.ReturnPath, Is.EqualTo("/some-page"));
        Assert.IsNull(model.ErrorMessage);
    }

    /// <summary>
    /// Verifies that GET Login accepts a null return path without error.
    /// </summary>
    [Test]
    public void Login_Get_NullReturnPath_ReturnsView()
    {
        IActionResult result = _controller.Login(null);

        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);

        var model = viewResult!.Model as BasicAuthLoginModel;
        Assert.IsNotNull(model);
        Assert.IsNull(model!.ReturnPath);
    }

    /// <summary>
    /// Verifies that successful login redirects to the provided local return path.
    /// </summary>
    [Test]
    public async Task Login_Post_Success_RedirectsToReturnPath()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "pass", false, true))
            .ReturnsAsync(SignInResult.Success);

        IActionResult result = await _controller.Login("admin", "pass", "/protected-page");

        var redirectResult = result as LocalRedirectResult;
        Assert.IsNotNull(redirectResult);
        Assert.That(redirectResult!.Url, Is.EqualTo("/protected-page"));
    }

    /// <summary>
    /// Verifies that successful login with no return path redirects to the site root.
    /// </summary>
    [Test]
    public async Task Login_Post_Success_NullReturnPath_RedirectsToRoot()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "pass", false, true))
            .ReturnsAsync(SignInResult.Success);

        IActionResult result = await _controller.Login("admin", "pass", null);

        var redirectResult = result as RedirectResult;
        Assert.IsNotNull(redirectResult);
        Assert.That(redirectResult!.Url, Is.EqualTo("/"));
    }

    /// <summary>
    /// Verifies that invalid credentials return the login view with an error message.
    /// </summary>
    [Test]
    public async Task Login_Post_InvalidCredentials_ReturnsLoginViewWithError()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "wrong", false, true))
            .ReturnsAsync(SignInResult.Failed);

        IActionResult result = await _controller.Login("admin", "wrong", "/page");

        AssertLoginViewWithError(result, "Invalid username or password.");
    }

    /// <summary>
    /// Verifies that a locked-out account returns the login view with a lockout message.
    /// </summary>
    [Test]
    public async Task Login_Post_LockedOut_ReturnsLoginViewWithError()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "pass", false, true))
            .ReturnsAsync(SignInResult.LockedOut);

        IActionResult result = await _controller.Login("admin", "pass", "/page");

        AssertLoginViewWithError(result, "Your account has been locked out. Please try again later.");
    }

    /// <summary>
    /// Verifies that a disallowed account returns the login view with an appropriate message.
    /// </summary>
    [Test]
    public async Task Login_Post_NotAllowed_ReturnsLoginViewWithError()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "pass", false, true))
            .ReturnsAsync(SignInResult.NotAllowed);

        IActionResult result = await _controller.Login("admin", "pass", "/page");

        AssertLoginViewWithError(result, "Your account is not allowed to sign in.");
    }

    /// <summary>
    /// Verifies that a RequiresTwoFactor result redirects to the TwoFactor action with the return path.
    /// </summary>
    [Test]
    public async Task Login_Post_RequiresTwoFactor_RedirectsToTwoFactorAction()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "pass", false, true))
            .ReturnsAsync(SignInResult.TwoFactorRequired);

        IActionResult result = await _controller.Login("admin", "pass", "/page");

        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.That(redirectResult!.ActionName, Is.EqualTo("TwoFactor"));
        Assert.That(redirectResult.RouteValues!["returnPath"], Is.EqualTo("/page"));
    }

    /// <summary>
    /// Verifies that an empty username returns the login view with a validation error
    /// and does not call the sign-in manager.
    /// </summary>
    [Test]
    public async Task Login_Post_EmptyUsername_ReturnsLoginViewWithError()
    {
        IActionResult result = await _controller.Login(string.Empty, "pass", "/page");

        AssertLoginViewWithError(result, "Please enter a username and password.");
        _signInManagerMock.Verify(
            x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that an empty password returns the login view with a validation error.
    /// </summary>
    [Test]
    public async Task Login_Post_EmptyPassword_ReturnsLoginViewWithError()
    {
        IActionResult result = await _controller.Login("admin", string.Empty, "/page");

        AssertLoginViewWithError(result, "Please enter a username and password.");
    }

    /// <summary>
    /// Verifies that a non-local return path (open redirect attempt) redirects to the site root instead.
    /// </summary>
    [Test]
    public async Task Login_Post_NonLocalReturnPath_RedirectsToRoot()
    {
        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync("admin", "pass", false, true))
            .ReturnsAsync(SignInResult.Success);

        IActionResult result = await _controller.Login("admin", "pass", "https://evil.com");

        var redirectResult = result as RedirectResult;
        Assert.IsNotNull(redirectResult);
        Assert.That(redirectResult!.Url, Is.EqualTo("/"));
    }

    /// <summary>
    /// Verifies that when IBackOfficeSignInManager is not registered, the login view shows
    /// an appropriate configuration error.
    /// </summary>
    [Test]
    public async Task Login_Post_NoSignInManager_ReturnsLoginViewWithError()
    {
        // Create controller without IBackOfficeSignInManager registered
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllersWithViews();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

        var controller = new BasicAuthLoginController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
        };

        IActionResult result = await controller.Login("admin", "pass", "/page");

        AssertLoginViewWithError(result, "Backoffice sign-in is not available. Ensure AddBackOfficeSignIn() is called at startup.");
    }

    /// <summary>
    /// Verifies that GET TwoFactor returns the 2FA view with the user's enabled provider names.
    /// </summary>
    [Test]
    public async Task TwoFactor_Get_ReturnsViewWithProviders()
    {
        var user = CreateBackOfficeUser();
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync(user);
        _twoFactorServiceMock
            .Setup(x => x.GetEnabledTwoFactorProviderNamesAsync(user.Key))
            .ReturnsAsync(["UmbracoUserAppAuthenticator"]);

        IActionResult result = await _controller.TwoFactor("/page");

        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);
        Assert.That(viewResult!.ViewName, Is.EqualTo("/umbraco/BasicAuthLogin/TwoFactor.cshtml"));

        var model = viewResult.Model as BasicAuthTwoFactorModel;
        Assert.IsNotNull(model);
        Assert.That(model!.ReturnPath, Is.EqualTo("/page"));
        Assert.IsNull(model.ErrorMessage);
        Assert.That(model.ProviderNames, Is.EquivalentTo(new[] { "UmbracoUserAppAuthenticator" }));
    }

    /// <summary>
    /// Verifies that GET TwoFactor redirects to login when no user is in the 2FA flow
    /// (e.g. session expired).
    /// </summary>
    [Test]
    public async Task TwoFactor_Get_NoUserInFlow_RedirectsToLogin()
    {
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync((BackOfficeIdentityUser?)null);

        IActionResult result = await _controller.TwoFactor("/page");

        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.That(redirectResult!.ActionName, Is.EqualTo("Login"));
        Assert.That(redirectResult.RouteValues!["returnPath"], Is.EqualTo("/page"));
    }

    /// <summary>
    /// Verifies that a valid 2FA code redirects to the provided local return path.
    /// </summary>
    [Test]
    public async Task TwoFactor_Post_Success_RedirectsToReturnPath()
    {
        _signInManagerMock
            .Setup(x => x.TwoFactorSignInAsync("UmbracoUserAppAuthenticator", "123456", false, false))
            .ReturnsAsync(SignInResult.Success);

        IActionResult result = await _controller.TwoFactor("UmbracoUserAppAuthenticator", "123456", "/page");

        var redirectResult = result as LocalRedirectResult;
        Assert.IsNotNull(redirectResult);
        Assert.That(redirectResult!.Url, Is.EqualTo("/page"));
    }

    /// <summary>
    /// Verifies that an invalid 2FA code returns the 2FA view with an error message
    /// and preserves the user's provider list.
    /// </summary>
    [Test]
    public async Task TwoFactor_Post_InvalidCode_ReturnsTwoFactorViewWithError()
    {
        var user = CreateBackOfficeUser();
        _signInManagerMock
            .Setup(x => x.TwoFactorSignInAsync("UmbracoUserAppAuthenticator", "000000", false, false))
            .ReturnsAsync(SignInResult.Failed);
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync(user);
        _twoFactorServiceMock
            .Setup(x => x.GetEnabledTwoFactorProviderNamesAsync(user.Key))
            .ReturnsAsync(["UmbracoUserAppAuthenticator"]);

        IActionResult result = await _controller.TwoFactor("UmbracoUserAppAuthenticator", "000000", "/page");

        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);
        Assert.That(viewResult!.ViewName, Is.EqualTo("/umbraco/BasicAuthLogin/TwoFactor.cshtml"));

        var model = viewResult.Model as BasicAuthTwoFactorModel;
        Assert.IsNotNull(model);
        Assert.That(model!.ErrorMessage, Is.EqualTo("Invalid verification code. Please try again."));
    }

    /// <summary>
    /// Verifies that a locked-out account during 2FA returns the login view with a lockout message.
    /// </summary>
    [Test]
    public async Task TwoFactor_Post_LockedOut_ReturnsLoginViewWithError()
    {
        _signInManagerMock
            .Setup(x => x.TwoFactorSignInAsync("UmbracoUserAppAuthenticator", "000000", false, false))
            .ReturnsAsync(SignInResult.LockedOut);

        IActionResult result = await _controller.TwoFactor("UmbracoUserAppAuthenticator", "000000", "/page");

        AssertLoginViewWithError(result, "Your account has been locked out. Please try again later.");
    }

    /// <summary>
    /// Verifies that an empty verification code returns the 2FA view with a validation error.
    /// </summary>
    [Test]
    public async Task TwoFactor_Post_EmptyCode_ReturnsTwoFactorViewWithError()
    {
        var user = CreateBackOfficeUser();
        _signInManagerMock
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync(user);
        _twoFactorServiceMock
            .Setup(x => x.GetEnabledTwoFactorProviderNamesAsync(user.Key))
            .ReturnsAsync(["UmbracoUserAppAuthenticator"]);

        IActionResult result = await _controller.TwoFactor("UmbracoUserAppAuthenticator", string.Empty, "/page");

        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);

        var model = viewResult!.Model as BasicAuthTwoFactorModel;
        Assert.IsNotNull(model);
        Assert.That(model!.ErrorMessage, Is.EqualTo("Please enter a verification code."));
    }

    /// <summary>
    /// Verifies that a non-local return path after 2FA success redirects to the site root
    /// to prevent open redirect attacks.
    /// </summary>
    [Test]
    public async Task TwoFactor_Post_NonLocalReturnPath_RedirectsToRoot()
    {
        _signInManagerMock
            .Setup(x => x.TwoFactorSignInAsync("UmbracoUserAppAuthenticator", "123456", false, false))
            .ReturnsAsync(SignInResult.Success);

        IActionResult result = await _controller.TwoFactor("UmbracoUserAppAuthenticator", "123456", "https://evil.com");

        var redirectResult = result as RedirectResult;
        Assert.IsNotNull(redirectResult);
        Assert.That(redirectResult.Url, Is.EqualTo("/"));
    }

    private static void AssertLoginViewWithError(IActionResult result, string expectedError)
    {
        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);
        Assert.That(viewResult.ViewName, Is.EqualTo("/umbraco/BasicAuthLogin/Login.cshtml"));

        var model = viewResult.Model as BasicAuthLoginModel;
        Assert.IsNotNull(model);
        Assert.That(model.ErrorMessage, Is.EqualTo(expectedError));
    }

    private static BackOfficeIdentityUser CreateBackOfficeUser()
    {
        var globalSettings = new global::Umbraco.Cms.Core.Configuration.Models.GlobalSettings();
        return BackOfficeIdentityUser.CreateNew(globalSettings, "admin", "admin@example.com", "en-US");
    }
}
