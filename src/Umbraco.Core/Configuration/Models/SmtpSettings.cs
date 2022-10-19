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
/// <seealso cref="http://www.mimekit.net/docs/html/T_MailKit_Security_SecureSocketOptions.htm" />
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
    internal const string StaticSecureSocketOptions = "Auto";
    internal const string StaticDeliveryMethod = "Network";

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
    public SecureSocketOptions SecureSocketOptions { get; set; } =
        Enum<SecureSocketOptions>.Parse(StaticSecureSocketOptions);

    /// <summary>
    ///     Gets or sets a value for the SMTP pick-up directory.
    /// </summary>
    public string? PickupDirectoryLocation { get; set; }

    /// <summary>
    ///     Gets or sets a value for the SMTP delivery method.
    /// </summary>
    [DefaultValue(StaticDeliveryMethod)]
    public SmtpDeliveryMethod DeliveryMethod { get; set; } = Enum<SmtpDeliveryMethod>.Parse(StaticDeliveryMethod);

    /// <summary>
    ///     Gets or sets a value for the SMTP user name.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    ///     Gets or sets a value for the SMTP password.
    /// </summary>
    public string? Password { get; set; }
}
