using System.Net.Mail;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;
using SecureSocketOptions = MailKit.Security.SecureSocketOptions;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Umbraco.Cms.Infrastructure.Mail
{
    /// <summary>
    ///    A basic SMTP email sender client using MailKits SMTP client.
    /// </summary>
    public class BasicSmtpEmailSenderClient : IEmailSenderClient
    {
        private readonly GlobalSettings _globalSettings;

        /// <inheritdoc />
        public BasicSmtpEmailSenderClient(IOptionsMonitor<GlobalSettings> globalSettings)
            => _globalSettings = globalSettings.CurrentValue;

        /// <inheritdoc />
        public async Task SendAsync(EmailMessage message)
            => await SendAsync(message, null);

        /// <inheritdoc />
        public async Task SendAsync(EmailMessage message, TimeSpan? expires)
        {
            using var client = new SmtpClient();

            await client.ConnectAsync(
                _globalSettings.Smtp!.Host,
                _globalSettings.Smtp.Port,
                (SecureSocketOptions)(int)_globalSettings.Smtp.SecureSocketOptions);

            if (!string.IsNullOrWhiteSpace(_globalSettings.Smtp.Username) &&
                !string.IsNullOrWhiteSpace(_globalSettings.Smtp.Password))
            {
                await client.AuthenticateAsync(_globalSettings.Smtp.Username, _globalSettings.Smtp.Password);
            }

            var mimeMessage = message.ToMimeMessage(_globalSettings.Smtp!.From);

            if (_globalSettings.IsSmtpExpiryConfigured)
            {
                expires ??= _globalSettings.Smtp.EmailExpiration;
            }

            if (expires.HasValue)
            {
                // `Expires` header needs to be in RFC 1123/2822 compatible format
                mimeMessage.Headers.Add("Expires", DateTimeOffset.UtcNow.Add(expires.GetValueOrDefault()).ToString("R"));
            }

            if (_globalSettings.Smtp.DeliveryMethod == SmtpDeliveryMethod.Network)
            {
                await client.SendAsync(mimeMessage);
            }
            else
            {
                client.Send(mimeMessage);
            }
        }
    }
}
