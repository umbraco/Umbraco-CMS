// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management;
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Security;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.Security;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Security;

[TestFixture]
public class BackOfficeControllerTests
{
    private Mock<IBackOfficeSignInManager> _signInManager = null!;
    private Mock<IBackOfficeTwoFactorOptions> _twoFactorOptions = null!;
    private Mock<IUserTwoFactorLoginService> _twoFactorLoginService = null!;

    [SetUp]
    public void SetUp()
    {
        _signInManager = new Mock<IBackOfficeSignInManager>();
        _twoFactorOptions = new Mock<IBackOfficeTwoFactorOptions>();
        _twoFactorLoginService = new Mock<IUserTwoFactorLoginService>();
    }

    // A pending second factor is not a failed sign-in. Reporting it as "failed" would tell the client
    // the login is over when the two-factor cookie is sitting there waiting to be redeemed - instead
    // the callback must send the browser on to complete it at the login app's MFA screen.
    [Test]
    public async Task Redirects_To_Login_Mfa_On_Two_Factor_Required_From_External_Login_Callback()
    {
        RedirectResult result = await ExternalLoginCallback(IdentitySignInResult.TwoFactorRequired);

        Assert.Multiple(() =>
        {
            Assert.That(result.Url, Does.StartWith(BackOfficeLoginController.LoginPath));
            Assert.That(result.Url, Does.Contain("flow=mfa"));
            Assert.That(result.Url, Does.Contain("ReturnUrl="));
        });
    }

    [Test]
    public async Task PendingTwoFactorInfo_Returns_NotFound_When_No_Pending_Two_Factor_Sign_In()
    {
        _signInManager
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync((BackOfficeIdentityUser?)null);

        IActionResult result = await CreateSut().PendingTwoFactorInfo();

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task PendingTwoFactorInfo_Returns_Provider_Options_When_A_Two_Factor_Sign_In_Is_Pending()
    {
        BackOfficeIdentityUser user = BackOfficeIdentityUser.CreateNew(new GlobalSettings(), "test@example.com", "test@example.com", "en-US", "Test User");

        _signInManager
            .Setup(x => x.GetTwoFactorAuthenticationUserAsync())
            .ReturnsAsync(user);
        _twoFactorOptions
            .Setup(x => x.GetTwoFactorView(user.UserName!))
            .Returns("my-custom-view");
        _twoFactorLoginService
            .Setup(x => x.GetProviderNamesAsync(user.Key))
            .ReturnsAsync(Attempt.SucceedWithStatus(
                TwoFactorOperationStatus.Success,
                (IEnumerable<UserTwoFactorProviderModel>)new[] { new UserTwoFactorProviderModel("email", true) }));

        IActionResult result = await CreateSut().PendingTwoFactorInfo();

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var model = (RequiresTwoFactorResponseModel)((OkObjectResult)result).Value!;
        Assert.Multiple(() =>
        {
            Assert.That(model.TwoFactorLoginView, Is.EqualTo("my-custom-view"));
            Assert.That(model.EnabledTwoFactorProviderNames, Is.EquivalentTo(new[] { "email" }));
        });
    }

    [TestCaseSource(nameof(FailureCases))]
    public async Task Can_Report_External_Login_Callback_Failure_Status(IdentitySignInResult signInResult, string expectedStatus)
    {
        RedirectResult result = await ExternalLoginCallback(signInResult);

        Assert.That(result.Url, Does.Contain($"status={expectedStatus}"));
    }

    [Test]
    public async Task Can_Report_External_Login_Callback_Flow_On_Every_Outcome()
    {
        RedirectResult result = await ExternalLoginCallback(IdentitySignInResult.Failed);

        Assert.That(result.Url, Does.Contain("flow=external-login"));
    }

    private static IEnumerable<TestCaseData> FailureCases()
    {
        yield return new TestCaseData(IdentitySignInResult.LockedOut, "locked-out").SetName("{m}(LockedOut)");
        yield return new TestCaseData(IdentitySignInResult.NotAllowed, "not-allowed").SetName("{m}(NotAllowed)");
        yield return new TestCaseData(IdentitySignInResult.Failed, "failed").SetName("{m}(Failed)");
    }

    private async Task<RedirectResult> ExternalLoginCallback(IdentitySignInResult signInResult)
    {
        var loginInfo = new ExternalLoginInfo(
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "provider-key")])),
            loginProvider: "TestProvider",
            providerKey: "provider-key",
            displayName: "Test Provider");

        _signInManager
            .Setup(x => x.GetExternalLoginInfoAsync(It.IsAny<string?>()))
            .ReturnsAsync(loginInfo);
        _signInManager
            .Setup(x => x.ExternalLoginSignInAsync(It.IsAny<ExternalLoginInfo>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(signInResult);

        IActionResult result = await CreateSut().ExternalLoginCallback();

        Assert.That(result, Is.InstanceOf<RedirectResult>(), $"Expected a redirect, got {result.GetType().Name}");
        return (RedirectResult)result;
    }

    private BackOfficeController CreateSut() =>
        new(
            Mock.Of<IHttpContextAccessor>(),
            _signInManager.Object,
            Mock.Of<IBackOfficeUserManager>(),
            Options.Create(new SecuritySettings()),
            Mock.Of<ILogger<BackOfficeController>>(),
            _twoFactorOptions.Object,
            _twoFactorLoginService.Object,
            Mock.Of<IBackOfficeExternalLoginService>(),
            Mock.Of<IBackOfficeUserClientCredentialsManager>());
}
