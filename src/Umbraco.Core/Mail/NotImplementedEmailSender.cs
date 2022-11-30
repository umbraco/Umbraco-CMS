using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Core.Mail;

internal class NotImplementedEmailSender : IEmailSender
{
    public Task SendAsync(EmailMessage message, string emailType)
        => throw new NotImplementedException(
            "To send an Email ensure IEmailSender is implemented with a custom implementation");

    public Task SendAsync(EmailMessage message, string emailType, bool enableNotification) =>
        throw new NotImplementedException(
            "To send an Email ensure IEmailSender is implemented with a custom implementation");

    public bool CanSendRequiredEmail()
        => throw new NotImplementedException(
            "To send an Email ensure IEmailSender is implemented with a custom implementation");
}
