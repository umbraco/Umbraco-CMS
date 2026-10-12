using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

[TestFixture]
public class EmailUserForgotPasswordSenderTests
{
    [Test]
    public async Task IsPasswordResetAvailableAsync_Returns_False_Without_Checking_Email_When_Password_Reset_Is_Not_Allowed()
    {
        var emailSenderMock = new Mock<IEmailSender>();
        emailSenderMock.Setup(x => x.IsEmailAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var sender = CreateSender(allowPasswordReset: false, emailSenderMock.Object);

        Assert.IsFalse(await sender.IsPasswordResetAvailableAsync());
        emailSenderMock.Verify(x => x.IsEmailAvailableAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task IsPasswordResetAvailableAsync_Returns_Email_Availability_When_Password_Reset_Is_Allowed(bool isEmailAvailable)
    {
        var emailSenderMock = new Mock<IEmailSender>();
        emailSenderMock.Setup(x => x.IsEmailAvailableAsync(It.IsAny<CancellationToken>())).ReturnsAsync(isEmailAvailable);
        var sender = CreateSender(allowPasswordReset: true, emailSenderMock.Object);

        Assert.AreEqual(isEmailAvailable, await sender.IsPasswordResetAvailableAsync());
    }

    private static EmailUserForgotPasswordSender CreateSender(bool allowPasswordReset, IEmailSender emailSender)
        => new(
            emailSender,
            Mock.Of<ILocalizedTextService>(),
            new TestOptionsMonitor<GlobalSettings>(new GlobalSettings()),
            new TestOptionsMonitor<SecuritySettings>(new SecuritySettings { AllowPasswordReset = allowPasswordReset }));
}
