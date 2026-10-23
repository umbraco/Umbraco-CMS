using System.Net.Sockets;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Mail;

[TestFixture]
public class EmailDeliveryHostedServiceTests
{
    [Test]
    public async Task Delivers_Queued_Emails_In_The_Order_They_Were_Queued()
    {
        var queue = new InMemoryEmailDeliveryQueue();
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
        await queue.EnqueueAsync(CreateMessage("First"), "Test", null);
        await queue.EnqueueAsync(CreateMessage("Second"), "Test", null);
        await queue.EnqueueAsync(CreateMessage("Third"), "Test", null);
        await allDelivered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await service.StopAsync(CancellationToken.None);

        Assert.AreEqual(new[] { "First", "Second", "Third" }, delivered);
    }

    [Test]
    public async Task Continues_Delivering_After_A_Delivery_Fails()
    {
        var queue = new InMemoryEmailDeliveryQueue();
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
        await queue.EnqueueAsync(CreateMessage("First"), "Test", null);
        await queue.EnqueueAsync(CreateMessage("Second"), "Test", null);

        Assert.DoesNotThrowAsync(async () => await secondDelivered.Task.WaitAsync(TimeSpan.FromSeconds(10)));
        await service.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task Delivers_Emails_Still_Queued_When_Stopping()
    {
        var queue = new InMemoryEmailDeliveryQueue();
        var clientMock = new Mock<IEmailSenderClient>();
        var service = CreateService(queue, clientMock.Object);
        var expires = TimeSpan.FromHours(1);

        await queue.EnqueueAsync(CreateMessage("First"), "Test", expires);
        await queue.EnqueueAsync(CreateMessage("Second"), "Test", null);
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

    private static EmailDeliveryHostedService CreateService(IEmailDeliveryQueue queue, IEmailSenderClient emailSenderClient)
        => new(queue, emailSenderClient, NullLogger<EmailDeliveryHostedService>.Instance);

    private static EmailMessage CreateMessage(string subject) => new("from@test.com", "to@test.com", subject, "Body", false);
}
