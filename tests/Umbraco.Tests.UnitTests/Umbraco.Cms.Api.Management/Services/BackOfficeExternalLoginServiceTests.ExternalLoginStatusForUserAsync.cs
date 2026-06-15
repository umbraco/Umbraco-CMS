using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

public partial class BackOfficeExternalLoginServiceTests
{
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

        Assert.That(providersAttempt.Success, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(providersAttempt.Result.Count(p => p.ProviderSchemeName.Equals(firstProviderName)), Is.EqualTo(1));
            Assert.That(providersAttempt.Result.Count(p => p.ProviderSchemeName.Equals(secondProviderName)), Is.EqualTo(1));
            Assert.That(providersAttempt.Result.Count(), Is.EqualTo(2));
        });
    }

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

        Assert.That(providersAttempt.Success, Is.True);
        Assert.That(providersAttempt.Result.Single(p => p.ProviderSchemeName == firstProviderName).IsLinkedOnUser, Is.True);
    }

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

        Assert.That(providersAttempt.Success, Is.True);
        Assert.That(
            providersAttempt.Result.Single(p => p.ProviderSchemeName == providerName).HasManualLinkingEnabled, Is.EqualTo(allowManualLinking));
    }
}
