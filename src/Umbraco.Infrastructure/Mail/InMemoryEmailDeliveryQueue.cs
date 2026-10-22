using System.Threading.Channels;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;

namespace Umbraco.Cms.Infrastructure.Mail;

/// <summary>
///     The default <see cref="IEmailDeliveryQueue"/>, which holds queued emails in memory until
///     <see cref="EmailDeliveryHostedService"/> delivers them.
/// </summary>
internal sealed class InMemoryEmailDeliveryQueue : IEmailDeliveryQueue
{
    /// <summary>
    ///     The maximum number of emails held at once. When the queue is full, queuing waits for space, so a burst of
    ///     emails slows the senders down rather than growing memory without limit.
    /// </summary>
    internal const int Capacity = 1000;

    private readonly Channel<QueuedEmail> _channel = Channel.CreateBounded<QueuedEmail>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });

    /// <summary>
    ///     Gets the reader for the queued emails.
    /// </summary>
    internal ChannelReader<QueuedEmail> Reader => _channel.Reader;

    /// <inheritdoc />
    public ValueTask EnqueueAsync(EmailMessage message, string emailType, TimeSpan? expires, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(new QueuedEmail(message, emailType, expires), cancellationToken);

    /// <summary>
    ///     An email waiting for delivery.
    /// </summary>
    /// <param name="Message">The email to deliver.</param>
    /// <param name="EmailType">The type of email, for diagnostics.</param>
    /// <param name="Expires">An optional time after which the email expires.</param>
    internal sealed record QueuedEmail(EmailMessage Message, string EmailType, TimeSpan? Expires);
}
