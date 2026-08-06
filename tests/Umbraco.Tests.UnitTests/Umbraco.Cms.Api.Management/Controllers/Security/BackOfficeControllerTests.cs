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
using Umbraco.Cms.Api.Management.Controllers.Security;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.Security;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Security;

[TestFixture]
public class BackOfficeControllerTests
{
    private Mock<IBackOfficeSignInManager> _signInManager = null!;

    [SetUp]
    public void SetUp() => _signInManager = new Mock<IBackOfficeSignInManager>();

    // A pending second factor is not a failed sign-in. Reporting it as "failed" tells the client the
    // login is over when the two-factor cookie is sitting there waiting to be redeemed at verify-2fa.
    [Test]
    public async Task Can_Report_Two_Factor_Required_Distinctly_From_External_Login_Callback()
    {
        RedirectResult result = await ExternalLoginCallback(IdentitySignInResult.TwoFactorRequired);

        Assert.That(result.Url, Does.Contain("status=two-factor-required"));
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
            Mock.Of<IBackOfficeTwoFactorOptions>(),
            Mock.Of<IUserTwoFactorLoginService>(),
            Mock.Of<IBackOfficeExternalLoginService>(),
            Mock.Of<IBackOfficeUserClientCredentialsManager>());
}
