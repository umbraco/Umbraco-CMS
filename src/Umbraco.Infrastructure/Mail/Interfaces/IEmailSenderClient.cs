using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Infrastructure.Mail.Interfaces
{
    /// <summary>
    /// Client for sending an email from a MimeMessage.
    /// </summary>
    public interface IEmailSenderClient
    {
        /// <summary>
        /// Sends the email message.
        /// </summary>
        /// <param name="message">The <see cref="EmailMessage"/> to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Obsolete("Please use the overload taking all parameters. Scheduled for removal in Umbraco 18.")]
        public Task SendAsync(EmailMessage message);

        /// <summary>
        /// Sends the email message with an expiration date.
        /// </summary>
        /// <param name="message">The <see cref="EmailMessage"/> to send.</param>
        /// <param name="expires">An optional time for expiry.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
        public Task SendAsync(EmailMessage message, TimeSpan? expires)
#pragma warning disable CS0618 // Type or member is obsolete
            => SendAsync(message);
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Verifies that the underlying transport can currently be reached, without sending a message.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that completes when the transport is reachable.</returns>
        /// <exception cref="Exception">Thrown when the transport cannot be reached.</exception>
        /// <remarks>
        /// The default implementation performs no check, so clients that don't override it are assumed to be reachable.
        /// </remarks>
        // TODO (V19): Remove the default implementation.
        public Task VerifyConnectionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
