using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
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
    private static readonly SmtpSettings _smtpServerSettings = new() { From = "from@test.com", Host = "smtp.test.com" };

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
        var sender = CreateSender(_smtpServerSettings, clientMock.Object);

        Assert.IsTrue(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Returns_False_When_Smtp_Server_Is_Unreachable()
    {
        var clientMock = CreateUnreachableClientMock();
        var sender = CreateSender(_smtpServerSettings, clientMock.Object);

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
        var clientMock = CreateUnreachableClientMock();
        var smtpSettings = new SmtpSettings { From = string.Empty, Host = "smtp.test.com", PickupDirectoryLocation = "C:\\pickup" };
        var sender = CreateSender(smtpSettings, clientMock.Object);

        Assert.IsFalse(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Returns_True_Without_Probing_When_Only_A_Notification_Handler_Is_Registered()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(
            new SmtpSettings { From = "from@test.com" },
            clientMock.Object,
            handler: Mock.Of<INotificationAsyncHandler<SendEmailNotification>>());

        Assert.IsTrue(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Probes_Smtp_Fallback_When_Notification_Handler_Is_Registered()
    {
        var clientMock = CreateUnreachableClientMock();
        var sender = CreateSender(
            _smtpServerSettings,
            clientMock.Object,
            handler: Mock.Of<INotificationAsyncHandler<SendEmailNotification>>());

        Assert.IsFalse(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Shares_A_Single_Probe_Between_Concurrent_Callers()
    {
        var probeCompletion = new TaskCompletionSource();
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()))
            .Returns(probeCompletion.Task);
        var sender = CreateSender(_smtpServerSettings, clientMock.Object);

        Task<bool> first = sender.IsEmailAvailableAsync();
        Task<bool> second = sender.IsEmailAvailableAsync();
        probeCompletion.SetResult();

        Assert.IsTrue(await first);
        Assert.IsTrue(await second);
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Caches_Available_Result_Until_It_Expires()
    {
        var timeProvider = new FakeTimeProvider();
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(_smtpServerSettings, clientMock.Object, timeProvider: timeProvider);

        await sender.IsEmailAvailableAsync();
        timeProvider.Advance(EmailSender.SmtpProbeAvailableCacheDuration - TimeSpan.FromSeconds(1));
        await sender.IsEmailAvailableAsync();
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);

        timeProvider.Advance(TimeSpan.FromSeconds(1));
        await sender.IsEmailAvailableAsync();
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task IsEmailAvailableAsync_Caches_Unavailable_Result_For_A_Shorter_Period()
    {
        var timeProvider = new FakeTimeProvider();
        var clientMock = CreateUnreachableClientMock();
        var sender = CreateSender(_smtpServerSettings, clientMock.Object, timeProvider: timeProvider);

        Assert.IsFalse(await sender.IsEmailAvailableAsync());
        timeProvider.Advance(EmailSender.SmtpProbeUnavailableCacheDuration - TimeSpan.FromSeconds(1));
        Assert.IsFalse(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);

        clientMock.Reset();
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        Assert.IsTrue(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Probes_Again_When_Settings_Change()
    {
        Action<GlobalSettings, string?>? onChange = null;
        var settings = new GlobalSettings { Smtp = _smtpServerSettings };
        var optionsMonitorMock = new Mock<IOptionsMonitor<GlobalSettings>>();
        optionsMonitorMock.SetupGet(x => x.CurrentValue).Returns(settings);
        optionsMonitorMock
            .Setup(x => x.OnChange(It.IsAny<Action<GlobalSettings, string?>>()))
            .Callback<Action<GlobalSettings, string?>>(listener => onChange = listener);
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(optionsMonitorMock.Object, clientMock.Object);

        await sender.IsEmailAvailableAsync();
        onChange!(new GlobalSettings { Smtp = new SmtpSettings { From = "from@test.com", Host = "other.test.com" } }, null);
        await sender.IsEmailAvailableAsync();

        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task IsEmailAvailableAsync_Returns_False_When_Probe_Times_Out()
    {
        var timeProvider = new FakeTimeProvider();
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken cancellationToken) => Task.Delay(Timeout.Infinite, cancellationToken));
        var sender = CreateSender(_smtpServerSettings, clientMock.Object, timeProvider: timeProvider);

        Task<bool> result = sender.IsEmailAvailableAsync();
        timeProvider.Advance(EmailSender.SmtpProbeTimeout);

        Assert.IsFalse(await result);
    }

    [Test]
    public async Task IsEmailAvailableAsync_Caller_Cancellation_Stops_Waiting_Without_Cancelling_Shared_Probe()
    {
        var probeCompletion = new TaskCompletionSource();
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()))
            .Returns(probeCompletion.Task);
        var sender = CreateSender(_smtpServerSettings, clientMock.Object);
        using var cancellationTokenSource = new CancellationTokenSource();

        Task<bool> cancelledCaller = sender.IsEmailAvailableAsync(cancellationTokenSource.Token);
        Task<bool> otherCaller = sender.IsEmailAvailableAsync();
        cancellationTokenSource.Cancel();

        Assert.CatchAsync<OperationCanceledException>(async () => await cancelledCaller);
        probeCompletion.SetResult();
        Assert.IsTrue(await otherCaller);
    }

    private static Mock<IEmailSenderClient> CreateUnreachableClientMock()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SocketException());
        return clientMock;
    }

    private static EmailSender CreateSender(
        SmtpSettings smtpSettings,
        IEmailSenderClient emailSenderClient,
        INotificationAsyncHandler<SendEmailNotification>? handler = null,
        TimeProvider? timeProvider = null)
        => CreateSender(
            new TestOptionsMonitor<GlobalSettings>(new GlobalSettings { Smtp = smtpSettings }),
            emailSenderClient,
            handler,
            timeProvider);

    private static EmailSender CreateSender(
        IOptionsMonitor<GlobalSettings> globalSettings,
        IEmailSenderClient emailSenderClient,
        INotificationAsyncHandler<SendEmailNotification>? handler = null,
        TimeProvider? timeProvider = null)
        => new(
            NullLogger<EmailSender>.Instance,
            globalSettings,
            Mock.Of<IEventAggregator>(),
            emailSenderClient,
            null,
            handler,
            timeProvider ?? TimeProvider.System);
}
