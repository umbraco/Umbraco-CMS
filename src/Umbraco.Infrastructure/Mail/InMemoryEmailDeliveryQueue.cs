using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;

namespace Umbraco.Cms.Infrastructure.Mail;

/// <summary>
///     The default <see cref="IEmailDeliveryQueue"/>, which holds queued emails in memory until
///     <see cref="EmailDeliveryHostedService"/> delivers them.
/// </summary>
/// <remarks>
///     Accepts an email only while the queue holds fewer than <see cref="SmtpSettings.BackgroundQueueCapacity"/> emails,
///     and while the attachments it holds stay within <see cref="SmtpSettings.BackgroundQueueMaximumAttachmentMegabytes"/>,
///     so the memory the queue uses is bounded.
/// </remarks>
internal sealed class InMemoryEmailDeliveryQueue : IEmailDeliveryQueue
{
    private const long BytesPerMegabyte = 1024 * 1024;

    private static readonly SmtpSettings _defaultSmtpSettings = new();

    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private readonly Channel<QueuedEmail> _channel;
    private long _queuedAttachmentBytes;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InMemoryEmailDeliveryQueue"/> class.
    /// </summary>
    /// <param name="globalSettings">The global settings, including the queue's limits.</param>
    public InMemoryEmailDeliveryQueue(IOptionsMonitor<GlobalSettings> globalSettings)
    {
        _globalSettings = globalSettings;
        var capacity = Math.Max(1, (globalSettings.CurrentValue.Smtp ?? _defaultSmtpSettings).BackgroundQueueCapacity);
        _channel = Channel.CreateBounded<QueuedEmail>(new BoundedChannelOptions(capacity) { SingleReader = true });
    }

    /// <summary>
    ///     Gets the reader for the queued emails.
    /// </summary>
    internal ChannelReader<QueuedEmail> Reader => _channel.Reader;

    /// <summary>
    ///     Gets the total size, in bytes, of the attachments of the emails waiting to be delivered.
    /// </summary>
    internal long QueuedAttachmentBytes => Interlocked.Read(ref _queuedAttachmentBytes);

    /// <inheritdoc />
    public async Task<bool> TryEnqueueAsync(EmailMessage message, string emailType, TimeSpan? expires, CancellationToken cancellationToken = default)
    {
        long attachmentBytes = 0;
        if (message.HasAttachments)
        {
            if (TryGetAttachmentBytes(message, out attachmentBytes) is false || TryReserve(attachmentBytes) is false)
            {
                return false;
            }

            try
            {
                message = await CopyAttachmentsAsync(message, cancellationToken);
            }
            catch
            {
                Release(attachmentBytes);
                throw;
            }
        }

        if (_channel.Writer.TryWrite(new QueuedEmail(message, emailType, expires, attachmentBytes)))
        {
            return true;
        }

        Release(attachmentBytes);
        return false;
    }

    /// <summary>
    ///     Frees the attachment allowance held by an email once it has been delivered, or has failed to be.
    /// </summary>
    /// <param name="email">The email that is no longer queued.</param>
    internal void Release(QueuedEmail email) => Release(email.AttachmentBytes);

    // The size of an attachment is only known in advance when its stream can report its length; one that can't is
    // not queued, as copying it to find out could exceed the limit before the check could be made.
    private static bool TryGetAttachmentBytes(EmailMessage message, out long attachmentBytes)
    {
        attachmentBytes = 0;
        foreach (EmailMessageAttachment attachment in message.Attachments!)
        {
            if (attachment.Stream.CanSeek is false)
            {
                return false;
            }

            attachmentBytes += attachment.Stream.Length - attachment.Stream.Position;
        }

        return true;
    }

    private static async Task<EmailMessage> CopyAttachmentsAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var attachments = new List<EmailMessageAttachment>();
        foreach (EmailMessageAttachment attachment in message.Attachments!)
        {
            var stream = new MemoryStream();
            await attachment.Stream.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;
            attachments.Add(new EmailMessageAttachment(stream, attachment.FileName));
        }

        return new EmailMessage(
            message.From,
            message.To,
            message.Cc,
            message.Bcc,
            message.ReplyTo,
            message.Subject,
            message.Body,
            message.IsBodyHtml,
            attachments);
    }

    private bool TryReserve(long attachmentBytes)
    {
        var limit = Math.Max(0, (_globalSettings.CurrentValue.Smtp ?? _defaultSmtpSettings).BackgroundQueueMaximumAttachmentMegabytes)
                    * BytesPerMegabyte;
        while (true)
        {
            var current = Interlocked.Read(ref _queuedAttachmentBytes);
            if (current + attachmentBytes > limit)
            {
                return false;
            }

            if (Interlocked.CompareExchange(ref _queuedAttachmentBytes, current + attachmentBytes, current) == current)
            {
                return true;
            }
        }
    }

    private void Release(long attachmentBytes)
    {
        if (attachmentBytes > 0)
        {
            Interlocked.Add(ref _queuedAttachmentBytes, -attachmentBytes);
        }
    }

    /// <summary>
    ///     An email waiting for delivery.
    /// </summary>
    /// <param name="Message">The email to deliver.</param>
    /// <param name="EmailType">The type of email, for diagnostics.</param>
    /// <param name="Expires">An optional time after which the email expires.</param>
    /// <param name="AttachmentBytes">The size of the email's attachments, held against the queue's limit.</param>
    internal sealed record QueuedEmail(EmailMessage Message, string EmailType, TimeSpan? Expires, long AttachmentBytes);
}
