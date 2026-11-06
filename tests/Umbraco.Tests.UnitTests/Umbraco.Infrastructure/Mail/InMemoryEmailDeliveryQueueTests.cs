using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Infrastructure.Mail;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Mail;

[TestFixture]
public class InMemoryEmailDeliveryQueueTests
{
    private const int OneMegabyte = 1024 * 1024;

    [Test]
    public async Task TryEnqueueAsync_Accepts_Email_Within_The_Limits()
    {
        var queue = CreateQueue();
        var expires = TimeSpan.FromHours(1);

        Assert.IsTrue(await queue.TryEnqueueAsync(CreateMessage(), "Test", expires));

        Assert.IsTrue(queue.Reader.TryRead(out InMemoryEmailDeliveryQueue.QueuedEmail? queued));
        Assert.AreEqual("Test", queued!.EmailType);
        Assert.AreEqual(expires, queued.Expires);
    }

    [Test]
    public async Task TryEnqueueAsync_Queues_A_Copy_Of_Attachments_That_Outlives_The_Senders_Streams()
    {
        var queue = CreateQueue();
        byte[] content = "Attachment content"u8.ToArray();
        var attachmentStream = new MemoryStream(content);

        await queue.TryEnqueueAsync(CreateMessage(new EmailMessageAttachment(attachmentStream, "file.txt")), "Test", null);
        await attachmentStream.DisposeAsync();

        Assert.IsTrue(queue.Reader.TryRead(out InMemoryEmailDeliveryQueue.QueuedEmail? queued));
        EmailMessageAttachment queuedAttachment = queued!.Message.Attachments!.Single();
        using var queuedContent = new MemoryStream();
        await queuedAttachment.Stream.CopyToAsync(queuedContent);
        Assert.AreEqual("file.txt", queuedAttachment.FileName);
        CollectionAssert.AreEqual(content, queuedContent.ToArray());
    }

    [Test]
    public async Task TryEnqueueAsync_Rejects_Email_When_The_Queue_Is_Full()
    {
        var queue = CreateQueue(capacity: 1);

        Assert.IsTrue(await queue.TryEnqueueAsync(CreateMessage(), "Test", null));
        Assert.IsFalse(await queue.TryEnqueueAsync(CreateMessage(), "Test", null));
    }

    [Test]
    public async Task TryEnqueueAsync_Releases_The_Attachment_Allowance_Of_Email_Rejected_Because_The_Queue_Is_Full()
    {
        var queue = CreateQueue(capacity: 1);
        await queue.TryEnqueueAsync(CreateMessage(), "Test", null);
        using var attachmentStream = new MemoryStream(new byte[OneMegabyte]);

        Assert.IsFalse(await queue.TryEnqueueAsync(CreateMessage(new EmailMessageAttachment(attachmentStream, "file.bin")), "Test", null));

        Assert.AreEqual(0, queue.QueuedAttachmentBytes);
    }

    [Test]
    public async Task TryEnqueueAsync_Rejects_Email_Whose_Attachments_Would_Exceed_The_Attachment_Limit()
    {
        var queue = CreateQueue(maximumAttachmentMegabytes: 1);
        using var firstStream = new MemoryStream(new byte[OneMegabyte * 3 / 4]);
        using var secondStream = new MemoryStream(new byte[OneMegabyte / 2]);

        Assert.IsTrue(await queue.TryEnqueueAsync(CreateMessage(new EmailMessageAttachment(firstStream, "first.bin")), "Test", null));
        Assert.IsFalse(await queue.TryEnqueueAsync(CreateMessage(new EmailMessageAttachment(secondStream, "second.bin")), "Test", null));

        Assert.AreEqual(OneMegabyte * 3 / 4, queue.QueuedAttachmentBytes);
    }

    [Test]
    public async Task TryEnqueueAsync_Rejects_Email_With_An_Attachment_Whose_Size_Cannot_Be_Determined()
    {
        var queue = CreateQueue();
        using var attachmentStream = new NonSeekableStream("Attachment content"u8.ToArray());

        Assert.IsFalse(await queue.TryEnqueueAsync(CreateMessage(new EmailMessageAttachment(attachmentStream, "file.txt")), "Test", null));
    }

    [Test]
    public async Task TryEnqueueAsync_Accepts_Email_Without_Attachments_When_No_Attachments_May_Be_Queued()
    {
        var queue = CreateQueue(maximumAttachmentMegabytes: 0);
        using var attachmentStream = new MemoryStream(new byte[1]);

        Assert.IsTrue(await queue.TryEnqueueAsync(CreateMessage(), "Test", null));
        Assert.IsFalse(await queue.TryEnqueueAsync(CreateMessage(new EmailMessageAttachment(attachmentStream, "file.bin")), "Test", null));
    }

    [Test]
    public async Task Release_Frees_The_Attachment_Allowance_Held_By_A_Queued_Email()
    {
        var queue = CreateQueue(maximumAttachmentMegabytes: 1);
        using var firstStream = new MemoryStream(new byte[OneMegabyte]);
        using var secondStream = new MemoryStream(new byte[OneMegabyte]);
        await queue.TryEnqueueAsync(CreateMessage(new EmailMessageAttachment(firstStream, "first.bin")), "Test", null);
        queue.Reader.TryRead(out InMemoryEmailDeliveryQueue.QueuedEmail? queued);

        queue.Release(queued!);

        Assert.AreEqual(0, queue.QueuedAttachmentBytes);
        Assert.IsTrue(await queue.TryEnqueueAsync(CreateMessage(new EmailMessageAttachment(secondStream, "second.bin")), "Test", null));
    }

    private static InMemoryEmailDeliveryQueue CreateQueue(int capacity = 1000, int maximumAttachmentMegabytes = 50)
        => new(new TestOptionsMonitor<GlobalSettings>(new GlobalSettings
        {
            Smtp = new SmtpSettings
            {
                BackgroundQueueCapacity = capacity,
                BackgroundQueueMaximumAttachmentMegabytes = maximumAttachmentMegabytes,
            },
        }));

    private static EmailMessage CreateMessage(params EmailMessageAttachment[] attachments)
        => new("from@test.com", ["to@test.com"], null, null, null, "Subject", "Body", false, attachments);

    private sealed class NonSeekableStream : MemoryStream
    {
        public NonSeekableStream(byte[] buffer)
            : base(buffer)
        {
        }

        public override bool CanSeek => false;
    }
}
