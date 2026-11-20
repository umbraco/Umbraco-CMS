using System.Net.Sockets;
using Microsoft.Extensions.Logging;
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
    public async Task SendAsync_Does_Not_Publish_Notification_When_Caller_Does_Not_Request_It()
    {
        var eventAggregatorMock = new Mock<IEventAggregator>();
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(_smtpServerSettings, clientMock.Object, eventAggregator: eventAggregatorMock.Object);

        await sender.SendAsync(CreateMessage(), "Test", enableNotification: false);

        eventAggregatorMock.Verify(
            x => x.PublishAsync(It.IsAny<SendEmailNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
        clientMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Test]
    public async Task SendAsync_Publishes_Notification_For_Every_Email_When_PublishNotificationForAllEmails_Is_Enabled()
    {
        var publishedNotifications = new List<SendEmailNotification>();
        var eventAggregatorMock = CreateEventAggregatorMock(notification =>
        {
            publishedNotifications.Add(notification);
            notification.HandleEmail();
            return Task.CompletedTask;
        });
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(CreatePublishForAllEmailsSettings(), clientMock.Object, eventAggregator: eventAggregatorMock.Object);

        await sender.SendAsync(CreateMessage(), "Test", enableNotification: false);

        Assert.AreEqual(1, publishedNotifications.Count);
        Assert.AreEqual("Test", publishedNotifications[0].EmailType);
        clientMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Test]
    public async Task SendAsync_Sends_Through_Transport_When_Published_Notification_Is_Not_Handled()
    {
        var eventAggregatorMock = CreateEventAggregatorMock(_ => Task.CompletedTask);
        var clientMock = new Mock<IEmailSenderClient>();
        var sender = CreateSender(CreatePublishForAllEmailsSettings(), clientMock.Object, eventAggregator: eventAggregatorMock.Object);

        await sender.SendAsync(CreateMessage(), "Test", enableNotification: false);

        clientMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Test]
    public async Task SendAsync_Does_Not_Publish_Notification_Again_When_Called_From_A_SendEmailNotification_Handler()
    {
        EmailSender? sender = null;
        var publishCount = 0;
        var eventAggregatorMock = CreateEventAggregatorMock(async notification =>
        {
            // Only the first publish sends on, so that a missing guard fails the assertion rather than recursing forever.
            if (++publishCount == 1)
            {
                await sender!.SendAsync(CreateMessage(), notification.EmailType, enableNotification: false);
                notification.HandleEmail();
            }
        });
        var clientMock = new Mock<IEmailSenderClient>();
        sender = CreateSender(CreatePublishForAllEmailsSettings(), clientMock.Object, eventAggregator: eventAggregatorMock.Object);

        await sender.SendAsync(CreateMessage(), "Test", enableNotification: false);

        Assert.AreEqual(1, publishCount);
        clientMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Test]
    public async Task SendAsync_Publishes_Notification_Requested_By_A_Send_From_A_SendEmailNotification_Handler()
    {
        EmailSender? sender = null;
        var publishCount = 0;
        var eventAggregatorMock = CreateEventAggregatorMock(async notification =>
        {
            // The first publish sends a further email requesting the notification; the second publish handles it.
            if (++publishCount == 1)
            {
                await sender!.SendAsync(CreateMessage(), notification.EmailType, enableNotification: true);
            }

            notification.HandleEmail();
        });
        var clientMock = new Mock<IEmailSenderClient>();
        sender = CreateSender(_smtpServerSettings, clientMock.Object, eventAggregator: eventAggregatorMock.Object);

        await sender.SendAsync(CreateMessage(), "Test", enableNotification: true);

        Assert.AreEqual(2, publishCount);
        clientMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Test]
    public async Task SendAsync_Honours_PublishNotificationForAllEmails_Changed_At_Runtime()
    {
        Action<GlobalSettings, string?>? onChange = null;
        var optionsMonitorMock = new Mock<IOptionsMonitor<GlobalSettings>>();
        optionsMonitorMock.SetupGet(x => x.CurrentValue).Returns(new GlobalSettings { Smtp = _smtpServerSettings });
        optionsMonitorMock
            .Setup(x => x.OnChange(It.IsAny<Action<GlobalSettings, string?>>()))
            .Callback<Action<GlobalSettings, string?>>(listener => onChange = listener);
        var publishCount = 0;
        var eventAggregatorMock = CreateEventAggregatorMock(_ =>
        {
            publishCount++;
            return Task.CompletedTask;
        });
        var sender = CreateSender(optionsMonitorMock.Object, Mock.Of<IEmailSenderClient>(), eventAggregator: eventAggregatorMock.Object);

        await sender.SendAsync(CreateMessage(), "Test", enableNotification: false);
        onChange!(new GlobalSettings { Smtp = CreatePublishForAllEmailsSettings() }, null);
        await sender.SendAsync(CreateMessage(), "Test", enableNotification: false);

        Assert.AreEqual(1, publishCount);
    }

    [Test]
    public async Task SendAsync_Logs_Warning_When_Email_Is_Dropped_Without_Being_Published_To_A_Registered_Handler()
    {
        var loggerMock = new Mock<ILogger<EmailSender>>();
        var sender = CreateSender(
            new SmtpSettings { From = "from@test.com" },
            Mock.Of<IEmailSenderClient>(),
            handler: Mock.Of<INotificationAsyncHandler<SendEmailNotification>>(),
            logger: loggerMock.Object);

        await sender.SendAsync(CreateMessage(), "Test", enableNotification: false);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task SendAsync_Logs_Warning_When_Email_Sent_On_From_A_SendEmailNotification_Handler_Is_Dropped()
    {
        EmailSender? sender = null;
        var loggerMock = new Mock<ILogger<EmailSender>>();
        var eventAggregatorMock = CreateEventAggregatorMock(async notification =>
        {
            await sender!.SendAsync(CreateMessage(), notification.EmailType, enableNotification: false);
            notification.HandleEmail();
        });
#pragma warning disable CS0618 // Type or member is obsolete
        var settings = new SmtpSettings { From = "from@test.com", PublishNotificationForAllEmails = true };
#pragma warning restore CS0618 // Type or member is obsolete
        sender = CreateSender(
            settings,
            Mock.Of<IEmailSenderClient>(),
            handler: Mock.Of<INotificationAsyncHandler<SendEmailNotification>>(),
            eventAggregator: eventAggregatorMock.Object,
            logger: loggerMock.Object);

        await sender.SendAsync(CreateMessage(), "Test", enableNotification: false);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static EmailMessage CreateMessage() => new("from@test.com", "to@test.com", "Subject", "Body", false);

#pragma warning disable CS0618 // Type or member is obsolete
    private static SmtpSettings CreatePublishForAllEmailsSettings()
        => new() { From = "from@test.com", Host = "smtp.test.com", PublishNotificationForAllEmails = true };
#pragma warning restore CS0618 // Type or member is obsolete

    private static Mock<IEventAggregator> CreateEventAggregatorMock(Func<SendEmailNotification, Task> onPublish)
    {
        var eventAggregatorMock = new Mock<IEventAggregator>();
        eventAggregatorMock
            .Setup(x => x.PublishAsync(It.IsAny<SendEmailNotification>(), It.IsAny<CancellationToken>()))
            .Returns((SendEmailNotification notification, CancellationToken _) => onPublish(notification));
        return eventAggregatorMock;
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
        IEventAggregator? eventAggregator = null,
        ILogger<EmailSender>? logger = null)
        => CreateSender(
            new TestOptionsMonitor<GlobalSettings>(new GlobalSettings { Smtp = smtpSettings }),
            emailSenderClient,
            handler,
            timeProvider,
            eventAggregator,
            logger);

    private static EmailSender CreateSender(
        IOptionsMonitor<GlobalSettings> globalSettings,
        IEmailSenderClient emailSenderClient,
        INotificationAsyncHandler<SendEmailNotification>? handler = null,
        TimeProvider? timeProvider = null,
        IEventAggregator? eventAggregator = null,
        ILogger<EmailSender>? logger = null)
        => new(
            logger ?? NullLogger<EmailSender>.Instance,
            globalSettings,
            eventAggregator ?? Mock.Of<IEventAggregator>(),
            emailSenderClient,
            null,
            handler,
            timeProvider ?? TimeProvider.System);
}
