using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Mail;

[TestFixture]
public class EmailSenderTests
{
    [Test]
    public async Task IsEmailAvailableAsync_Returns_False_When_Nothing_Is_Configured()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(new SmtpSettings { From = "from@test.com" }, clientMock.Object);

        Assert.IsFalse(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Returns_True_When_Smtp_Server_Is_Reachable()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(new SmtpSettings { From = "from@test.com", Host = "smtp.test.com" }, clientMock.Object);

        Assert.IsTrue(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Returns_False_When_Smtp_Server_Is_Unreachable()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SocketException());
        var sender = CreateSender(new SmtpSettings { From = "from@test.com", Host = "smtp.test.com" }, clientMock.Object);

        Assert.IsFalse(await sender.IsEmailAvailableAsync());
    }

    [Test]
    public async Task IsEmailAvailableAsync_Does_Not_Probe_Smtp_When_Pickup_Directory_Is_Used()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        var smtpSettings = new SmtpSettings { From = "from@test.com", Host = "smtp.test.com", PickupDirectoryLocation = "C:\\pickup" };
        var sender = CreateSender(smtpSettings, clientMock.Object);

        Assert.IsTrue(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Probes_Smtp_When_Pickup_Directory_Has_No_From_Address()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SocketException());
        var smtpSettings = new SmtpSettings { From = string.Empty, Host = "smtp.test.com", PickupDirectoryLocation = "C:\\pickup" };
        var sender = CreateSender(smtpSettings, clientMock.Object);

        Assert.IsFalse(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void IsEmailAvailableAsync_Propagates_Cancellation_Requested_By_Caller()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cancellationTokenSource.Token));
        var sender = CreateSender(new SmtpSettings { From = "from@test.com", Host = "smtp.test.com" }, clientMock.Object);

        Assert.ThrowsAsync<OperationCanceledException>(() => sender.IsEmailAvailableAsync(cancellationTokenSource.Token));
    }

    [Test]
    public async Task IsEmailAvailableAsync_Returns_False_When_Probe_Is_Cancelled_Without_Caller_Cancellation()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        var sender = CreateSender(new SmtpSettings { From = "from@test.com", Host = "smtp.test.com" }, clientMock.Object);

        Assert.IsFalse(await sender.IsEmailAvailableAsync(CancellationToken.None));
    }

    [Test]
    public async Task IsEmailAvailableAsync_Does_Not_Probe_Smtp_When_Notification_Handler_Is_Registered()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(
            new SmtpSettings { From = "from@test.com", Host = "smtp.test.com" },
            clientMock.Object,
            Mock.Of<INotificationAsyncHandler<SendEmailNotification>>());

        Assert.IsTrue(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static EmailSender CreateSender(
        SmtpSettings smtpSettings,
        IEmailSenderClient emailSenderClient,
        INotificationAsyncHandler<SendEmailNotification>? handler = null)
        => new(
            NullLogger<EmailSender>.Instance,
            new TestOptionsMonitor<GlobalSettings>(new GlobalSettings { Smtp = smtpSettings }),
            Mock.Of<IEventAggregator>(),
            emailSenderClient,
            null,
            handler);
}
