using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

public partial class BackOfficeExternalLoginServiceTests
{
    private const int CallbackUserId = 1234;
    private const string CurrentSecurityStamp = "current-stamp";

    [Test]
    public async Task HandleLoginCallback_Links_External_Login_From_Link_Flow_Started_For_The_User()
    {
        var serviceSetup = new BackOfficeExternalLoginServiceSetup();
        BackOfficeIdentityUser user = SetupCallback(serviceSetup, CurrentSecurityStamp, linkFlowUserId: CallbackUserId.ToString());
        serviceSetup.BackOfficeUserManager
            .Setup(m => m.AddLoginAsync(user, It.IsAny<UserLoginInfo>()))
            .ReturnsAsync(IdentityResult.Success);
        serviceSetup.BackOfficeSignInManager
            .Setup(m => m.UpdateExternalAuthenticationTokensAsync(It.IsAny<ExternalLoginInfo>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await serviceSetup.Sut.HandleLoginCallbackAsync(CreateHttpContext(CurrentSecurityStamp));

        Assert.AreEqual(ExternalLoginOperationStatus.Success, result.Status);
        serviceSetup.BackOfficeUserManager.Verify(m => m.AddLoginAsync(user, It.IsAny<UserLoginInfo>()), Times.Once);
    }

    [Test]
    public async Task HandleLoginCallback_Does_Not_Link_External_Login_From_Link_Flow_Started_For_Another_User()
    {
        var serviceSetup = new BackOfficeExternalLoginServiceSetup();
        SetupCallback(serviceSetup, CurrentSecurityStamp, linkFlowUserId: "5678");

        var result = await serviceSetup.Sut.HandleLoginCallbackAsync(CreateHttpContext(CurrentSecurityStamp));

        Assert.AreEqual(ExternalLoginOperationStatus.ExternalInfoNotFound, result.Status);
        serviceSetup.BackOfficeUserManager.Verify(m => m.AddLoginAsync(It.IsAny<BackOfficeIdentityUser>(), It.IsAny<UserLoginInfo>()), Times.Never);
    }

    [Test]
    public async Task HandleLoginCallback_Does_Not_Link_External_Login_For_Cookie_With_Outdated_Security_Stamp()
    {
        var serviceSetup = new BackOfficeExternalLoginServiceSetup();
        SetupCallback(serviceSetup, CurrentSecurityStamp, linkFlowUserId: CallbackUserId.ToString());

        var result = await serviceSetup.Sut.HandleLoginCallbackAsync(CreateHttpContext("outdated-stamp"));

        Assert.AreEqual(ExternalLoginOperationStatus.Unauthorized, result.Status);
        serviceSetup.BackOfficeUserManager.Verify(m => m.AddLoginAsync(It.IsAny<BackOfficeIdentityUser>(), It.IsAny<UserLoginInfo>()), Times.Never);
    }

    /// <summary>
    /// Sets up the cookie user and the external login info, the latter mirroring the sign-in manager: the info is
    /// returned when no XSRF value is expected, or when the expected value is the user the link flow was started for.
    /// </summary>
    private static BackOfficeIdentityUser SetupCallback(
        BackOfficeExternalLoginServiceSetup serviceSetup,
        string userSecurityStamp,
        string linkFlowUserId)
    {
        var user = new BackOfficeIdentityUser(Mock.Of<GlobalSettings>(), CallbackUserId, Enumerable.Empty<IReadOnlyUserGroup>())
        {
            SecurityStamp = userSecurityStamp,
        };
        serviceSetup.BackOfficeUserManager
            .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var info = new ExternalLoginInfo(new ClaimsPrincipal(new ClaimsIdentity()), "provider", "provider-key", "Provider");
        serviceSetup.BackOfficeSignInManager
            .Setup(m => m.GetExternalLoginInfoAsync(It.IsAny<string?>()))
            .ReturnsAsync((string? expectedXsrf) => expectedXsrf is null || expectedXsrf == linkFlowUserId ? info : null);

        return user;
    }

    private static HttpContext CreateHttpContext(string cookieSecurityStamp)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, CallbackUserId.ToString()),
                new Claim(Constants.Security.SecurityStampClaimType, cookieSecurityStamp),
            ],
            Constants.Security.BackOfficeAuthenticationType));
        var ticket = new AuthenticationTicket(principal, Constants.Security.BackOfficeAuthenticationType);

        var authenticationService = new Mock<IAuthenticationService>();
        authenticationService
            .Setup(m => m.AuthenticateAsync(It.IsAny<HttpContext>(), Constants.Security.BackOfficeAuthenticationType))
            .ReturnsAsync(AuthenticateResult.Success(ticket));

        return new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddSingleton(authenticationService.Object).BuildServiceProvider(),
        };
    }
}
