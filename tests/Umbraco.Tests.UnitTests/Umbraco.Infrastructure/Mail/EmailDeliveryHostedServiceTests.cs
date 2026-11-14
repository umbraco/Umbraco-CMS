using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Mail;

[TestFixture]
public class EmailDeliveryHostedServiceTests
{
    [Test]
    public async Task Delivers_Queued_Emails_In_The_Order_They_Were_Queued()
    {
        var queue = CreateQueue();
        var delivered = new List<string?>();
        var allDelivered = new TaskCompletionSource();
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<TimeSpan?>()))
            .Returns((EmailMessage message, TimeSpan? _) =>
            {
                delivered.Add(message.Subject);
                if (delivered.Count == 3)
                {
                    allDelivered.SetResult();
                }

                return Task.CompletedTask;
            });
        var service = CreateService(queue, clientMock.Object);

        await service.StartAsync(CancellationToken.None);
        await queue.TryEnqueueAsync(CreateMessage("First"), "Test", null);
        await queue.TryEnqueueAsync(CreateMessage("Second"), "Test", null);
        await queue.TryEnqueueAsync(CreateMessage("Third"), "Test", null);
        await allDelivered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await service.StopAsync(CancellationToken.None);

        Assert.AreEqual(new[] { "First", "Second", "Third" }, delivered);
    }

    [Test]
    public async Task Continues_Delivering_After_A_Delivery_Fails()
    {
        var queue = CreateQueue();
        var secondDelivered = new TaskCompletionSource();
        var clientMock = new Mock<IEmailSenderClient>();
        clientMock
            .Setup(x => x.SendAsync(It.Is<EmailMessage>(m => m.Subject == "First"), It.IsAny<TimeSpan?>()))
            .ThrowsAsync(new SocketException());
        clientMock
            .Setup(x => x.SendAsync(It.Is<EmailMessage>(m => m.Subject == "Second"), It.IsAny<TimeSpan?>()))
            .Returns(() =>
            {
                secondDelivered.SetResult();
                return Task.CompletedTask;
            });
        var service = CreateService(queue, clientMock.Object);

        await service.StartAsync(CancellationToken.None);
        await queue.TryEnqueueAsync(CreateMessage("First"), "Test", null);
        await queue.TryEnqueueAsync(CreateMessage("Second"), "Test", null);

        Assert.DoesNotThrowAsync(async () => await secondDelivered.Task.WaitAsync(TimeSpan.FromSeconds(10)));
        await service.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task Delivers_Emails_Still_Queued_When_Stopping()
    {
        var queue = CreateQueue();
        var clientMock = new Mock<IEmailSenderClient>();
        var service = CreateService(queue, clientMock.Object);
        var expires = TimeSpan.FromHours(1);

        await queue.TryEnqueueAsync(CreateMessage("First"), "Test", expires);
        await queue.TryEnqueueAsync(CreateMessage("Second"), "Test", null);
        await service.StopAsync(CancellationToken.None);

        clientMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.Subject == "First"), expires), Times.Once);
        clientMock.Verify(x => x.SendAsync(It.Is<EmailMessage>(m => m.Subject == "Second"), null), Times.Once);
    }

    [Test]
    public async Task Does_Not_Deliver_When_The_Queue_Has_Been_Replaced()
    {
        var clientMock = new Mock<IEmailSenderClient>();
        var service = CreateService(Mock.Of<IEmailDeliveryQueue>(), clientMock.Object);

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        clientMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Test]
    public async Task Releases_The_Attachment_Allowance_Of_An_Email_Once_It_Has_Been_Sent()
    {
        var queue = CreateQueue(maximumAttachmentMegabytes: 1);
        var service = CreateService(queue, Mock.Of<IEmailSenderClient>());

        await service.StartAsync(CancellationToken.None);
        Assert.IsTrue(await queue.TryEnqueueAsync(CreateMessageWithAttachment("First", 1024 * 1024), "Test", null));
        await WaitUntilAsync(() => queue.QueuedAttachmentBytes == 0);
        var acceptedAfterRelease = await queue.TryEnqueueAsync(CreateMessageWithAttachment("Second", 1024 * 1024), "Test", null);
        await service.StopAsync(CancellationToken.None);

        Assert.IsTrue(acceptedAfterRelease);
    }

    private static EmailDeliveryHostedService CreateService(IEmailDeliveryQueue queue, IEmailSenderClient emailSenderClient)
        => new(queue, emailSenderClient, NullLogger<EmailDeliveryHostedService>.Instance);

    private static EmailMessage CreateMessage(string subject) => new("from@test.com", "to@test.com", subject, "Body", false);

    private static EmailMessage CreateMessageWithAttachment(string subject, int attachmentBytes)
        => new(
            "from@test.com",
            ["to@test.com"],
            null,
            null,
            null,
            subject,
            "Body",
            false,
            [new EmailMessageAttachment(new MemoryStream(new byte[attachmentBytes]), "file.bin")]);

    private static InMemoryEmailDeliveryQueue CreateQueue(int maximumAttachmentMegabytes = 50)
        => new(new TestOptionsMonitor<GlobalSettings>(new GlobalSettings
        {
            Smtp = new SmtpSettings { BackgroundQueueMaximumAttachmentMegabytes = maximumAttachmentMegabytes },
        }));

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (condition() is false)
        {
            await Task.Delay(10, timeout.Token);
        }
    }
}
