using System;
using MimeKit;
using MimeKit.Text;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Extensions
{
    internal static class EmailMessageExtensions
    {
        public static MimeMessage ToMimeMessage(this EmailMessage mailMessage, string configuredFromAddress)
        {
            var fromEmail = mailMessage.From;
            if (string.IsNullOrEmpty(fromEmail))
            {
                fromEmail = configuredFromAddress;
            }

            if (!InternetAddress.TryParse(mailMessage.From, out InternetAddress fromAddress))
            {
                throw new ArgumentException($"Email could not be sent.  Could not parse from address {fromEmail} as a valid email address.");
            }

            var messageToSend = new MimeMessage
            {
                From = { fromAddress },
                Subject = mailMessage.Subject,
            };

            AddAddresses(messageToSend, mailMessage.To, x => x.To, throwIfNoneValid: true);
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

                foreach (EmailMessageAttachment attachment in mailMessage.Attachments)
                {
                    builder.Attachments.Add(attachment.FileName, attachment.Stream);
                }

                messageToSend.Body = builder.ToMessageBody();
            }
            else
            {
                messageToSend.Body = new TextPart(mailMessage.IsBodyHtml ? TextFormat.Html : TextFormat.Plain) { Text = mailMessage.Body };
            }

            return messageToSend;
        }

        private static void AddAddresses(MimeMessage message, string[] addresses, Func<MimeMessage, InternetAddressList> addressListGetter, bool throwIfNoneValid = false)
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
                throw new InvalidOperationException($"Email could not be sent. Could not parse a valid recipient address.");
            }
        }
    }
}
