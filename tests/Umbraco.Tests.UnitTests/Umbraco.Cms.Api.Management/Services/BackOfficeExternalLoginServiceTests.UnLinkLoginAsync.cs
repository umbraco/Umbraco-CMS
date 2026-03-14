using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Unit tests for the BackOfficeExternalLoginService.
/// </summary>
public partial class BackOfficeExternalLoginServiceTests
{
    /// <summary>
    /// Verifies that <c>UnLinkLoginAsync</c> returns the expected <see cref="ExternalLoginOperationStatus"/> for various input scenarios.
    /// </summary>
    /// <param name="expectedResult">The expected status result of the unlink operation.</param>
    /// <param name="claimsPrincipalHasIdentity">Whether the claims principal has an identity.</param>
    /// <param name="backOfficeManagerCanFindUser">Whether the back office manager can find the user.</param>
    /// <param name="authenticationSchemeCanBeFound">Whether the authentication scheme can be found.</param>
    /// <param name="authenticationOptionsCanBeFound">Whether the authentication options can be found.</param>
    /// <param name="manualLinkingIsEnabled">Whether manual linking is enabled.</param>
    /// <param name="userHasMatchingLoginConfigured">Whether the user has a matching login configured.</param>
    /// <param name="removeLoginPasses">Whether the remove login operation succeeds.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestCase(ExternalLoginOperationStatus.Success, true, true, true, true, true, true, true)]
    [TestCase(ExternalLoginOperationStatus.IdentityNotFound, false, true, true, true, true, true, true)]
    [TestCase(ExternalLoginOperationStatus.UserNotFound, true, false, true, true, true, true, true)]
    [TestCase(ExternalLoginOperationStatus.AuthenticationSchemeNotFound, true, true, false, true, true, true, true)]
    [TestCase(ExternalLoginOperationStatus.AuthenticationOptionsNotFound, true, true, true, false, true, true, true)]
    [TestCase(ExternalLoginOperationStatus.UnlinkingDisabled, true, true, true, true, false, true, true)]
    [TestCase(ExternalLoginOperationStatus.InvalidProviderKey, true, true, true, true, true, false, true)]
    [TestCase(ExternalLoginOperationStatus.Unknown, true, true, true, true, true, true, false)]
    public async Task UnLinkLogin_Returns_Correct_Status(
        ExternalLoginOperationStatus expectedResult,
        bool claimsPrincipalHasIdentity,
        bool backOfficeManagerCanFindUser,
        bool authenticationSchemeCanBeFound,
        bool authenticationOptionsCanBeFound,
        bool manualLinkingIsEnabled,
        bool userHasMatchingLoginConfigured,
        bool removeLoginPasses)
    {
        // arrange
        var serviceSetup = new BackOfficeExternalLoginServiceSetup();
        var userId = Guid.NewGuid();
        var loginProviderName = "one";
        var providerKeyPostFix = "provKey";

        var claimsPrinciple = BuildClaimsPrinciple(claimsPrincipalHasIdentity ? userId : null);
        SetupBackOfficeIdentityUser(
            serviceSetup,
            backOfficeManagerCanFindUser ? userId : null,
            userHasMatchingLoginConfigured ? [loginProviderName] : null,
            providerKeyPostFix);
        SetupAuthenticationScheme(serviceSetup, authenticationSchemeCanBeFound ? loginProviderName : null);
        SetupExternalLoginProviderScheme(
            serviceSetup,
            authenticationOptionsCanBeFound ? loginProviderName : null,
            manualLinkingIsEnabled);
        SetupRemoveLoginAsync(serviceSetup, removeLoginPasses);

        var externalLoginService = serviceSetup.Sut;

        // act
        var providersAttempt =
            await externalLoginService.UnLinkLoginAsync(
                claimsPrinciple,
                loginProviderName,
                ProviderKey(loginProviderName, providerKeyPostFix));

        // assert
        Assert.AreEqual(expectedResult, providersAttempt.Result);
    }

    private ClaimsPrincipal BuildClaimsPrinciple(Guid? identityId)
    {
        var identity = new ClaimsIdentity(new ClaimsIdentity(identityId is not null
            ? new[] { new Claim(ClaimTypes.NameIdentifier, identityId.ToString()) }
            : Enumerable.Empty<Claim>()));

        var claimsPrinciple = new ClaimsPrincipal(identity);
        return claimsPrinciple;
    }

    private void SetupBackOfficeIdentityUser(
        BackOfficeExternalLoginServiceSetup serviceSetup,
        Guid? userId,
        string[]? configuredLoginProviderNames,
        string providerKeyNamePostfix) =>
        serviceSetup.BackOfficeUserManager
            .Setup(bum => bum.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userId is null
                ? null
                : () =>
                {
                    var mock = new BackOfficeIdentityUser(
                        Mock.Of<GlobalSettings>(),
                        -1,
                        Enumerable.Empty<IReadOnlyUserGroup>());
                    mock.Key = userId.Value;
                    mock.SetLoginsCallback(new Lazy<IEnumerable<IIdentityUserLogin>?>(() =>
                        configuredLoginProviderNames is null
                            ? Array.Empty<IdentityUserLogin>()
                            : configuredLoginProviderNames.Select(provName =>
                                    new IdentityUserLogin(
                                        provName,
                                        ProviderKey(provName, providerKeyNamePostfix),
                                        userId.ToString()))
                                .ToArray()));

                    return mock;
                });

    private void SetupAuthenticationScheme(BackOfficeExternalLoginServiceSetup serviceSetup, string? authTypeName)
    {
        var authSchemas = new List<AuthenticationScheme>();
        if (authTypeName is not null)
        {
            authSchemas.Add(TestAuthenticationScheme(authTypeName));
        }

        serviceSetup.BackOfficeSignInManager
            .Setup(manager => manager.GetExternalAuthenticationSchemesAsync())
            .ReturnsAsync(authSchemas);
    }

    private void SetupExternalLoginProviderScheme(
        BackOfficeExternalLoginServiceSetup serviceSetup,
        string? authTypeName,
        bool allowManualLinking)
    {
        if (authTypeName is null)
        {
            serviceSetup.BackOfficeLoginProviders
                .Setup(lp => lp.GetAsync(It.IsAny<string>()))
                .ReturnsAsync((BackOfficeExternaLoginProviderScheme?)null);
            return;
        }

        var mock = TestExternalLoginProviderScheme(authTypeName, allowManualLinking);

        serviceSetup.BackOfficeLoginProviders
            .Setup(lp => lp.GetAsync(authTypeName))
            .ReturnsAsync(mock);
    }

    private void SetupRemoveLoginAsync(BackOfficeExternalLoginServiceSetup serviceSetup, bool shouldSucceed) =>
        serviceSetup.BackOfficeUserManager
            .Setup(manager => manager.RemoveLoginAsync(
                It.IsAny<BackOfficeIdentityUser>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(shouldSucceed ? IdentityResult.Success : IdentityResult.Failed());

    private string ProviderKey(string providerName, string providerKeyPostFix) => providerName + providerKeyPostFix;
}
