using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Contains unit tests for the <see cref="BackOfficeExternalLoginService"/> class in the Umbraco CMS API Management Services.
/// </summary>
public partial class BackOfficeExternalLoginServiceTests
{
    /// <summary>
    /// Verifies that <c>ExternalLoginStatusForUserAsync</c> returns all registered external login providers for a given user.
    /// Ensures that the result includes each provider exactly once and that the total count matches the number of registered providers.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExternalLoginStatusForUser_Returns_All_Registered_Providers()
    {
        // arrange
        var userId = Guid.NewGuid();
        var firstProviderName = "one";
        var secondProviderName = "two";
        var serviceSetup = new BackOfficeExternalLoginServiceSetup();
        serviceSetup.BackOfficeLoginProviders
            .Setup(s => s.GetBackOfficeProvidersAsync())
            .ReturnsAsync(new[]
            {
                TestExternalLoginProviderScheme(firstProviderName, true),
                TestExternalLoginProviderScheme(secondProviderName, true)
            });
        serviceSetup.UserService
            .Setup(s => s.GetLinkedLoginsAsync(userId))
            .ReturnsAsync(Attempt.SucceedWithStatus<ICollection<IIdentityUserLogin>, UserOperationStatus>(
                UserOperationStatus.Success, Array.Empty<IIdentityUserLogin>()));

        var externalLoginService = serviceSetup.Sut;

        // act
        var providersAttempt = await externalLoginService.ExternalLoginStatusForUserAsync(userId);

        Assert.True(providersAttempt.Success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, providersAttempt.Result.Count(p => p.ProviderSchemeName.Equals(firstProviderName)));
            Assert.AreEqual(1, providersAttempt.Result.Count(p => p.ProviderSchemeName.Equals(secondProviderName)));
            Assert.AreEqual(2, providersAttempt.Result.Count());
        });
    }

    /// <summary>
    /// Tests that the external login status for a user correctly incorporates linked logins.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task ExternalLoginStatusForUser_Incorporates_Linked_Logins()
    {
        // arrange
        var userId = Guid.NewGuid();
        var firstProviderName = "one";
        var secondProviderName = "two";
        var serviceSetup = new BackOfficeExternalLoginServiceSetup();
        serviceSetup.BackOfficeLoginProviders
            .Setup(s => s.GetBackOfficeProvidersAsync())
            .ReturnsAsync(new[]
            {
                TestExternalLoginProviderScheme(firstProviderName, true),
                TestExternalLoginProviderScheme(secondProviderName, true)
            });
        serviceSetup.UserService
            .Setup(s => s.GetLinkedLoginsAsync(userId))
            .ReturnsAsync(Attempt.SucceedWithStatus<ICollection<IIdentityUserLogin>, UserOperationStatus>(
                UserOperationStatus.Success,
                new[] { new IdentityUserLogin(firstProviderName, firstProviderName + "ProvKey", userId.ToString()) }));

        var externalLoginService = serviceSetup.Sut;

        // act
        var providersAttempt = await externalLoginService.ExternalLoginStatusForUserAsync(userId);

        Assert.True(providersAttempt.Success);
        Assert.IsTrue(providersAttempt.Result.Single(p => p.ProviderSchemeName == firstProviderName).IsLinkedOnUser);
    }

    /// <summary>
    /// Verifies that <c>ExternalLoginStatusForUserAsync</c> correctly reflects the value of <c>HasManualLinkingEnabled</c>
    /// for a user, depending on whether manual linking is allowed for the external login provider.
    /// </summary>
    /// <param name="allowManualLinking">If <c>true</c>, manual linking is enabled for the provider; otherwise, it is disabled.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [TestCase(true)]
    [TestCase(false)]
    public async Task ExternalLoginStatusForUser_Returns_Correct_AllowManualLinking(bool allowManualLinking)
    {
        // arrange
        var userId = Guid.NewGuid();
        var providerName = "one";
        var serviceSetup = new BackOfficeExternalLoginServiceSetup();
        serviceSetup.BackOfficeLoginProviders
            .Setup(s => s.GetBackOfficeProvidersAsync())
            .ReturnsAsync(new[] { TestExternalLoginProviderScheme(providerName, allowManualLinking), });
        serviceSetup.UserService
            .Setup(s => s.GetLinkedLoginsAsync(userId))
            .ReturnsAsync(Attempt.SucceedWithStatus<ICollection<IIdentityUserLogin>, UserOperationStatus>(
                UserOperationStatus.Success,
                new[] { new IdentityUserLogin(providerName, providerName + "ProvKey", userId.ToString()) }));

        var externalLoginService = serviceSetup.Sut;

        // act
        var providersAttempt = await externalLoginService.ExternalLoginStatusForUserAsync(userId);

        Assert.True(providersAttempt.Success);
        Assert.AreEqual(
            allowManualLinking,
            providersAttempt.Result.Single(p => p.ProviderSchemeName == providerName).HasManualLinkingEnabled);
    }
}
