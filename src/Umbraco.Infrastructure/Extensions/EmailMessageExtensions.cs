using MimeKit;
using MimeKit.Text;
using Umbraco.Cms.Core.Models.Email;

namespace Umbraco.Cms.Infrastructure.Extensions;

internal static class EmailMessageExtensions
{
    /// <summary>
    /// Converts an <see cref="EmailMessage"/> to a <see cref="MimeMessage"/>, including all recipients, subject, body, and attachments.
    /// If the <paramref name="mailMessage"/> does not specify a From address, the <paramref name="configuredFromAddress"/> is used as the sender.
    /// </summary>
    /// <param name="mailMessage">The email message to convert.</param>
    /// <param name="configuredFromAddress">The default sender address to use if <paramref name="mailMessage"/> does not specify one.</param>
    /// <returns>
    /// A <see cref="MimeMessage"/> representing the converted email message, including all recipients, subject, body (as HTML or plain text), and any attachments.
    /// </returns>
    public static MimeMessage ToMimeMessage(this EmailMessage mailMessage, string configuredFromAddress)
    {
        var fromEmail = string.IsNullOrEmpty(mailMessage.From) ? configuredFromAddress : mailMessage.From;

        if (!InternetAddress.TryParse(fromEmail, out InternetAddress fromAddress))
        {
            throw new ArgumentException(
                $"Email could not be sent.  Could not parse from address {fromEmail} as a valid email address.");
        }

        var messageToSend = new MimeMessage { From = { fromAddress }, Subject = mailMessage.Subject };

        AddAddresses(messageToSend, mailMessage.To, x => x.To, true);
        AddAddresses(messageToSend, mailMessage.Cc, x => x.Cc);
        AddAddresses(messageToSend, mailMessage.Bcc, x => x.Bcc);
        AddAddresses(messageToSend, mailMessage.ReplyTo, x => x.ReplyTo);

        if (mailMessage.HasAttachments)
        {
            var builder = new BodyBuilder();
            if (mailMessage.IsBodyHtml)
            {
                builder.HtmlBody = mailMessage.Body;
            }
            else
            {
                builder.TextBody = mailMessage.Body;
            }

            foreach (EmailMessageAttachment attachment in mailMessage.Attachments!)
            {
                builder.Attachments.Add(attachment.FileName, attachment.Stream);
            }

            messageToSend.Body = builder.ToMessageBody();
        }
        else
        {
            messageToSend.Body =
                new TextPart(mailMessage.IsBodyHtml ? TextFormat.Html : TextFormat.Plain) { Text = mailMessage.Body };
        }

        return messageToSend;
    }

    /// <summary>
    /// Converts an <see cref="EmailMessage"/> to a <see cref="NotificationEmailModel"/>.
    /// If the <paramref name="emailMessage"/>'s from address is not set or is empty, the <paramref name="configuredFromAddress"/> is used as the sender address.
    /// </summary>
    /// <param name="emailMessage">The email message to convert.</param>
    /// <param name="configuredFromAddress">The fallback sender address to use if the email message's from address is not specified.</param>
    /// <returns>
    /// A <see cref="NotificationEmailModel"/> representing the notification email, with all recipients, subject, body, attachments, and sender address appropriately set.
    /// </returns>
    public static NotificationEmailModel ToNotificationEmail(
        this EmailMessage emailMessage,
        string? configuredFromAddress)
    {
        var fromEmail = string.IsNullOrEmpty(emailMessage.From) ? configuredFromAddress : emailMessage.From;

        NotificationEmailAddress? from = ToNotificationAddress(fromEmail);

        return new NotificationEmailModel(
            from,
            GetNotificationAddresses(emailMessage.To),
            GetNotificationAddresses(emailMessage.Cc),
            GetNotificationAddresses(emailMessage.Bcc),
            GetNotificationAddresses(emailMessage.ReplyTo),
            emailMessage.Subject,
            emailMessage.Body,
            emailMessage.Attachments,
            emailMessage.IsBodyHtml);
    }

    private static void AddAddresses(MimeMessage message, string?[]? addresses, Func<MimeMessage, InternetAddressList> addressListGetter, bool throwIfNoneValid = false)
    {
        var foundValid = false;
        if (addresses != null)
        {
            foreach (var address in addresses)
            {
                if (InternetAddress.TryParse(address, out InternetAddress internetAddress))
                {
                    addressListGetter(message).Add(internetAddress);
                    foundValid = true;
                }
            }
        }

        if (throwIfNoneValid && foundValid == false)
        {
            throw new InvalidOperationException("Email could not be sent. Could not parse a valid recipient address.");
        }
    }

    private static NotificationEmailAddress? ToNotificationAddress(string? address)
    {
        if (InternetAddress.TryParse(address, out InternetAddress internetAddress))
        {
            if (internetAddress is MailboxAddress mailboxAddress)
            {
                return new NotificationEmailAddress(mailboxAddress.Address, internetAddress.Name);
            }
        }

        return null;
    }

    private static IEnumerable<NotificationEmailAddress>? GetNotificationAddresses(IEnumerable<string?>? addresses)
    {
        if (addresses is null)
        {
            return null;
        }

        var notificationAddresses = new List<NotificationEmailAddress>();

        foreach (var address in addresses)
        {
            NotificationEmailAddress? notificationAddress = ToNotificationAddress(address);
            if (notificationAddress is not null)
            {
                notificationAddresses.Add(notificationAddress);
            }
        }

        return notificationAddresses;
    }
}
