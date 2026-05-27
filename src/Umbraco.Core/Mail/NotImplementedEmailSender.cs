using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Mail;

/// <summary>
/// An <see cref="IEmailSender"/> implementation that throws <see cref="NotImplementedException"/> for all operations.
/// </summary>
/// <remarks>
/// This is the default implementation used when no custom email sender is configured.
/// To send emails, implement <see cref="IEmailSender"/> with a custom implementation.
/// </remarks>
internal sealed class NotImplementedEmailSender : IEmailSender
{
    /// <inheritdoc />
    public Task SendAsync(EmailMessage message, string emailType, bool enableNotification = false, TimeSpan? expires = null) =>
        throw new NotImplementedException(
            "To send an Email ensure IEmailSender is implemented with a custom implementation");

    /// <inheritdoc />
    public bool CanSendRequiredEmail()
        => throw new NotImplementedException(
            "To send an Email ensure IEmailSender is implemented with a custom implementation");
}
