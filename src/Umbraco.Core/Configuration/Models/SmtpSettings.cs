// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Umbraco.Cms.Core.Configuration.Models.Validation;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Matches MailKit.Security.SecureSocketOptions and defined locally to avoid having to take
///     a dependency on this external library into Umbraco.Core.
/// </summary>
/// <seealso href="http://www.mimekit.net/docs/html/T_MailKit_Security_SecureSocketOptions.htm" />
public enum SecureSocketOptions
{
    /// <summary>
    ///     No SSL or TLS encryption should be used.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Allow the IMailService to decide which SSL or TLS options to use (default). If the server does not support SSL or
    ///     TLS, then the connection will continue without any encryption.
    /// </summary>
    Auto = 1,

    /// <summary>
    ///     The connection should use SSL or TLS encryption immediately.
    /// </summary>
    SslOnConnect = 2,

    /// <summary>
    ///     Elevates the connection to use TLS encryption immediately after reading the greeting and capabilities of the
    ///     server. If the server does not support the STARTTLS extension, then the connection will fail and a
    ///     NotSupportedException will be thrown.
    /// </summary>
    StartTls = 3,

    /// <summary>
    ///     Elevates the connection to use TLS encryption immediately after reading the greeting and capabilities of the
    ///     server, but only if the server supports the STARTTLS extension.
    /// </summary>
    StartTlsWhenAvailable = 4,
}

/// <summary>
///     Typed configuration options for SMTP settings.
/// </summary>
public class SmtpSettings : ValidatableEntryBase
{
    /// <summary>
    ///     The default secure socket options for SMTP.
    /// </summary>
    internal const string StaticSecureSocketOptions = "Auto";

    /// <summary>
    ///     The default SMTP delivery method.
    /// </summary>
    internal const string StaticDeliveryMethod = "Network";

    /// <summary>
    ///     The default value for whether emails are sent in the background.
    /// </summary>
    internal const bool StaticSendInBackground = false;

    /// <summary>
    ///     The default maximum number of emails waiting to be sent in the background.
    /// </summary>
    internal const int StaticBackgroundQueueCapacity = 1000;

    /// <summary>
    ///     The default maximum total size, in megabytes, of the attachments of emails waiting to be sent in the background.
    /// </summary>
    internal const int StaticBackgroundQueueMaximumAttachmentMegabytes = 50;

    /// <summary>
    ///     Gets or sets a value for the SMTP from address to use for messages.
    /// </summary>
    [Required]
    [EmailAddress]
    public string From { get; set; } = null!;

    /// <summary>
    ///     Gets or sets a value for the SMTP host.
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    ///     Gets or sets a value for the SMTP port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    ///     Gets or sets a value for the secure socket options.
    /// </summary>
    [DefaultValue(StaticSecureSocketOptions)]
    public SecureSocketOptions SecureSocketOptions { get; set; } = Enum.Parse<SecureSocketOptions>(StaticSecureSocketOptions);

    /// <summary>
    ///     Gets or sets a value for the SMTP pick-up directory.
    /// </summary>
    public string? PickupDirectoryLocation { get; set; }

    /// <summary>
    ///     Gets or sets a value for the SMTP delivery method.
    /// </summary>
    [DefaultValue(StaticDeliveryMethod)]
    public SmtpDeliveryMethod DeliveryMethod { get; set; } = Enum.Parse<SmtpDeliveryMethod>(StaticDeliveryMethod);

    /// <summary>
    ///     Gets or sets a value for the SMTP user name.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    ///     Gets or sets a value for the SMTP password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    ///     Gets or sets a value for the time until an email expires.
    /// </summary>
    public TimeSpan? EmailExpiration { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether emails are sent through the SMTP client in the background, rather than
    ///     while the sender waits.
    /// </summary>
    /// <remarks>
    ///     When enabled, sending an email completes once the email is queued for delivery, so a delivery failure is logged
    ///     rather than reported to the sender. Queued emails that have not been delivered when the application stops are lost.
    ///     Delivery runs outside the request that sent the email, so a custom SMTP client cannot rely on the current request
    ///     or on scoped services from it.
    /// </remarks>
    // TODO (V20): Consider enabling this by default.
    [DefaultValue(StaticSendInBackground)]
    public bool SendInBackground { get; set; } = StaticSendInBackground;

    /// <summary>
    ///     Gets or sets the maximum number of emails that can wait to be sent in the background.
    /// </summary>
    /// <remarks>
    ///     Applies when <see cref="SendInBackground"/> is enabled and the default queue is used. When the queue is full, an
    ///     email is sent while the sender waits instead. Read when the application starts, so a change needs a restart.
    ///     Values below 1 are treated as 1.
    /// </remarks>
    [DefaultValue(StaticBackgroundQueueCapacity)]
    public int BackgroundQueueCapacity { get; set; } = StaticBackgroundQueueCapacity;

    /// <summary>
    ///     Gets or sets the maximum total size, in megabytes, of the attachments of emails waiting to be sent in the background.
    /// </summary>
    /// <remarks>
    ///     Applies when <see cref="SendInBackground"/> is enabled and the default queue is used, which holds a copy of each
    ///     queued email's attachments in memory. An email whose attachments would take the total over this limit is sent
    ///     while the sender waits instead, as is one with an attachment whose size cannot be determined in advance. Set to
    ///     0 to send every email with attachments while the sender waits.
    /// </remarks>
    [DefaultValue(StaticBackgroundQueueMaximumAttachmentMegabytes)]
    public int BackgroundQueueMaximumAttachmentMegabytes { get; set; } = StaticBackgroundQueueMaximumAttachmentMegabytes;
}
