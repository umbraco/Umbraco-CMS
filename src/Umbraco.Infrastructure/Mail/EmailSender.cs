// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net.Mail;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.IO;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Extensions;
using SecureSocketOptions = MailKit.Security.SecureSocketOptions;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Umbraco.Cms.Infrastructure.Mail;

/// <summary>
///     A utility class for sending emails
/// </summary>
public class EmailSender : IEmailSender
{
    // TODO: This should encapsulate a BackgroundTaskRunner with a queue to send these emails!
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<EmailSender> _logger;
    private readonly bool _notificationHandlerRegistered;
    private GlobalSettings _globalSettings;

    public EmailSender(
        ILogger<EmailSender> logger,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IEventAggregator eventAggregator)
        : this(logger, globalSettings, eventAggregator, null, null)
    {
    }

    public EmailSender(
        ILogger<EmailSender> logger,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IEventAggregator eventAggregator,
        INotificationHandler<SendEmailNotification>? handler1,
        INotificationAsyncHandler<SendEmailNotification>? handler2)
    {
        _logger = logger;
        _eventAggregator = eventAggregator;
        _globalSettings = globalSettings.CurrentValue;
        _notificationHandlerRegistered = handler1 is not null || handler2 is not null;
        globalSettings.OnChange(x => _globalSettings = x);
    }

    /// <summary>
    ///     Sends the message async
    /// </summary>
    /// <returns></returns>
    public async Task SendAsync(EmailMessage message, string emailType) =>
        await SendAsyncInternal(message, emailType, false);

    public async Task SendAsync(EmailMessage message, string emailType, bool enableNotification) =>
        await SendAsyncInternal(message, emailType, enableNotification);

    /// <summary>
    ///     Returns true if the application should be able to send a required application email
    /// </summary>
    /// <remarks>
    ///     We assume this is possible if either an event handler is registered or an smtp server is configured
    ///     or a pickup directory location is configured
    /// </remarks>
    public bool CanSendRequiredEmail() => _globalSettings.IsSmtpServerConfigured
                                          || _globalSettings.IsPickupDirectoryLocationConfigured
                                          || _notificationHandlerRegistered;

    private async Task SendAsyncInternal(EmailMessage message, string emailType, bool enableNotification)
    {
        if (enableNotification)
        {
            var notification =
                new SendEmailNotification(message.ToNotificationEmail(_globalSettings.Smtp?.From), emailType);
            await _eventAggregator.PublishAsync(notification);

            // if a handler handled sending the email then don't continue.
            if (notification.IsHandled)
            {
                _logger.LogDebug(
                    "The email sending for {Subject} was handled by a notification handler",
                    notification.Message.Subject);
                return;
            }
        }

        if (!_globalSettings.IsSmtpServerConfigured && !_globalSettings.IsPickupDirectoryLocationConfigured)
        {
            _logger.LogDebug(
                "Could not send email for {Subject}. It was not handled by a notification handler and there is no SMTP configured.",
                message.Subject);
            return;
        }

        if (_globalSettings.IsPickupDirectoryLocationConfigured &&
            !string.IsNullOrWhiteSpace(_globalSettings.Smtp?.From))
        {
            // The following code snippet is the recommended way to handle PickupDirectoryLocation.
            // See more https://github.com/jstedfast/MailKit/blob/master/FAQ.md#q-how-can-i-send-email-to-a-specifiedpickupdirectory
            do
            {
                var path = Path.Combine(_globalSettings.Smtp.PickupDirectoryLocation!, Guid.NewGuid() + ".eml");
                Stream stream;

                try
                {
                    stream = File.Open(path, FileMode.CreateNew);
                }
                catch (IOException)
                {
                    if (File.Exists(path))
                    {
                        continue;
                    }

                    throw;
                }

                try
                {
                    using (stream)
                    {
                        using var filtered = new FilteredStream(stream);
                        filtered.Add(new SmtpDataFilter());

                        FormatOptions options = FormatOptions.Default.Clone();
                        options.NewLineFormat = NewLineFormat.Dos;

                        await message.ToMimeMessage(_globalSettings.Smtp.From).WriteToAsync(options, filtered);
                        filtered.Flush();
                        return;
                    }
                }
                catch
                {
                    File.Delete(path);
                    throw;
                }
            }
            while (true);
        }

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

        var mailMessage = message.ToMimeMessage(_globalSettings.Smtp.From);
        if (_globalSettings.Smtp.DeliveryMethod == SmtpDeliveryMethod.Network)
        {
            await client.SendAsync(mailMessage);
        }
        else
        {
            client.Send(mailMessage);
        }

        await client.DisconnectAsync(true);
    }
}
