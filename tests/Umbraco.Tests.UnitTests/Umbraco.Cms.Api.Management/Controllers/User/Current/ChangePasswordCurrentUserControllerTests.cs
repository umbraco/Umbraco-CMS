using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.User.Current;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.User.Current;

[TestFixture]
public class ChangePasswordCurrentUserControllerTests
{
    private Mock<IBackOfficeUserManager> _userManager = null!;
    private Mock<IBackOfficeSignInManager> _signInManager = null!;
    private Mock<IAuthenticationService> _authenticationService = null!;
    private ClaimsPrincipal _cookiePrincipal = null!;

    [SetUp]
    public void SetUp()
    {
        _userManager = new Mock<IBackOfficeUserManager>();
        _signInManager = new Mock<IBackOfficeSignInManager>();
        _authenticationService = new Mock<IAuthenticationService>();
        _cookiePrincipal = new ClaimsPrincipal(new ClaimsIdentity("test"));
    }

    [Test]
    public async Task Does_Not_Reissue_The_Sign_In_When_The_Request_Is_Not_Cookie_Authenticated()
    {
        _authenticationService
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), Constants.Security.BackOfficeAuthenticationType))
            .ReturnsAsync(AuthenticateResult.NoResult());

        IActionResult result = await CreateSut().ChangePassword(CancellationToken.None, new ChangePasswordCurrentUserRequestModel { NewPassword = "0987654321" });

        Assert.That(result, Is.InstanceOf<OkResult>());
        _signInManager.Verify(x => x.ReissueSignInAsync(It.IsAny<BackOfficeIdentityUser>()), Times.Never);
    }

    [Test]
    public async Task Reissues_The_Sign_In_Of_The_Cookie_Principal_When_The_Request_Is_Cookie_Authenticated()
    {
        var identityUser = new BackOfficeIdentityUser(new GlobalSettings(), 1, []);
        _authenticationService
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), Constants.Security.BackOfficeAuthenticationType))
            .ReturnsAsync(AuthenticateResult.Success(
                new AuthenticationTicket(_cookiePrincipal, Constants.Security.BackOfficeAuthenticationType)));
        _userManager
            .Setup(x => x.GetUserAsync(_cookiePrincipal))
            .ReturnsAsync(identityUser);

        IActionResult result = await CreateSut().ChangePassword(CancellationToken.None, new ChangePasswordCurrentUserRequestModel { NewPassword = "0987654321" });

        Assert.That(result, Is.InstanceOf<OkResult>());
        _signInManager.Verify(x => x.ReissueSignInAsync(identityUser), Times.Once);
    }

    private ChangePasswordCurrentUserController CreateSut()
    {
        var user = new Mock<IUser>();
        user.SetupGet(x => x.Key).Returns(Guid.NewGuid());
        var backOfficeSecurity = new Mock<IBackOfficeSecurity>();
        backOfficeSecurity.SetupGet(x => x.CurrentUser).Returns(user.Object);
        var accessor = new Mock<IBackOfficeSecurityAccessor>();
        accessor.SetupGet(x => x.BackOfficeSecurity).Returns(backOfficeSecurity.Object);

        var userService = new Mock<IUserService>();
        userService
            .Setup(x => x.ChangePasswordAsync(It.IsAny<Guid>(), It.IsAny<ChangeUserPasswordModel>()))
            .ReturnsAsync(Attempt.SucceedWithStatus(UserOperationStatus.Success, new PasswordChangedModel()));

        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection()
                .AddSingleton(_authenticationService.Object)
                .BuildServiceProvider(),
        };

        return new ChangePasswordCurrentUserController(
            accessor.Object,
            userService.Object,
            _userManager.Object,
            _signInManager.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
        };
    }
}
