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
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;

        /// <inheritdoc />
        public BasicSmtpEmailSenderClient(IOptionsMonitor<GlobalSettings> globalSettings)
            => _globalSettings = globalSettings;

        /// <inheritdoc />
        public async Task SendAsync(EmailMessage message)
            => await SendAsync(message, null);

        /// <inheritdoc />
        public async Task SendAsync(EmailMessage message, TimeSpan? expires)
        {
            GlobalSettings globalSettings = _globalSettings.CurrentValue;
            using var client = new SmtpClient();

            await ConnectAndAuthenticateAsync(client, globalSettings, CancellationToken.None);

            var mimeMessage = message.ToMimeMessage(globalSettings.Smtp!.From);

            if (globalSettings.IsSmtpExpiryConfigured)
            {
                expires ??= globalSettings.Smtp.EmailExpiration;
            }

            if (expires.HasValue)
            {
                // `Expires` header needs to be in RFC 1123/2822 compatible format
                mimeMessage.Headers.Add("Expires", DateTimeOffset.UtcNow.Add(expires.GetValueOrDefault()).ToString("R"));
            }

            if (globalSettings.Smtp.DeliveryMethod == SmtpDeliveryMethod.Network)
            {
                await client.SendAsync(mimeMessage);
            }
            else
            {
                client.Send(mimeMessage);
            }
        }

        /// <inheritdoc />
        public async Task VerifyConnectionAsync(CancellationToken cancellationToken = default)
        {
            using var client = new SmtpClient();

            await ConnectAndAuthenticateAsync(client, _globalSettings.CurrentValue, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        private static async Task ConnectAndAuthenticateAsync(SmtpClient client, GlobalSettings globalSettings, CancellationToken cancellationToken)
        {
            await client.ConnectAsync(
                globalSettings.Smtp!.Host!,
                globalSettings.Smtp.Port,
                (SecureSocketOptions)(int)globalSettings.Smtp.SecureSocketOptions,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(globalSettings.Smtp.Username) &&
                !string.IsNullOrWhiteSpace(globalSettings.Smtp.Password))
            {
                await client.AuthenticateAsync(globalSettings.Smtp.Username, globalSettings.Smtp.Password, cancellationToken);
            }
        }
    }
}
