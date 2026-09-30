// Copyright (c) Umbraco.
// See LICENSE for more details.

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
using Umbraco.Cms.Infrastructure.Mail.Interfaces;

namespace Umbraco.Cms.Infrastructure.Mail;

/// <summary>
///     A utility class for sending emails.
/// </summary>
public class EmailSender : IEmailSender
{
    // TODO: This should encapsulate a BackgroundTaskRunner with a queue to send these emails!
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<EmailSender> _logger;
    private readonly bool _notificationHandlerRegistered;
    private GlobalSettings _globalSettings;
    private readonly IEmailSenderClient _emailSenderClient;
    private readonly TimeProvider _timeProvider;
    private readonly Lock _smtpProbeLock = new();
    private Task<bool>? _smtpProbe;
    private DateTimeOffset _smtpProbeExpiry;

    /// <summary>
    /// The maximum time the SMTP probe may take before the server is treated as unavailable.
    /// </summary>
    internal static readonly TimeSpan SmtpProbeTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How long a successful SMTP probe result is reused before the server is probed again.
    /// </summary>
    internal static readonly TimeSpan SmtpProbeAvailableCacheDuration = TimeSpan.FromSeconds(60);

    /// <summary>
    /// How long a failed SMTP probe result is reused before the server is probed again, kept short so recovery is noticed quickly.
    /// </summary>
    internal static readonly TimeSpan SmtpProbeUnavailableCacheDuration = TimeSpan.FromSeconds(15);

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSender"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="globalSettings">The global settings, including the SMTP configuration.</param>
    /// <param name="eventAggregator">The event aggregator used to publish <see cref="SendEmailNotification"/>.</param>
    /// <param name="emailSenderClient">The client used to send emails and verify the SMTP connection.</param>
    /// <param name="handler1">An optional synchronous handler for <see cref="SendEmailNotification"/>.</param>
    /// <param name="handler2">An optional asynchronous handler for <see cref="SendEmailNotification"/>.</param>
    public EmailSender(
        ILogger<EmailSender> logger,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IEventAggregator eventAggregator,
        IEmailSenderClient emailSenderClient,
        INotificationHandler<SendEmailNotification>? handler1,
        INotificationAsyncHandler<SendEmailNotification>? handler2)
        : this(logger, globalSettings, eventAggregator, emailSenderClient, handler1, handler2, TimeProvider.System)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailSender"/> class using the specified time provider.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="globalSettings">The global settings, including the SMTP configuration.</param>
    /// <param name="eventAggregator">The event aggregator used to publish <see cref="SendEmailNotification"/>.</param>
    /// <param name="emailSenderClient">The client used to send emails and verify the SMTP connection.</param>
    /// <param name="handler1">An optional synchronous handler for <see cref="SendEmailNotification"/>.</param>
    /// <param name="handler2">An optional asynchronous handler for <see cref="SendEmailNotification"/>.</param>
    /// <param name="timeProvider">The time provider used for the SMTP probe cache expiry and timeout.</param>
    /// <remarks>
    /// This constructor is internal because the time provider only exists so tests can control the SMTP probe cache
    /// expiry and timeout. Production code uses <see cref="TimeProvider.System"/> via the public constructor, so exposing
    /// this would add public API surface without a use case.
    /// </remarks>
    internal EmailSender(
        ILogger<EmailSender> logger,
        IOptionsMonitor<GlobalSettings> globalSettings,
        IEventAggregator eventAggregator,
        IEmailSenderClient emailSenderClient,
        INotificationHandler<SendEmailNotification>? handler1,
        INotificationAsyncHandler<SendEmailNotification>? handler2,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _eventAggregator = eventAggregator;
        _globalSettings = globalSettings.CurrentValue;
        _notificationHandlerRegistered = handler1 is not null || handler2 is not null;
        _emailSenderClient = emailSenderClient;
        _timeProvider = timeProvider;
        globalSettings.OnChange(x =>
        {
            lock (_smtpProbeLock)
            {
                _globalSettings = x;
                _smtpProbe = null;
            }
        });
    }

    /// <inheritdoc/>
    public async Task SendAsync(EmailMessage message, string emailType) =>
        await SendAsyncInternal(message, emailType, false, null);

