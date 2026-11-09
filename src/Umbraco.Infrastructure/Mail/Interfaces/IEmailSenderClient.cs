using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Infrastructure.Mail.Interfaces
{
    /// <summary>
    /// Client for sending an email from a MimeMessage.
    /// </summary>
    public interface IEmailSenderClient
    {
        /// <summary>
        /// Sends the email message with an expiration date.
        /// </summary>
        /// <param name="message">The <see cref="EmailMessage"/> to send.</param>
        /// <param name="expires">An optional time for expiry.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
        public Task SendAsync(EmailMessage message, TimeSpan? expires);

        /// <summary>
        /// Verifies that the underlying transport can currently be reached, without sending a message.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that completes when the transport is reachable.</returns>
        /// <exception cref="Exception">Thrown when the transport cannot be reached.</exception>
        public Task VerifyConnectionAsync(CancellationToken cancellationToken = default);
    }
}
