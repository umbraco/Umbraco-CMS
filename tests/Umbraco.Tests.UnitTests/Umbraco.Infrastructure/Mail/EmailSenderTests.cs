using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Mail;

[TestFixture]
public class EmailSenderTests
{
    private static readonly SmtpSettings _smtpServerSettings = new() { From = "from@test.com", Host = "smtp.test.com" };

    private static readonly SmtpSettings _sendInBackgroundSettings = new() { From = "from@test.com", Host = "smtp.test.com", SendInBackground = true };

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
    public async Task IsEmailAvailableAsync_Does_Not_Probe_Smtp_Fallback_When_Notification_Handler_Is_Registered()
    {
        var clientMock = CreateUnreachableClientMock();
        var sender = CreateSender(
            _smtpServerSettings,
            clientMock.Object,
            handler: Mock.Of<INotificationAsyncHandler<SendEmailNotification>>());

        Assert.IsTrue(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Never);
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
    public async Task IsEmailAvailableAsync_Probe_Superseded_By_Settings_Change_Does_Not_Set_Cache_Expiry()
    {
        Action<GlobalSettings, string?>? onChange = null;
        var optionsMonitorMock = new Mock<IOptionsMonitor<GlobalSettings>>();
        optionsMonitorMock.SetupGet(x => x.CurrentValue).Returns(new GlobalSettings { Smtp = _smtpServerSettings });
        optionsMonitorMock
            .Setup(x => x.OnChange(It.IsAny<Action<GlobalSettings, string?>>()))
            .Callback<Action<GlobalSettings, string?>>(listener => onChange = listener);
        var supersededProbe = new TaskCompletionSource();
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .SetupSequence(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()))
            .Returns(supersededProbe.Task)
            .ThrowsAsync(new SocketException())
            .Returns(Task.CompletedTask);
        var timeProvider = new FakeTimeProvider();
        var sender = CreateSender(optionsMonitorMock.Object, clientMock.Object, timeProvider: timeProvider);

        Task<bool> supersededResult = sender.IsEmailAvailableAsync();
        onChange!(new GlobalSettings { Smtp = new SmtpSettings { From = "from@test.com", Host = "other.test.com" } }, null);
        Assert.IsFalse(await sender.IsEmailAvailableAsync());
        supersededProbe.SetResult();
        Assert.IsTrue(await supersededResult);

        timeProvider.Advance(EmailSender.SmtpProbeUnavailableCacheDuration);
        Assert.IsTrue(await sender.IsEmailAvailableAsync());
        clientMock.Verify(x => x.VerifyConnectionAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
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

    [Test]
    public async Task SendAsync_Queues_Email_Instead_Of_Sending_When_SendInBackground_Is_Enabled()
    {
        var queueMock = CreateQueueMock(accepts: true);
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(_sendInBackgroundSettings, clientMock.Object, emailDeliveryQueue: queueMock.Object);
        var message = CreateMessage();
        var expires = TimeSpan.FromHours(1);

        await sender.SendAsync(message, "Test", expires: expires);

        queueMock.Verify(x => x.TryEnqueueAsync(message, "Test", expires, It.IsAny<CancellationToken>()), Times.Once);
        clientMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Test]
    public async Task SendAsync_Sends_Immediately_When_The_Queue_Cannot_Accept_The_Email()
    {
        var queueMock = CreateQueueMock(accepts: false);
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(_sendInBackgroundSettings, clientMock.Object, emailDeliveryQueue: queueMock.Object);
        var message = CreateMessage();
        var expires = TimeSpan.FromHours(1);

        await sender.SendAsync(message, "Test", expires: expires);

        queueMock.Verify(x => x.TryEnqueueAsync(message, "Test", expires, It.IsAny<CancellationToken>()), Times.Once);
        clientMock.Verify(x => x.SendAsync(message, expires), Times.Once);
    }

    [Test]
    public void SendAsync_Reports_Send_Failure_To_Caller_When_SendInBackground_Is_Disabled()
    {
        var queueMock = new Mock<IEmailDeliveryQueue>();
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<TimeSpan?>()))
            .ThrowsAsync(new SocketException());
        var sender = CreateSender(_smtpServerSettings, clientMock.Object, emailDeliveryQueue: queueMock.Object);

        Assert.ThrowsAsync<SocketException>(async () => await sender.SendAsync(CreateMessage(), "Test"));
        queueMock.Verify(
            x => x.TryEnqueueAsync(It.IsAny<EmailMessage>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task SendAsync_Does_Not_Queue_Email_Handled_By_A_Notification_Handler_When_SendInBackground_Is_Enabled()
    {
        var eventAggregatorMock = new Mock<IEventAggregator>();
        eventAggregatorMock
            .Setup(x => x.PublishAsync(It.IsAny<SendEmailNotification>(), It.IsAny<CancellationToken>()))
            .Callback((SendEmailNotification notification, CancellationToken _) => notification.HandleEmail())
            .Returns(Task.CompletedTask);
        var queueMock = new Mock<IEmailDeliveryQueue>();
        var sender = CreateSender(
            _sendInBackgroundSettings,
            Mock.Of<IEmailSenderClient>(),
            emailDeliveryQueue: queueMock.Object,
            eventAggregator: eventAggregatorMock.Object);

        await sender.SendAsync(CreateMessage(), "Test", enableNotification: true);

        queueMock.Verify(
            x => x.TryEnqueueAsync(It.IsAny<EmailMessage>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static EmailMessage CreateMessage() => new("from@test.com", "to@test.com", "Subject", "Body", false);

    private static Mock<IEmailDeliveryQueue> CreateQueueMock(bool accepts)
    {
        var queueMock = new Mock<IEmailDeliveryQueue>();
        queueMock
            .Setup(x => x.TryEnqueueAsync(It.IsAny<EmailMessage>(), It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(accepts);
        return queueMock;
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
        TimeProvider? timeProvider = null,
        IEmailDeliveryQueue? emailDeliveryQueue = null,
        IEventAggregator? eventAggregator = null)
        => CreateSender(
            new TestOptionsMonitor<GlobalSettings>(new GlobalSettings { Smtp = smtpSettings }),
            emailSenderClient,
            handler,
            timeProvider,
            emailDeliveryQueue,
            eventAggregator);

    private static EmailSender CreateSender(
        IOptionsMonitor<GlobalSettings> globalSettings,
        IEmailSenderClient emailSenderClient,
        INotificationAsyncHandler<SendEmailNotification>? handler = null,
        TimeProvider? timeProvider = null,
        IEmailDeliveryQueue? emailDeliveryQueue = null,
        IEventAggregator? eventAggregator = null)
        => new(
            NullLogger<EmailSender>.Instance,
            globalSettings,
            eventAggregator ?? Mock.Of<IEventAggregator>(),
            emailSenderClient,
            emailDeliveryQueue ?? Mock.Of<IEmailDeliveryQueue>(),
            handler is not null,
            timeProvider ?? TimeProvider.System);
}