    /// <inheritdoc/>
    public async Task SendAsync(EmailMessage message, string emailType, bool enableNotification) =>
        await SendAsyncInternal(message, emailType, enableNotification, null);

    /// <inheritdoc/>
    public async Task SendAsync(EmailMessage message, string emailType, bool enableNotification = false, TimeSpan? expires = null) =>
        await SendAsyncInternal(message, emailType, enableNotification, expires);

    /// <inheritdoc/>
    [Obsolete("Please use IsEmailConfigured to check configuration only, or IsEmailAvailableAsync to also check that the transport can currently be reached. Scheduled for removal in Umbraco 19.")]
    public bool CanSendRequiredEmail() => IsEmailConfigured();

    /// <inheritdoc/>
    /// <remarks>
    ///     We assume this is possible if either an event handler is registered or an smtp server is configured
    ///     or a pickup directory location is configured.
    /// </remarks>
    public bool IsEmailConfigured() => _globalSettings.IsSmtpServerConfigured
                                       || _globalSettings.IsPickupDirectoryLocationConfigured
                                       || _notificationHandlerRegistered;

    /// <inheritdoc/>
    /// <remarks>
    ///     Only the SMTP transport is probed. A pickup directory only requires a local file write, so it is assumed to be
    ///     available. A registered notification handler cannot be probed, and may handle only some email types, so SMTP is
    ///     still probed when it is configured as the fallback; a handler alone is assumed to be available.
    ///     Concurrent callers share a single probe, and its result is cached briefly, so the SMTP server is contacted at
    ///     most once per cache period regardless of how often this is called. The cancellation token only stops the
    ///     caller waiting; it does not cancel the shared probe.
    /// </remarks>
    public async Task<bool> IsEmailAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (UsesSmtpTransport() is false)
        {
            return IsEmailConfigured();
        }

        return await GetOrStartSmtpProbe().WaitAsync(cancellationToken);
    }

    private Task<bool> GetOrStartSmtpProbe()
    {
        lock (_smtpProbeLock)
        {
            if (_smtpProbe is null || (_smtpProbe.IsCompleted && _timeProvider.GetUtcNow() >= _smtpProbeExpiry))
            {
                _smtpProbe = ProbeSmtpAsync();
            }

            return _smtpProbe;
        }
    }

    private async Task<bool> ProbeSmtpAsync()
    {
        bool isAvailable;
        using (var timeout = new CancellationTokenSource(SmtpProbeTimeout, _timeProvider))
        {
            try
            {
                await _emailSenderClient.VerifyConnectionAsync(timeout.Token);
                isAvailable = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not connect to the SMTP server at {SmtpHost}:{SmtpPort}.",
                    _globalSettings.Smtp?.Host,
                    _globalSettings.Smtp?.Port);
                isAvailable = false;
            }
        }

        lock (_smtpProbeLock)
        {
            _smtpProbeExpiry = _timeProvider.GetUtcNow()
                               + (isAvailable ? SmtpProbeAvailableCacheDuration : SmtpProbeUnavailableCacheDuration);
        }

        return isAvailable;
    }

    // Mirrors SendAsyncInternal: a pickup directory is only used when a From address is set, otherwise sending falls back to SMTP.
    private bool UsesSmtpTransport()
        => _globalSettings.IsSmtpServerConfigured
           && (_globalSettings.IsPickupDirectoryLocationConfigured is false || string.IsNullOrWhiteSpace(_globalSettings.Smtp?.From));

    private async Task SendAsyncInternal(EmailMessage message, string emailType, bool enableNotification, TimeSpan? expires)
    {
        if (enableNotification)
        {
            var notification =
                new SendEmailNotification(message.ToNotificationEmail(_globalSettings.Smtp?.From), emailType);
            await _eventAggregator.PublishAsync(notification);

            // if a handler handled sending the email then don't continue.
            if (notification.IsHandled)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(
                    "The email sending for {Subject} was handled by a notification handler",
                    notification.Message.Subject);
                }
                return;
            }
        }

        if (!_globalSettings.IsSmtpServerConfigured && !_globalSettings.IsPickupDirectoryLocationConfigured)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                "Could not send email for {Subject}. It was not handled by a notification handler and there is no SMTP configured.",
                message.Subject);
            }
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

        await _emailSenderClient.SendAsync(message, expires);
    }
}
